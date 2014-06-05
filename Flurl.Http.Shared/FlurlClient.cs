using System.Net.Http;

namespace Flurl.Http
{
	/// <summary>
	/// A simple container for a Url and an HttpClient, used to enable fluent chaining.
	/// </summary>
	public class FlurlClient
	{
		public FlurlClient(string url) : this(new Url(url)) { }

		public FlurlClient(Url url) {
			this.Url = url;
		}

		private HttpClient _httpClient;

		/// <summary>
		/// Gets the URL to be called in subsequent HTTP calls.
		/// </summary>
		public Url Url { get; private set; }

		/// <summary>
		/// Gets the HttpClient to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.HttpClientFactory. Reused for the life of the FlurlClient.
		/// </summary>
		public HttpClient HttpClient {
			get {
				if (_httpClient == null)
					_httpClient = FlurlHttp.Configuration.HttpClientFactory.CreateClient(Url);
				return _httpClient;
			}
		}
	}
}
