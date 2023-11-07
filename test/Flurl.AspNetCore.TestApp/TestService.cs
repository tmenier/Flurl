using Flurl.Http;
using Flurl.Http.Configuration;

namespace Flurl.AspNetCore.TestApp
{
	public class TestService
	{
		private readonly IFlurlClient _flurlClient;

		public TestService(IFlurlClientCache flurlCache) {
			_flurlClient = flurlCache.Get("foo");
		}

		public Task<string> CallServiceAsync() =>
			_flurlClient.Request("xyz").GetStringAsync();
	}
}
