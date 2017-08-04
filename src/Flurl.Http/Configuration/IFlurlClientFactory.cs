using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Interface defining creation of HttpClient and HttpMessageHandler used in all Flurl HTTP calls.
	/// Implementation can be added via FlurlHttp.Configure. However, in order not to lose much of
	/// Flurl.Http's functionality, it's almost always best to inherit DefaultFlurlClientFactory and
	/// extend the base implementations, rather than implementing this interface directly.
	/// </summary>
	public interface IFlurlClientFactory
	{
		/// <summary>
		/// Creates the client.
		/// </summary>
		/// <param name="handler">The message handler being used in this call</param>
		/// <returns></returns>
		HttpClient CreateHttpClient(HttpMessageHandler handler);
		
		/// <summary>
		/// Creates the message handler.
		/// </summary>
		/// <returns></returns>
		HttpMessageHandler CreateMessageHandler();

		/// <summary>
		/// Strategy to create an HttpClient or reuse an exisitng one, based on URL being called.
		/// </summary>
		/// <param name="url">The URL being called.</param>
		/// <returns></returns>
		IFlurlClient GetClient(Url url);
	}
}