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

		public override HttpClient CreateClient(Url url) {
			return new HttpClient(new FlurlMessageHandler(new FakeHttpMessageHandler(_test.GetNextResponse))) {
				Timeout = FlurlHttp.Configuration.DefaultTimeout
			};
		}
	}
}
