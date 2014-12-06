using System.Net;
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
        public virtual HttpClient CreateClient(Url url) {

            FlurlMessageHandler flurlHandler;
            if (url.CookieContainer != null)
                flurlHandler = new FlurlMessageHandler(new HttpClientHandler() { CookieContainer = url.CookieContainer });
            else
                flurlHandler = new FlurlMessageHandler();
            return new HttpClient(flurlHandler) {
				Timeout = FlurlHttp.Configuration.DefaultTimeout
			};
		}
	}
}
