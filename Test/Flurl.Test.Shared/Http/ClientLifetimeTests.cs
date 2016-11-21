using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
    public class ClientLifetimeTests
	{
		[Test]
		public async Task autodispose_true_creates_new_httpclients() {
			var fac = new TestHttpClientFactoryWithCounter();
			var fc = new FlurlClient("http://www.mysite.com") {
				Settings = { HttpClientFactory = fac, AutoDispose = true }
			};
			var x = await fc.GetAsync();
			var y = await fc.GetAsync();
			var z = await fc.GetAsync();
			Assert.AreEqual(3, fac.NewClientCount);
		}

		[Test]
		public async Task autodispose_false_reuses_httpclient() {
			var fac = new TestHttpClientFactoryWithCounter();
			var fc = new FlurlClient("http://www.mysite.com") {
				Settings = { HttpClientFactory = fac, AutoDispose = false }
			};
			var x = await fc.GetAsync();
			var y = await fc.GetAsync();
			var z = await fc.GetAsync();
			Assert.AreEqual(1, fac.NewClientCount);
		}

		private class TestHttpClientFactoryWithCounter : TestHttpClientFactory
		{
			public int NewClientCount { get; set; }

			public override HttpClient CreateClient(Url url, HttpMessageHandler handler) {
				NewClientCount++;
				return base.CreateClient(url, handler);
			}
		}
	}
}
