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

		protected abstract Task<HttpResponseMessage> CallOnStringAsync(string url);
		protected abstract Task<HttpResponseMessage> CallOnUrlAsync(Url url);
		protected abstract Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req);
		protected abstract HttpResponseMessage CallOnString(string url);
		protected abstract HttpResponseMessage CallOnUrl(Url url);
		protected abstract HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req);

		[Test]
		public async Task can_call_on_FlurlClient_async() {
			await CallOnFlurlRequestAsync(new FlurlRequest("http://www.api.com"));
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public async Task can_call_on_string_async() {
			await CallOnStringAsync("http://www.api.com");
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public async Task can_call_on_url_async() {
			await CallOnUrlAsync(new Url("http://www.api.com"));
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public void can_call_on_FlurlClient()
		{
			CallOnFlurlRequest(new FlurlRequest("http://www.api.com"));
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public void can_call_on_string()
		{
			CallOnString("http://www.api.com");
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}

		[Test]
		public void can_call_on_url()
		{
			CallOnUrl(new Url("http://www.api.com"));
			HttpTest.ShouldHaveCalled("http://www.api.com").WithVerb(_verb).Times(1);
		}
	}

	[TestFixture, Parallelizable]
	public class PutTests : HttpMethodTests
	{
		public PutTests() : base(HttpMethod.Put) { }
		protected override Task<HttpResponseMessage> CallOnStringAsync(string url) => url.PutAsync(null);
		protected override Task<HttpResponseMessage> CallOnUrlAsync(Url url) => url.PutAsync(null);
		protected override Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req) => req.PutAsync(null);
		protected override HttpResponseMessage CallOnString(string url) => url.Put(null);
		protected override HttpResponseMessage CallOnUrl(Url url) => url.Put(null);
		protected override HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req) => req.Put(null);
	}

	[TestFixture, Parallelizable]
	public class PatchTests : HttpMethodTests
	{
		public PatchTests() : base(new HttpMethod("PATCH")) { }
		protected override Task<HttpResponseMessage> CallOnStringAsync(string url) => url.PatchAsync(null);
		protected override Task<HttpResponseMessage> CallOnUrlAsync(Url url) => url.PatchAsync(null);
		protected override Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req) => req.PatchAsync(null);
		protected override HttpResponseMessage CallOnString(string url) => url.Patch(null);
		protected override HttpResponseMessage CallOnUrl(Url url) => url.Patch(null);
		protected override HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req) => req.Patch(null);
	}

	[TestFixture, Parallelizable]
	public class DeleteTests : HttpMethodTests
	{
		public DeleteTests() : base(HttpMethod.Delete) { }
		protected override Task<HttpResponseMessage> CallOnStringAsync(string url) => url.DeleteAsync();
		protected override Task<HttpResponseMessage> CallOnUrlAsync(Url url) => url.DeleteAsync();
		protected override Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req) => req.DeleteAsync();
		protected override HttpResponseMessage CallOnString(string url) => url.Delete();
		protected override HttpResponseMessage CallOnUrl(Url url) => url.Delete();
		protected override HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req) => req.Delete();
	}

	[TestFixture, Parallelizable]
	public class HeadTests : HttpMethodTests
	{
		public HeadTests() : base(HttpMethod.Head) { }
		protected override Task<HttpResponseMessage> CallOnStringAsync(string url) => url.HeadAsync();
		protected override Task<HttpResponseMessage> CallOnUrlAsync(Url url) => url.HeadAsync();
		protected override Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req) => req.HeadAsync();
		protected override HttpResponseMessage CallOnString(string url) => url.Head();
		protected override HttpResponseMessage CallOnUrl(Url url) => url.Head();
		protected override HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req) => req.Head();
	}

	[TestFixture, Parallelizable]
	public class OptionsTests : HttpMethodTests
	{
		public OptionsTests() : base(HttpMethod.Options) { }
		protected override Task<HttpResponseMessage> CallOnStringAsync(string url) => url.OptionsAsync();
		protected override Task<HttpResponseMessage> CallOnUrlAsync(Url url) => url.OptionsAsync();
		protected override Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req) => req.OptionsAsync();
		protected override HttpResponseMessage CallOnString(string url) => url.Options();
		protected override HttpResponseMessage CallOnUrl(Url url) => url.Options();
		protected override HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req) => req.Options();
	}
}
