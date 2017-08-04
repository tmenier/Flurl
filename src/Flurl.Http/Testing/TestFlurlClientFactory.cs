using System;
using System.Diagnostics;
using System.Net.Http;
using Flurl.Http.Configuration;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Fake http client factory.
	/// </summary>
	public class TestFlurlClientFactory : DefaultFlurlClientFactory
	{
		private readonly Lazy<FlurlClient> _client = new Lazy<FlurlClient>(() => new FlurlClient());

		/// <summary>
		/// Creates an instance of FakeHttpMessageHander, which prevents actual HTTP calls from being made.
		/// </summary>
		/// <returns></returns>
		public override HttpMessageHandler CreateMessageHandler() {
			return new FakeHttpMessageHandler();
		}

		/// <summary>
		/// Returns the FlurlClient sigleton used for testing
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The FlurlClient instance.</returns>
		public override IFlurlClient GetClient(Url url) {
			return _client.Value;
		}
	}
}