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
			FlurlHttp.Configuration.ResetDefaults();
		}

		[Test]
		public void can_provide_custom_httpclient_factory() {
			FlurlHttp.Configuration.HttpClientFactory = new SomeCustomHttpClientFactory();
			var client = new FlurlClient("http://www.api.com");

			Assert.IsInstanceOf<SomeCustomHttpClient>(client.HttpClient);
			Assert.IsInstanceOf<SomeCustomMessageHandler>(client.HttpMessageHandler);
		}

		[Test]
		public async Task can_allow_non_success_status() {
			FlurlHttp.Configuration.AllowedHttpStatusRange = "4xx";
			using (var test = new HttpTest()) {
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
				FlurlHttp.Configuration.BeforeCall = req => {
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
				FlurlHttp.Configuration.AfterCall = call => {
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
				FlurlHttp.Configuration.OnError = call => {
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
			FlurlHttp.Configuration.OnError = call => {
				call.ExceptionHandled = true;
			};

			using (var test = new HttpTest()) {
				test.RespondWith(500, "server error");
				try {
					var result = await "http://www.api.com".GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (Exception) {
					Assert.Fail("Exception should not have been thrown.");
				}
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
