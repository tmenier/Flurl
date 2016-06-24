using System;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// All global settings can also be set at the client level, so this base class allows ClientConfigTests to 
	/// inherit all the same tests.
	/// </summary>
	public abstract class ConfigTestsBase
	{
		protected abstract FlurlHttpSettings GetSettings();

		private FlurlClient _client;
		protected FlurlClient GetClient() {
			if (_client == null)
				_client = new FlurlClient("http://www.api.com");
			return _client;
		}

		[TearDown]
		public void ResetDefaults() {
			GetSettings().ResetDefaults();
			_client = null;
		}

		[Test]
		public void can_provide_custom_httpclient_factory() {
			GetSettings().HttpClientFactory = new SomeCustomHttpClientFactory();
			Assert.IsInstanceOf<SomeCustomHttpClient>(GetClient().HttpClient);
			Assert.IsInstanceOf<SomeCustomMessageHandler>(GetClient().HttpMessageHandler);
		}

		[Test]
		public async Task can_allow_non_success_status() {
			using (var test = new HttpTest()) {
				GetSettings().AllowedHttpStatusRange = "4xx";
				test.RespondWith("I'm a teapot", 418);
				try {
					var result = await GetClient().GetAsync();
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
				GetSettings().BeforeCall = req => {
					CollectionAssert.IsNotEmpty(test.ResponseQueue); // verifies that callback is running before HTTP call is made
					callbackCalled = true;
				};
				Assert.IsFalse(callbackCalled);
				await GetClient().GetAsync();
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
				await GetClient().GetAsync();
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
					await GetClient().GetAsync();
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
					var result = await GetClient().GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (FlurlHttpException) {
					Assert.Fail("Flurl should not have thrown exception.");
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

	[TestFixture]
	public class GlobalConfigTestsBase : ConfigTestsBase
	{
		protected override FlurlHttpSettings GetSettings() {
			return FlurlHttp.GlobalSettings;
		}
	}
}
