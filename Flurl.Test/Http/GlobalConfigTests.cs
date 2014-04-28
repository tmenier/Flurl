using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

		private class SomeCustomHttpClientFactory : IHttpClientFactory
		{
			public HttpClient CreateClient(Url url) {
				return new SomeCustomHttpClient();
			}
		}

		private class SomeCustomHttpClient : HttpClient { }
	}
}
