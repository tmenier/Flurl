using Flurl.Http.Content;

namespace Flurl.AspNetCore.TestApp
{
	public class MiddlewareDependency
	{
		public string GetString() => "I'm from the middleware's dependency!";
	}

	public class TestMiddleware : DelegatingHandler
	{
		private readonly MiddlewareDependency _dep;

		public TestMiddleware(MiddlewareDependency dep) {
			_dep = dep;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			return Task.FromResult(new HttpResponseMessage {
				Content = new CapturedStringContent(_dep.GetString())
			});
		}
	}
}
