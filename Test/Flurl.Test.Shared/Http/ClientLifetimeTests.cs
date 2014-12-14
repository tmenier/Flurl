using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
    public class ClientLifetimeTests
	{
		private readonly TestHttpClientFactoryWithCounter _fac;

		public ClientLifetimeTests() {
			_fac = new TestHttpClientFactoryWithCounter();
			FlurlHttp.Configure(opts => opts.HttpClientFactory = _fac);
		} 

		[SetUp]
		public void CreateHttpTest() {
			_fac.NewClientCount = 0;
		}

		[Test]
		public async Task autodispose_true_creates_new_httpclients() {
			var fc = new FlurlClient("http://www.mysite.com", true);
			var x = await fc.GetAsync();
			var y = await fc.GetAsync();
			var z = await fc.GetAsync();
			Assert.AreEqual(3, _fac.NewClientCount);
		}

		[Test]
		public async Task autodispose_false_resues_httpclient() {
			var fc = new FlurlClient("http://www.mysite.com", false);
			var x = await fc.GetAsync();
			var y = await fc.GetAsync();
			var z = await fc.GetAsync();
			Assert.AreEqual(1, _fac.NewClientCount);
		}

		private class TestHttpClientFactoryWithCounter : TestHttpClientFactory
		{
			public int NewClientCount { get; set; }

			public TestHttpClientFactoryWithCounter() : base(new HttpTest()) { }

			public override HttpClient CreateClient(Url url, HttpMessageHandler handler) {
				NewClientCount++;
				return base.CreateClient(url, handler);
			}
		}
	}
}
