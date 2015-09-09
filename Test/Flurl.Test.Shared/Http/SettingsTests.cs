using System;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class GlobalConfigTests
	{
		[TearDown]
		public void ResetDefaults() {
			FlurlHttp.GlobalSettings.ResetDefaults();
		}

		[Test]
		public void can_provide_custom_httpclient_factory() {
			FlurlHttp.GlobalSettings.HttpClientFactory = new SomeCustomHttpClientFactory();
			var client = new FlurlClient("http://www.api.com");

			Assert.IsInstanceOf<SomeCustomHttpClient>(client.HttpClient);
			Assert.IsInstanceOf<SomeCustomMessageHandler>(client.HttpMessageHandler);
		}

		[Test]
		public async Task can_allow_non_success_status() {
			using (var test = new HttpTest()) {
				FlurlHttp.GlobalSettings.AllowedHttpStatusRange = "4xx";
				test.RespondWith(418, "I'm a teapot");
				try {
					var result = await "http://www.api.com".GetAsync();
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
				FlurlHttp.GlobalSettings.BeforeCall = req => {
					CollectionAssert.IsNotEmpty(test.ResponseQueue); // verifies that callback is running before HTTP call is made
					callbackCalled = true;
				};
				Assert.IsFalse(callbackCalled);
				await "http://www.api.com".GetAsync();
				Assert.IsTrue(callbackCalled);
			}
		}

		[Test]
		public async Task can_set_post_callback() {
			var callbackCalled = false;
			using (var test = new HttpTest()) {
				test.RespondWith("ok");
				FlurlHttp.GlobalSettings.AfterCall = call => {
					CollectionAssert.IsEmpty(test.ResponseQueue); // verifies that callback is running after HTTP call is made
					callbackCalled = true;
				};
				Assert.IsFalse(callbackCalled);
				await "http://www.api.com".GetAsync();
				Assert.IsTrue(callbackCalled);				
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task can_set_error_callback(bool markExceptionHandled) {
			var callbackCalled = false;
			using (var test = new HttpTest()) {
				test.RespondWith(500, "server error");
				FlurlHttp.GlobalSettings.OnError = call => {
					CollectionAssert.IsEmpty(test.ResponseQueue); // verifies that callback is running after HTTP call is made
					callbackCalled = true;
					call.ExceptionHandled = markExceptionHandled;
				};
				Assert.IsFalse(callbackCalled);
				try {
					await "http://www.api.com".GetAsync();
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
				FlurlHttp.GlobalSettings.OnError = call => {
					call.ExceptionHandled = true;
				};
				test.RespondWith(500, "server error");
				try {
					var result = await "http://www.api.com".GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (FlurlHttpException) {
					Assert.Fail("Flurl should not have thrown exception.");
				}
			}
		}

		[Test]
		public async Task client_can_override_global_settings() {
			var overridden = false;
			using (new HttpTest()) {
				FlurlHttp.GlobalSettings.AfterCall = _ => overridden = false;
				var fc = new FlurlClient("http://www.api.com");
				fc.Settings.AfterCall = _ => overridden = true;
				await fc.GetAsync();
				Assert.True(overridden);
			}
		}

		private class SomeCustomHttpClientFactory : IHttpClientFactory
		{
			public HttpClient CreateClient(Url url, HttpMessageHandler handler) {
				return new SomeCustomHttpClient();
			}

			public HttpMessageHandler CreateMessageHandler() {
				return new SomeCustomMessageHandler();
			}
		}

		private class SomeCustomHttpClient : HttpClient { }
		private class SomeCustomMessageHandler : HttpClientHandler { }
	}
}
