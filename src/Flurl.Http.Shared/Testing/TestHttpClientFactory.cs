using System.Net.Http;
using Flurl.Http.Configuration;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Fake http client factory.
	/// </summary>
	public class TestHttpClientFactory : DefaultHttpClientFactory
	{
		/// <summary>
		/// Creates an instance of FakeHttpMessageHander, which prevents actual HTTP calls from being made.
		/// </summary>
		/// <returns></returns>
		public override HttpMessageHandler CreateMessageHandler() {
			return new FakeHttpMessageHandler();
		}
	}
}