using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Default implementation of IHttpClientFactory used by FlurlHttp. The created HttpClient includes hooks
	/// that enable FlurlHttp's testing features and respect its configuration settings. Therefore, custom factories
	/// should inherit from this class (rather than implementing IHttpClientFactory directly) and ensure base.CreateClient
	/// is called before providing enhancements.
	/// </summary>
	public class DefaultHttpClientFactory : IHttpClientFactory
	{
		/// <summary>
		/// Override in custom factory to customize the creation of HttpClient.
		/// </summary>
		public virtual HttpClient CreateClient(Url url) {
			return new HttpClient(new FlurlMessageHandler(CreateClientHandler())) {
				Timeout = FlurlHttp.Configuration.DefaultTimeout
			};
		}

		/// <summary>
		/// Override in custom factory to customize the creation of HttpClientHandler.
		/// </summary>
		public virtual HttpClientHandler CreateClientHandler() {
			return new HttpClientHandler();
		}
	}
}
