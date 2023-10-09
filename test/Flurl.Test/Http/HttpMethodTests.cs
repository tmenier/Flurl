using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// Each HTTP method with first-class support in Flurl (via PostAsync, GetAsync, etc.) should
	/// have a test fixture that inherits from this base class.
	/// </summary>
	public abstract class HttpMethodTests : HttpTestFixtureBase
	{
		private readonly HttpMethod _verb;

		protected HttpMethodTests(HttpMethod verb) {
			_verb = verb;
		}

		protected abstract Task<IFlurlResponse> CallOnString(string url);
		protected abstract Task<IFlurlResponse> CallOnUrl(Url url);
		protected abstract Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req);

		[Test]
		public async Task can_call_on_FlurlClient() {
			var resp = await CallOnFlurlRequest(new FlurlRequest("http://www.api.com"));
			Assert.AreEqual(200, resp.StatusCode);
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public async Task can_call_on_string() {
			var resp = await CallOnString("http://www.api.com");
			Assert.AreEqual(200, resp.StatusCode);
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public async Task can_call_on_url() {
			var resp = await CallOnUrl(new Url("http://www.api.com"));
			Assert.AreEqual(200, resp.StatusCode);
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}
	}

	[TestFixture, Parallelizable]
	public class PutTests : HttpMethodTests
	{
		public PutTests() : base(HttpMethod.Put) { }
		protected override Task<IFlurlResponse> CallOnString(string url) => url.PutAsync(null);
		protected override Task<IFlurlResponse> CallOnUrl(Url url) => url.PutAsync(null);
		protected override Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req) => req.PutAsync(null);
	}

	[TestFixture, Parallelizable]
	public class PatchTests : HttpMethodTests
	{
		public PatchTests() : base(new HttpMethod("PATCH")) { }
		protected override Task<IFlurlResponse> CallOnString(string url) => url.PatchAsync(null);
		protected override Task<IFlurlResponse> CallOnUrl(Url url) => url.PatchAsync(null);
		protected override Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req) => req.PatchAsync(null);
	}

	[TestFixture, Parallelizable]
	public class DeleteTests : HttpMethodTests
	{
		public DeleteTests() : base(HttpMethod.Delete) { }
		protected override Task<IFlurlResponse> CallOnString(string url) => url.DeleteAsync();
		protected override Task<IFlurlResponse> CallOnUrl(Url url) => url.DeleteAsync();
		protected override Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req) => req.DeleteAsync();
	}

	[TestFixture, Parallelizable]
	public class HeadTests : HttpMethodTests
	{
		public HeadTests() : base(HttpMethod.Head) { }
		protected override Task<IFlurlResponse> CallOnString(string url) => url.HeadAsync();
		protected override Task<IFlurlResponse> CallOnUrl(Url url) => url.HeadAsync();
		protected override Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req) => req.HeadAsync();
	}

	[TestFixture, Parallelizable]
	public class OptionsTests : HttpMethodTests
	{
		public OptionsTests() : base(HttpMethod.Options) { }
		protected override Task<IFlurlResponse> CallOnString(string url) => url.OptionsAsync();
		protected override Task<IFlurlResponse> CallOnUrl(Url url) => url.OptionsAsync();
		protected override Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req) => req.OptionsAsync();
	}
}
