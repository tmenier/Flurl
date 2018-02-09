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

	/// <summary>
	/// IFlurlClientFactory implementation used to fake and record calls in tests.
	/// </summary>
	public class TestFlurlClientFactory : FlurlClientFactoryBase
	{
		/// <summary>
		/// Returns the FlurlClient sigleton used for testing
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The FlurlClient instance.</returns>
		public override IFlurlClient Get(Url url) {
			return new FlurlClient(url.ToString());
		}

		/// <summary>
		/// Not used. Singleton FlurlClient used for lifetime of test.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		protected override string GetCacheKey(Url url) {
			return null;
		}
	}
}