using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class GlobalConfigTests
	{
		[TearDown]
		public void ResetDefaults() {
			FlurlHttp.ResetDefaults();
		}

		[Test]
		public void can_provide_custom_httpclient_factory() {
			FlurlHttp.HttpClientFactory = new SomeCustomHttpClientFactory();
			var client = new FlurlClient("http://www.api.com");
			
			Assert.IsInstanceOf<SomeCustomHttpClient>(client.HttpClient);
		}

		[Test]
		public async Task can_set_pre_callback() {
			var callbackCalled = false;
			FlurlHttp.Testing.RespondWith("ok");
			FlurlHttp.BeforeCall = req => {
				CollectionAssert.IsNotEmpty(FlurlHttp.Testing.ResponseQueue); // verifies that callback is running before HTTP call is made
				callbackCalled = true;
			};
			Assert.IsFalse(callbackCalled);
			await "http://www.api.com".GetAsync();
			Assert.IsTrue(callbackCalled);
		}

		[Test]
		public async Task can_set_post_callback() {
			var callbackCalled = false;
			FlurlHttp.Testing.RespondWith("ok");
			FlurlHttp.AfterCall = (req, resp) => {
				CollectionAssert.IsEmpty(FlurlHttp.Testing.ResponseQueue); // verifies that callback is running after HTTP call is made
				callbackCalled = true;
			};
			Assert.IsFalse(callbackCalled);
			await "http://www.api.com".GetAsync();
			Assert.IsTrue(callbackCalled);
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
