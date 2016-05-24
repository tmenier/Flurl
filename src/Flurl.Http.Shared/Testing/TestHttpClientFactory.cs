using System.Net.Http;
using Flurl.Http.Configuration;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Fake http client factory.
	/// </summary>
	public class TestHttpClientFactory : DefaultHttpClientFactory
	{
		private readonly HttpTest _test;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestHttpClientFactory"/> class.
		/// </summary>
		/// <param name="test">The test.</param>
		public TestHttpClientFactory(HttpTest test) {
			_test = test;
		}

		/// <summary>
		/// Creates the message handler.
		/// </summary>
		/// <returns></returns>
		public override HttpMessageHandler CreateMessageHandler() {
			return new FakeHttpMessageHandler(_test.GetNextResponse);
		}
	}
}