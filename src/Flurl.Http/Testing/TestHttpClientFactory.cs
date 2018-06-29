using System;
using System.Net.Http;
using Flurl.Http.Configuration;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// IHttpClientFactory implementation used to fake and record calls in tests.
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