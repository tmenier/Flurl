using System.Net.Http;
using Flurl.Http.Configuration;

namespace Flurl.Http.Testing
{
	public class TestHttpClientFactory : DefaultHttpClientFactory
	{
		private readonly HttpTest _test;

		public TestHttpClientFactory(HttpTest test) {
			_test = test;
		}

		public override HttpMessageHandler CreateMessageHandler() {
			return new FakeHttpMessageHandler(_test.GetNextResponse);
		}
	}
}
