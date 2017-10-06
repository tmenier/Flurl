namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Interface for defining a strategy for creating, caching, and reusing IFlurlClient instances and,
	/// by proxy, their underlying HttpClient instances.
	/// </summary>
	public interface IFlurlClientFactory
	{
		/// <summary>
		/// Strategy to create a FlurlClient or reuse an exisitng one, based on URL being called.
		/// </summary>
		/// <param name="url">The URL being called.</param>
		/// <returns></returns>
		IFlurlClient Get(Url url);
	}
}