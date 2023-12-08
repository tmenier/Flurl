using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// A Headers collection is available on IFlurlRequest, IFlurlClient, and IFlurlBuilder.
	/// This abstract class allows the same tests to be run against all 3.
	/// </summary>
	public abstract class HeadersTestsBase<T> where T : IHeadersContainer
	{
		protected abstract T CreateContainer();
		protected abstract IFlurlRequest GetRequest(T container);

		[Test]
		public void can_set_header() {
			var c = CreateContainer();
			c.WithHeader("a", 1);
			Assert.AreEqual(("a", "1"), GetRequest(c).Headers.Single());
		}

		[Test]
		public void can_set_headers_from_anon_object() {
			var c = CreateContainer();
			// null values shouldn't be added
			c.WithHeaders(new { a = "b", one = 2, three = (object)null });

			var req = GetRequest(c);
			Assert.AreEqual(2, req.Headers.Count);
			Assert.IsTrue(req.Headers.Contains("a", "b"));
			Assert.IsTrue(req.Headers.Contains("one", "2"));
		}

		[Test]
		public void can_remove_header_by_setting_null() {
			var c = CreateContainer();
			c.WithHeaders(new { a = 1, b = 2 });
			Assert.AreEqual(2, GetRequest(c).Headers.Count);

			c.WithHeader("b", null);

			var req = GetRequest(c);
			Assert.AreEqual(1, req.Headers.Count);
			Assert.IsFalse(req.Headers.Contains("b"));
		}

		[Test]
		public void can_set_headers_from_dictionary() {
			var c = CreateContainer();
			c.WithHeaders(new Dictionary<string, object> { { "a", "b" }, { "one", 2 } });

			var req = GetRequest(c);
			Assert.AreEqual(2, req.Headers.Count);
			Assert.IsTrue(req.Headers.Contains("a", "b"));
			Assert.IsTrue(req.Headers.Contains("one", "2"));
		}

		[Test]
		public void underscores_in_properties_convert_to_hyphens_in_header_names() {
			var c = CreateContainer();
			c.WithHeaders(new { User_Agent = "Flurl", Cache_Control = "no-cache" });

			var req = GetRequest(c);
			Assert.IsTrue(req.Headers.Contains("User-Agent"));
			Assert.IsTrue(req.Headers.Contains("Cache-Control"));

			// make sure we can disable the behavior
			c.WithHeaders(new { no_i_really_want_underscores = "foo" }, false);
			Assert.IsTrue(GetRequest(c).Headers.Contains("no_i_really_want_underscores"));

			// dictionaries don't get this behavior since you can use hyphens explicitly
			c.WithHeaders(new Dictionary<string, string> { { "exclude_dictionaries", "bar" } });
			Assert.IsTrue(GetRequest(c).Headers.Contains("exclude_dictionaries"));

			// same with strings
			c.WithHeaders("exclude_strings=123");
			Assert.IsTrue(GetRequest(c).Headers.Contains("exclude_strings"));
		}

		[Test]
		public void header_names_are_case_insensitive() {
			var c = CreateContainer();
			c.WithHeader("a", 1).WithHeader("A", 2);

			var req = GetRequest(c);
			Assert.AreEqual(1, req.Headers.Count);
			Assert.AreEqual("A", req.Headers.Single().Name);
			Assert.AreEqual("2", req.Headers.Single().Value);
		}

		[Test] // #623
		public async Task header_values_are_trimmed() {
			var c = CreateContainer();
			c.WithHeader("a", "   1 \t\r\n");
			c.Headers.Add("b", "   2   ");

			var req = GetRequest(c);
			Assert.AreEqual(2, req.Headers.Count);
			Assert.AreEqual("1", req.Headers[0].Value);
			// Not trimmed when added directly to Headers collection (implementation seemed like overkill),
			// but below we'll make sure it happens on HttpRequestMessage when request is sent.
			Assert.AreEqual("   2   ", req.Headers[1].Value);

			using var test = new HttpTest();
			await GetRequest(c).GetAsync();
			var sentHeaders = test.CallLog[0].HttpRequestMessage.Headers;
			Assert.AreEqual("1", sentHeaders.GetValues("a").Single());
			Assert.AreEqual("2", sentHeaders.GetValues("b").Single());
		}

		[Test]
		public void can_setup_oauth_bearer_token() {
			var c = CreateContainer();
			c.WithOAuthBearerToken("mytoken");

			var req = GetRequest(c);
			Assert.AreEqual(1, req.Headers.Count);
			Assert.IsTrue(req.Headers.Contains("Authorization", "Bearer mytoken"));
		}

		[Test]
		public void can_setup_basic_auth() {
			var c = CreateContainer();
			c.WithBasicAuth("user", "pass");

			var req = GetRequest(c);
			Assert.AreEqual(1, req.Headers.Count);
			Assert.IsTrue(req.Headers.Contains("Authorization", "Basic dXNlcjpwYXNz"));
		}
	}

	[TestFixture]
	public class RequestHeadersTests : HeadersTestsBase<IFlurlRequest>
	{
		protected override IFlurlRequest CreateContainer() => new FlurlRequest("http://api.com");
		protected override IFlurlRequest GetRequest(IFlurlRequest req) => req;
	}

	[TestFixture]
	public class ClientHeadersTests : HeadersTestsBase<IFlurlClient>
	{
		protected override IFlurlClient CreateContainer() => new FlurlClient();
		protected override IFlurlRequest GetRequest(IFlurlClient cli) => cli.Request("http://api.com");

		[Test] // #778
		public void can_copy_multi_value_header_from_HttpClient() {
			var userAgent = "Mozilla/5.0 (X11; Linux i686; rv:109.0) Gecko/20100101 Firefox/120.0";
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Add("user-agent", userAgent);
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("foo", new[] {"a", "b", "c"});
			var flurlClient = new FlurlClient(httpClient);

			Assert.AreEqual(userAgent, flurlClient.Headers[0].Value);
			Assert.AreEqual("a,b,c", flurlClient.Headers[1].Value);
		}
	}

	[TestFixture]
	public class ClientBuilderHeadersTests : HeadersTestsBase<IFlurlClientBuilder>
	{
		protected override IFlurlClientBuilder CreateContainer() => new FlurlClientBuilder();
		protected override IFlurlRequest GetRequest(IFlurlClientBuilder builder) => builder.Build().Request("http://api.com");
	}
}
