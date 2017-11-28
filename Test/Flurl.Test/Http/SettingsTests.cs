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
	/// FlurlHttpSettings are available at the global, test, client, and request level. This abstract class
	/// allows the same tests to be run against settings at all 4 levels.
	/// </summary>
	public abstract class SettingsTestsBase
	{
		protected abstract FlurlHttpSettings GetSettings();
		protected abstract IFlurlRequest GetRequest();

		[Test]
		public async Task can_allow_non_success_status() {
			using (var test = new HttpTest()) {
				GetSettings().AllowedHttpStatusRange = "4xx";
				test.RespondWith("I'm a teapot", 418);
				try {
					var result = await GetRequest().GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (Exception) {
					Assert.Fail("Exception should not have been thrown.");
				}
			}
		}

		[Test]
		public async Task can_set_pre_callback() {
			var callbackCalled = false;
			using (var test = new HttpTest()) {
				test.RespondWith("ok");
				GetSettings().BeforeCall = call => {
					CollectionAssert.IsNotEmpty(test.ResponseQueue); // verifies that callback is running before HTTP call is made
					callbackCalled = true;
				};
				Assert.IsFalse(callbackCalled);
				await GetRequest().GetAsync();
				Assert.IsTrue(callbackCalled);
			}
		}

		[Test]
		public async Task can_set_post_callback() {
			var callbackCalled = false;
			using (var test = new HttpTest()) {
				test.RespondWith("ok");
				GetSettings().AfterCall = call => {
					CollectionAssert.IsEmpty(test.ResponseQueue); // verifies that callback is running after HTTP call is made
					callbackCalled = true;
				};
				Assert.IsFalse(callbackCalled);
				await GetRequest().GetAsync();
				Assert.IsTrue(callbackCalled);
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task can_set_error_callback(bool markExceptionHandled) {
			var callbackCalled = false;
			using (var test = new HttpTest()) {
				test.RespondWith("server error", 500);
				GetSettings().OnError = call => {
					CollectionAssert.IsEmpty(test.ResponseQueue); // verifies that callback is running after HTTP call is made
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
		}

		[Test]
		public async Task can_disable_exception_behavior() {
			using (var test = new HttpTest()) {
				GetSettings().OnError = call => {
					call.ExceptionHandled = true;
				};
				test.RespondWith("server error", 500);
				try {
					var result = await GetRequest().GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (FlurlHttpException) {
					Assert.Fail("Flurl should not have thrown exception.");
				}
			}
		}

		[Test]
		public void can_reset_defaults() {
			GetSettings().JsonSerializer = null;
			GetSettings().CookiesEnabled = true;
			GetSettings().BeforeCall = (call) => Console.WriteLine("Before!");

			Assert.IsNull(GetSettings().JsonSerializer);
			Assert.IsTrue(GetSettings().CookiesEnabled);
			Assert.IsNotNull(GetSettings().BeforeCall);

			GetSettings().ResetDefaults();

			Assert.That(GetSettings().JsonSerializer is NewtonsoftJsonSerializer);
			Assert.IsFalse(GetSettings().CookiesEnabled);
			Assert.IsNull(GetSettings().BeforeCall);
		}
	}

	[TestFixture, NonParallelizable] // touches global settings, so can't run in parallel
	public class GlobalSettingsTests : SettingsTestsBase
	{
		protected override FlurlHttpSettings GetSettings() => FlurlHttp.GlobalSettings;
		protected override IFlurlRequest GetRequest() => new FlurlRequest("http://api.com");

		[TearDown]
		public void ResetDefaults() => FlurlHttp.GlobalSettings.ResetDefaults();

		[Test]
		public void settings_propagate_correctly() {
			FlurlHttp.GlobalSettings.CookiesEnabled = false;
			FlurlHttp.GlobalSettings.AllowedHttpStatusRange = "4xx";

			var client1 = new FlurlClient();
			client1.Settings.CookiesEnabled = true;
			Assert.AreEqual("4xx", client1.Settings.AllowedHttpStatusRange);
			client1.Settings.AllowedHttpStatusRange = "5xx";

			var req = client1.Request("http://myapi.com");
			Assert.IsTrue(req.Settings.CookiesEnabled, "request should inherit client settings when not set at request level");
			Assert.AreEqual("5xx", req.Settings.AllowedHttpStatusRange, "request should inherit client settings when not set at request level");

			var client2 = new FlurlClient();
			client2.Settings.CookiesEnabled = false;

			req.WithClient(client2);
			Assert.IsFalse(req.Settings.CookiesEnabled, "request should inherit client settings when not set at request level");
			Assert.AreEqual("4xx", req.Settings.AllowedHttpStatusRange, "request should inherit global settings when not set at request or client level");

			client2.Settings.CookiesEnabled = true;
			client2.Settings.AllowedHttpStatusRange = "3xx";
			Assert.IsTrue(req.Settings.CookiesEnabled, "request should inherit client sttings when not set at request level");
			Assert.AreEqual("3xx", req.Settings.AllowedHttpStatusRange, "request should inherit client sttings when not set at request level");

			req.Settings.CookiesEnabled = false;
			req.Settings.AllowedHttpStatusRange = "6xx";
			Assert.IsFalse(req.Settings.CookiesEnabled, "request-level settings should override any defaults");
			Assert.AreEqual("6xx", req.Settings.AllowedHttpStatusRange, "request-level settings should override any defaults");

			req.Settings.ResetDefaults();
			Assert.IsTrue(req.Settings.CookiesEnabled, "request should inherit client sttings when cleared at request level");
			Assert.AreEqual("3xx", req.Settings.AllowedHttpStatusRange, "request should inherit client sttings when cleared request level");

			client2.Settings.ResetDefaults();
			Assert.IsFalse(req.Settings.CookiesEnabled, "request should inherit global settings when cleared at request and client level");
			Assert.AreEqual("4xx", req.Settings.AllowedHttpStatusRange, "request should inherit global settings when cleared at request and client level");
		}

		[Test]
		public void can_provide_custom_client_factory() {
			FlurlHttp.GlobalSettings.HttpClientFactory = new SomeCustomHttpClientFactory();
			Assert.IsInstanceOf<SomeCustomHttpClient>(GetRequest().Client.HttpClient);
			Assert.IsInstanceOf<SomeCustomMessageHandler>(GetRequest().Client.HttpMessageHandler);
		}

		[Test]
		public void can_configure_global_from_FlurlHttp_object() {
			FlurlHttp.Configure(settings => settings.CookiesEnabled = true);
			Assert.IsTrue(FlurlHttp.GlobalSettings.CookiesEnabled);
		}

		[Test]
		public void can_configure_client_from_FlurlHttp_object() {
			FlurlHttp.ConfigureClient("http://host1.com/foo", settings => settings.CookiesEnabled = true);
			Assert.IsTrue(new FlurlRequest("https://host1.com/bar").Client.Settings.CookiesEnabled); // different URL but same host, so should use same client
			Assert.IsFalse(new FlurlRequest("http://host2.com").Client.Settings.CookiesEnabled);
		}
	}

	[TestFixture, Parallelizable]
	public class HttpTestSettingsTests : SettingsTestsBase
	{
		private HttpTest _test;

		[SetUp]
		public void CreateTest() => _test = new HttpTest();

		[TearDown]
		public void DisposeTest() => _test.Dispose();

		protected override FlurlHttpSettings GetSettings() => HttpTest.Current.Settings;
		protected override IFlurlRequest GetRequest() => new FlurlRequest("http://api.com");
	}

	[TestFixture, Parallelizable]
	public class ClientSettingsTests : SettingsTestsBase
	{
		private readonly Lazy<IFlurlClient> _client = new Lazy<IFlurlClient>(() => new FlurlClient());

		protected override FlurlHttpSettings GetSettings() => _client.Value.Settings;
		protected override IFlurlRequest GetRequest() => _client.Value.Request("http://api.com");

		[Test]
		public void can_provide_custom_client_factory() {
			var cli = new FlurlClient();
			cli.Settings.HttpClientFactory = new SomeCustomHttpClientFactory();
			Assert.IsInstanceOf<SomeCustomHttpClient>(cli.HttpClient);
			Assert.IsInstanceOf<SomeCustomMessageHandler>(cli.HttpMessageHandler);
		}

		[Test]
		public async Task connection_lease_timeout_sets_connection_close_header() {
			using (var test = new HttpTest()) {
				var cli = new FlurlClient("http://api.com");
				cli.Settings.ConnectionLeaseTimeout = TimeSpan.FromMilliseconds(50);

				await cli.Request("1").GetAsync();
				test.ShouldHaveCalled("http://api.com/1").WithoutHeader("Connection");

				// exceed the timeout
				await Task.Delay(51);

				// slam it many times concurrently
				await Task.WhenAll(
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync(),
					cli.Request("2").GetAsync());

				// connection:close header should get sent exactly once
				test.ShouldHaveCalled("http://api.com/2").WithHeader("Connection", "close").Times(1);

				await Task.Delay(10);

				await cli.Request("3").GetAsync();
				test.ShouldHaveCalled("http://api.com/3").WithoutHeader("Connection");
			}
		}
	}

	[TestFixture, Parallelizable]
	public class RequestSettingsTests : SettingsTestsBase
	{
		private readonly Lazy<IFlurlRequest> _req = new Lazy<IFlurlRequest>(() => new FlurlRequest("http://api.com"));

		protected override FlurlHttpSettings GetSettings() => _req.Value.Settings;
		protected override IFlurlRequest GetRequest() => _req.Value;

		[Test, NonParallelizable] // github #239
		public void request_default_settings_change_when_client_changes() {
			FlurlHttp.ConfigureClient("http://test.com", settings => settings.CookiesEnabled = true);
			var req = new FlurlRequest("http://test.com");
			var cli1 = req.Client;
			Assert.IsTrue(req.Settings.CookiesEnabled, "pre-configured client should provide defaults to new request");

			req.Url = "http://test.com/foo";
			Assert.AreSame(cli1, req.Client, "new URL with same host should hold onto same client");
			Assert.IsTrue(req.Settings.CookiesEnabled);

			req.Url = "http://test2.com";
			Assert.AreNotSame(cli1, req.Client, "new host should trigger new client");
			Assert.IsFalse(req.Settings.CookiesEnabled);

			FlurlHttp.ConfigureClient("http://test2.com", settings => settings.CookiesEnabled = true);
			Assert.IsTrue(req.Settings.CookiesEnabled, "changing client settings should be reflected in request");

			req.Settings = new FlurlHttpSettings();
			Assert.IsTrue(req.Settings.CookiesEnabled, "entirely new settings object should still inherit current client settings");

			req.Client = new FlurlClient();
			Assert.IsFalse(req.Settings.CookiesEnabled, "entirely new client should provide new defaults");

			req.Url = "http://test.com";
			Assert.AreNotSame(cli1, req.Client, "client was explicitly set on request, so it shouldn't change even if the URL changes");
			Assert.IsFalse(req.Settings.CookiesEnabled);
		}

		[Test]
		public void request_gets_global_settings_when_no_client() {
			var req = new FlurlRequest();
			Assert.IsNull(req.Client);
			Assert.IsNull(req.Url);
			Assert.AreEqual(FlurlHttp.GlobalSettings.JsonSerializer, req.Settings.JsonSerializer);
		}
	}

	public class SomeCustomHttpClientFactory : IHttpClientFactory
	{
		public HttpClient CreateHttpClient(HttpMessageHandler handler) => new SomeCustomHttpClient();
		public HttpMessageHandler CreateMessageHandler() => new SomeCustomMessageHandler();
	}

	public class SomeCustomHttpClient : HttpClient { }
	public class SomeCustomMessageHandler : HttpClientHandler { }
}
