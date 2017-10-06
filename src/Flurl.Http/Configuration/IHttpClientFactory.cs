using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Interface defining creation of HttpClient and HttpMessageHandler used in all Flurl HTTP calls.
	/// Implementation can be added via FlurlHttp.Configure. However, in order not to lose much of
	/// Flurl.Http's functionality, it's almost always best to inherit DefaultHttpClientFactory and
	/// extend the base implementations, rather than implementing this interface directly.
	/// </summary>
	public interface IHttpClientFactory
	{
		/// <summary>
		/// Defines how HttpClient should be instantiated and configured by default. Do NOT attempt
		/// to cache/reuse HttpClient instances here - that should be done at the FlurlClient level
		/// via a custom FlurlClientFactory that gets registered globally.
		/// </summary>
		/// <param name="handler">The HttpMessageHandler used to construct the HttpClient.</param>
		/// <returns></returns>
		HttpClient CreateHttpClient(HttpMessageHandler handler);

		/// <summary>
		/// Defines how the 
		/// </summary>
		/// <returns></returns>
		HttpMessageHandler CreateMessageHandler();
	}
}