using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	// IFlurlClient and IFlurlRequest both implement IHttpSettingsContainer, which defines a number
	// of settings-related extension methods. This abstract test class allows those methods to be
	// tested against both both client-level and request-level implementations.
	public abstract class SettingsExtensionsTests<T> where T : IHttpSettingsContainer
	{
		protected abstract T GetSettingsContainer();
		protected abstract IFlurlRequest GetRequest(T sc);

		[Test]
		public void can_set_timeout() {
			var sc = GetSettingsContainer().WithTimeout(TimeSpan.FromSeconds(15));
			Assert.AreEqual(TimeSpan.FromSeconds(15), sc.Settings.Timeout);
		}

		[Test]
		public void can_set_timeout_in_seconds() {
			var sc = GetSettingsContainer().WithTimeout(15);
			Assert.AreEqual(sc.Settings.Timeout, TimeSpan.FromSeconds(15));
		}

		[Test]
		public void can_set_header() {
			var sc = GetSettingsContainer().WithHeader("a", 1);
			Assert.AreEqual(1, sc.Headers.Count);
			Assert.AreEqual(1, sc.Headers["a"]);
		}

		[Test]
		public void can_set_headers_from_anon_object() {
			var sc = GetSettingsContainer().WithHeaders(new { a = "b", one = 2 });
			Assert.AreEqual(2, sc.Headers.Count);
			Assert.AreEqual("b", sc.Headers["a"]);
			Assert.AreEqual(2, sc.Headers["one"]);
		}

		[Test]
		public void can_set_headers_from_dictionary() {
			var sc = GetSettingsContainer().WithHeaders(new Dictionary<string, object> { { "a", "b" }, { "one", 2 } });
			Assert.AreEqual(2, sc.Headers.Count);
			Assert.AreEqual("b", sc.Headers["a"]);
			Assert.AreEqual(2, sc.Headers["one"]);
		}

		[Test]
		public void can_setup_oauth_bearer_token() {
			var sc = GetSettingsContainer().WithOAuthBearerToken("mytoken");
			Assert.AreEqual(1, sc.Headers.Count);
			Assert.AreEqual("Bearer mytoken", sc.Headers["Authorization"]);
		}

		[Test]
		public void can_setup_basic_auth() {
			var sc = GetSettingsContainer().WithBasicAuth("user", "pass");
			Assert.AreEqual(1, sc.Headers.Count);
			Assert.AreEqual("Basic dXNlcjpwYXNz", sc.Headers["Authorization"]);
		}

		[Test]
		public async Task can_allow_specific_http_status() {
			using (var test = new HttpTest()) {
				test.RespondWith("Nothing to see here", 404);
				var sc = GetSettingsContainer().AllowHttpStatus(HttpStatusCode.Conflict, HttpStatusCode.NotFound);
				await GetRequest(sc).DeleteAsync(); // no exception = pass
			}
		}

		[Test]
		public void can_clear_non_success_status() {
			using (var test = new HttpTest()) {
				test.RespondWith("I'm a teapot", 418);
				// allow 4xx
				var sc = GetSettingsContainer().AllowHttpStatus("4xx");
				// but then disallow it
				sc.Settings.AllowedHttpStatusRange = null;
				Assert.ThrowsAsync<FlurlHttpException>(async () => await GetRequest(sc).GetAsync());
			}
		}

		[Test]
		public async Task can_allow_any_http_status() {
			using (var test = new HttpTest()) {
				test.RespondWith("epic fail", 500);
				try {
					var sc = GetSettingsContainer().AllowAnyHttpStatus();
					var result = await GetRequest(sc).GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (Exception) {
					Assert.Fail("Exception should not have been thrown.");
				}
			}
		}
	}

	[TestFixture, Parallelizable]
	public class ClientSettingsExtensionsTests : SettingsExtensionsTests<IFlurlClient>
	{
		protected override IFlurlClient GetSettingsContainer() => new FlurlClient();
		protected override IFlurlRequest GetRequest(IFlurlClient client) => client.Request("http://api.com");

		[Test]
		public void WithUrl_shares_client_but_not_Url() {
			var client = new FlurlClient().WithCookie("mycookie", "123");
			var req1 = client.Request("http://www.api.com/for-req1");
			var req2 = client.Request("http://www.api.com/for-req2");
			var req3 = client.Request("http://www.api.com/for-req3");

			CollectionAssert.AreEquivalent(req1.Cookies, req2.Cookies);
			CollectionAssert.AreEquivalent(req1.Cookies, req3.Cookies);
			var urls = new[] { req1, req2, req3 }.Select(c => c.Url.ToString());
			CollectionAssert.AllItemsAreUnique(urls);
		}

		[Test]
		public void WithClient_shares_client_but_not_Url() {
			var client = new FlurlClient().WithCookie("mycookie", "123");
			var req1 = "http://www.api.com/for-req1".WithClient(client);
			var req2 = "http://www.api.com/for-req2".WithClient(client);
			var req3 = "http://www.api.com/for-req3".WithClient(client);

			CollectionAssert.AreEquivalent(req1.Cookies, req2.Cookies);
			CollectionAssert.AreEquivalent(req1.Cookies, req3.Cookies);
			var urls = new[] { req1, req2, req3 }.Select(c => c.Url.ToString());
			CollectionAssert.AllItemsAreUnique(urls);
		}

		[Test]
		public void can_use_uri_with_WithUrl() {
			var uri = new System.Uri("http://www.mysite.com/foo?x=1");
			var req = new FlurlClient().Request(uri);
			Assert.AreEqual(uri.ToString(), req.Url.ToString());
		}

		[Test]
		public void can_override_settings_fluently() {
			using (var test = new HttpTest()) {
				var client = new FlurlClient().Configure(s => s.AllowedHttpStatusRange = "*");
				test.RespondWith("epic fail", 500);
				Assert.ThrowsAsync<FlurlHttpException>(async () => await "http://www.api.com"
					.Configure(c => c.AllowedHttpStatusRange = "2xx")
					.WithClient(client) // client-level settings shouldn't win
					.GetAsync());
			}
		}
	}

	[TestFixture, Parallelizable]
	public class RequestSettingsExtensionsTests : SettingsExtensionsTests<IFlurlRequest>
	{
		protected override IFlurlRequest GetSettingsContainer() => new FlurlRequest("http://api.com");
		protected override IFlurlRequest GetRequest(IFlurlRequest req) => req;
	}
}