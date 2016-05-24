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
		/// Creates the client.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="handler">The handler.</param>
		/// <returns></returns>
		HttpClient CreateClient(Url url, HttpMessageHandler handler);
		
		/// <summary>
		/// Creates the message handler.
		/// </summary>
		/// <returns></returns>
		HttpMessageHandler CreateMessageHandler();
	}
}