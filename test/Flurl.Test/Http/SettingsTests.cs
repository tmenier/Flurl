using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// A Settings collection is available on IFlurlRequest, IFlurlClient, IFlurlClientBuilder, and HttpTest.
	/// This abstract class allows the same tests to be run against all 4.
	/// </summary>
	public abstract class SettingsTestsBase<T> where T : ISettingsContainer
	{
		protected abstract T CreateContainer();
		protected abstract IFlurlRequest GetRequest(T container);

		[Test]
		public async Task can_set_http_version() {
			using var test = new HttpTest();

			var c = CreateContainer();
			Assert.AreEqual("1.1", c.Settings.HttpVersion); // default

			c.Settings.HttpVersion = "2.0";
			var req = GetRequest(c);
			Assert.AreEqual("2.0", req.Settings.HttpVersion);

			Version versionUsed = null;
			await req
				.BeforeCall(c => versionUsed = c.HttpRequestMessage.Version)
				.GetAsync();

			Assert.AreEqual("2.0", versionUsed.ToString());
		}

		[Test]
		public void cannot_set_invalid_http_version() {
			Assert.Throws<ArgumentException>(() => CreateContainer().Settings.HttpVersion = "foo");
		}

		[Test]
		public async Task can_allow_non_success_status() {
			using var test = new HttpTest();

			var c = CreateContainer();
			c.Settings.AllowedHttpStatusRange = "4xx";
			test.RespondWith("I'm a teapot", 418);
			try {
				var result = await GetRequest(c).GetAsync();
				Assert.AreEqual(418, result.StatusCode);
			}
			catch (Exception) {
				Assert.Fail("Exception should not have been thrown.");
			}
		}

		[Test]
		public void can_reset_defaults() {
			var c = CreateContainer();

			c.Settings.JsonSerializer = null;
			c.Settings.Redirects.Enabled = false;
			c.Settings.Redirects.MaxAutoRedirects = 5;

			var req = GetRequest(c);

			Assert.IsNull(req.Settings.JsonSerializer);
			Assert.IsFalse(req.Settings.Redirects.Enabled);
			Assert.AreEqual(5, req.Settings.Redirects.MaxAutoRedirects);

			c.Settings.ResetDefaults();
			req = GetRequest(c);

			Assert.That(req.Settings.JsonSerializer is DefaultJsonSerializer);
			Assert.IsTrue(req.Settings.Redirects.Enabled);
			Assert.AreEqual(10, req.Settings.Redirects.MaxAutoRedirects);
		}

		[Test] // #256
		public async Task explicit_content_type_header_is_not_overridden() {
			using var test = new HttpTest();

			// PostJsonAsync will normally set Content-Type to application/json, but it shouldn't touch it if it was set explicitly.
			await "https://api.com"
				.WithHeader("content-type", "application/json-patch+json; utf-8")
				.WithHeader("CONTENT-LENGTH", 10) // header names are case-insensitive
				.PostJsonAsync(new { foo = "bar" });

			var h = test.CallLog[0].HttpRequestMessage.Content.Headers;
			Assert.AreEqual(new[] { "application/json-patch+json; utf-8" }, h.GetValues("Content-Type"));
			Assert.AreEqual(new[] { "10" }, h.GetValues("Content-Length"));
		}

		[Test]
		public void can_set_timeout() {
			var c = CreateContainer().WithTimeout(TimeSpan.FromSeconds(15));
			var req = GetRequest(c);
			Assert.AreEqual(TimeSpan.FromSeconds(15), req.Settings.Timeout);
		}

		[Test]
		public void can_set_timeout_in_seconds() {
			var c = CreateContainer().WithTimeout(15);
			var req = GetRequest(c);
			Assert.AreEqual(req.Settings.Timeout, TimeSpan.FromSeconds(15));
		}

		[Test]
		public async Task can_allow_specific_http_status() {
			using var test = new HttpTest();
			test.RespondWith("Nothing to see here", 404);
			var c = CreateContainer().AllowHttpStatus(409, 404);
			await GetRequest(c).DeleteAsync(); // no exception = pass
		}

		[Test]
		public async Task allow_specific_http_status_also_allows_2xx() {
			using var test = new HttpTest();
			test.RespondWith("I'm just an innocent 2xx, I should never fail!", 201);
			var c = CreateContainer().AllowHttpStatus(409, 404);
			await GetRequest(c).GetAsync(); // no exception = pass
		}

		[Test]
		public void can_clear_non_success_status() {
			using var test = new HttpTest();
			test.RespondWith("I'm a teapot", 418);
			// allow 4xx
			var c = CreateContainer().AllowHttpStatus("4xx");
			// but then disallow it
			c.Settings.AllowedHttpStatusRange = null;
			Assert.ThrowsAsync<FlurlHttpException>(async () => await GetRequest(c).GetAsync());
		}

		[Test]
		public async Task can_allow_any_http_status() {
			using var test = new HttpTest();
			test.RespondWith("epic fail", 500);
			try {
				var c = CreateContainer().AllowAnyHttpStatus();
				var result = await GetRequest(c).GetAsync();
				Assert.AreEqual(500, result.StatusCode);
			}
			catch (Exception) {
				Assert.Fail("Exception should not have been thrown.");
			}
		}
	}

	[TestFixture]
	public class RequestSettingsTests : SettingsTestsBase<IFlurlRequest>
	{
		protected override IFlurlRequest CreateContainer() => new FlurlRequest("http://api.com");
		protected override IFlurlRequest GetRequest(IFlurlRequest req) => req;

		[Test]
		public void request_gets_default_settings_when_no_client() {
			var req = new FlurlRequest();
			Assert.IsNull(req.Client);
			Assert.IsNull(req.Url);
			Assert.IsInstanceOf<DefaultJsonSerializer>(req.Settings.JsonSerializer);
		}

		[Test]
		public void can_override_settings_fluently() {
			using var test = new HttpTest();
			var cli = new FlurlClient().WithSettings(s => s.AllowedHttpStatusRange = "*");
			test.RespondWith("epic fail", 500);
			var req = "http://www.api.com".WithSettings(c => c.AllowedHttpStatusRange = "2xx");
			req.Client = cli; // client-level settings shouldn't win
			Assert.ThrowsAsync<FlurlHttpException>(async () => await req.GetAsync());
		}
	}

	[TestFixture]
	public class ClientSettingsTests : SettingsTestsBase<IFlurlClient>
	{
		protected override IFlurlClient CreateContainer() => new FlurlClient();
		protected override IFlurlRequest GetRequest(IFlurlClient cli) => cli.Request("http://api.com");
	}

	[TestFixture]
	public class ClientBuilderSettingsTests : SettingsTestsBase<IFlurlClientBuilder>
	{
		protected override IFlurlClientBuilder CreateContainer() => new FlurlClientBuilder();
		protected override IFlurlRequest GetRequest(IFlurlClientBuilder builder) => builder.Build().Request("http://api.com");
	}

	[TestFixture]
	public class HttpTestSettingsTests : SettingsTestsBase<HttpTest>
	{
		protected override HttpTest CreateContainer() => HttpTest.Current ?? new HttpTest();
		protected override IFlurlRequest GetRequest(HttpTest container) => new FlurlRequest("http://api.com");

		[TearDown]
		public void DisposeTest() => HttpTest.Current?.Dispose();

		[Test] // #246
		public void test_settings_dont_override_request_settings_when_not_set_explicitily() {
			var ser1 = new FakeSerializer();
			var ser2 = new FakeSerializer();

			using var test = new HttpTest();

			var cli = new FlurlClient();
			cli.Settings.JsonSerializer = ser1;
			Assert.AreSame(ser1, cli.Settings.JsonSerializer);

			var req = new FlurlRequest { Client = cli };
			Assert.AreSame(ser1, req.Settings.JsonSerializer);

			req.Settings.JsonSerializer = ser2;
			Assert.AreSame(ser2, req.Settings.JsonSerializer);
		}

		private class FakeSerializer : ISerializer
		{
			public string Serialize(object obj) => "foo";
			public T Deserialize<T>(string s) => default;
			public T Deserialize<T>(Stream stream) => default;
		}
	}
}
