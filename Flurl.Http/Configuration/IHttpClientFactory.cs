using System.Net.Http;

namespace Flurl.Http.Configuration
{
	public interface IHttpClientFactory
	{
		HttpClient CreateClient(Url url);
	}
}
