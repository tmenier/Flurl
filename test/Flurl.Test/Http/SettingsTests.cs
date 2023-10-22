using System;
using System.IO;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// FlurlHttpSettings are available at the test, client, and request level. This abstract class
	/// allows the same tests to be run against settings at all 4 levels.
	/// </summary>
	public abstract class SettingsTestsBase
	{
		protected abstract FlurlHttpSettings GetSettings();
		protected abstract IFlurlRequest GetRequest();

		[Test]
		public async Task can_set_http_version() {
			Assert.AreEqual("1.1", GetSettings().HttpVersion); // default

			using var test = new HttpTest();

			GetSettings().HttpVersion = "2.0";
			var req = GetRequest();
			Assert.AreEqual("2.0", req.Settings.HttpVersion);

			Version versionUsed = null;
			await req
				.BeforeCall(c => versionUsed = c.HttpRequestMessage.Version)
				.GetAsync();

			Assert.AreEqual("2.0", versionUsed.ToString());
		}

		[Test]
		public void cannot_set_invalid_http_version() {
			Assert.Throws<ArgumentException>(() => GetSettings().HttpVersion = "foo");
		}

		[Test]
		public async Task can_allow_non_success_status() {
			using var test = new HttpTest();

			GetSettings().AllowedHttpStatusRange = "4xx";
			test.RespondWith("I'm a teapot", 418);
			try {
				var result = await GetRequest().GetAsync();
				Assert.AreEqual(418, result.StatusCode);
			}
			catch (Exception) {
				Assert.Fail("Exception should not have been thrown.");
			}
		}

		[Test]
		public async Task can_set_pre_callback() {
			var callbackCalled = false;
			using var test = new HttpTest();

			test.RespondWith("ok");
			GetSettings().BeforeCall = call => {
				Assert.Null(call.Response); // verifies that callback is running before HTTP call is made
				callbackCalled = true;
			};
			Assert.IsFalse(callbackCalled);
			await GetRequest().GetAsync();
			Assert.IsTrue(callbackCalled);
		}

		[Test]
		public async Task can_set_post_callback() {
			var callbackCalled = false;
			using var test = new HttpTest();

			test.RespondWith("ok");
			GetSettings().AfterCall = call => {
				Assert.NotNull(call.Response); // verifies that callback is running after HTTP call is made
				callbackCalled = true;
			};
			Assert.IsFalse(callbackCalled);
			await GetRequest().GetAsync();
			Assert.IsTrue(callbackCalled);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task can_set_error_callback(bool markExceptionHandled) {
			var callbackCalled = false;
			using var test = new HttpTest();

			test.RespondWith("server error", 500);
			GetSettings().OnError = call => {
				Assert.NotNull(call.Response); // verifies that callback is running after HTTP call is made
				callbackCalled = true;
				call.ExceptionHandled = markExceptionHandled;
			};
			Assert.IsFalse(callbackCalled);
			try {
				await GetRequest().GetAsync();
				Assert.IsTrue(callbackCalled, "OnError was never called");
				Assert.IsTrue(markExceptionHandled, "ExceptionHandled was marked false in callback, but exception was not propagated.");
			}
			catch (FlurlHttpException) {
				Assert.IsTrue(callbackCalled, "OnError was never called");
				Assert.IsFalse(markExceptionHandled, "ExceptionHandled was marked true in callback, but exception was propagated.");
			}
		}

		[Test]
		public async Task can_disable_exception_behavior() {
			using var test = new HttpTest();

			GetSettings().OnError = call => {
				call.ExceptionHandled = true;
			};
			test.RespondWith("server error", 500);
			try {
				var result = await GetRequest().GetAsync();
				Assert.AreEqual(500, result.StatusCode);
			}
			catch (FlurlHttpException) {
				Assert.Fail("Flurl should not have thrown exception.");
			}
		}

		[Test]
		public void can_reset_defaults() {
			GetSettings().JsonSerializer = null;
			GetSettings().Redirects.Enabled = false;
			GetSettings().BeforeCall = (call) => Console.WriteLine("Before!");
			GetSettings().Redirects.MaxAutoRedirects = 5;

			Assert.IsNull(GetSettings().JsonSerializer);
			Assert.IsFalse(GetSettings().Redirects.Enabled);
			Assert.IsNotNull(GetSettings().BeforeCall);
			Assert.AreEqual(5, GetSettings().Redirects.MaxAutoRedirects);

			GetSettings().ResetDefaults();

			Assert.That(GetSettings().JsonSerializer is DefaultJsonSerializer);
			Assert.IsTrue(GetSettings().Redirects.Enabled);
			Assert.IsNull(GetSettings().BeforeCall);
			Assert.AreEqual(10, GetSettings().Redirects.MaxAutoRedirects);
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
	}

	[TestFixture]
	public class HttpTestSettingsTests : SettingsTestsBase
	{
		private HttpTest _test;

		[SetUp]
		public void CreateTest() => _test = new HttpTest();

		[TearDown]
		public void DisposeTest() => _test.Dispose();

		protected override FlurlHttpSettings GetSettings() => HttpTest.Current.Settings;
		protected override IFlurlRequest GetRequest() => new FlurlRequest("http://api.com");

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

	[TestFixture]
	public class ClientSettingsTests : SettingsTestsBase
	{
		private readonly Lazy<IFlurlClient> _client = new Lazy<IFlurlClient>(() => new FlurlClient());

		protected override FlurlHttpSettings GetSettings() => _client.Value.Settings;
		protected override IFlurlRequest GetRequest() => _client.Value.Request("http://api.com");
	}

	[TestFixture]
	public class RequestSettingsTests : SettingsTestsBase
	{
		private readonly Lazy<IFlurlRequest> _req = new Lazy<IFlurlRequest>(() => new FlurlRequest("http://api.com"));

		protected override FlurlHttpSettings GetSettings() => _req.Value.Settings;
		protected override IFlurlRequest GetRequest() => _req.Value;

		[Test]
		public void request_gets_default_settings_when_no_client() {
			var req = new FlurlRequest();
			Assert.IsNull(req.Client);
			Assert.IsNull(req.Url);
			Assert.IsInstanceOf<DefaultJsonSerializer>(req.Settings.JsonSerializer);
		}
	}
}
