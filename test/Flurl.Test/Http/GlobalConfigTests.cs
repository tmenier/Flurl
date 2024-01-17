using System;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, NonParallelizable]
	public class GlobalConfigTests : HttpTestFixtureBase
	{
		[SetUp, TearDown]
		public void ResetGlobalDefaults() {
			FlurlHttp.UseClientPerHostStrategy();
			FlurlHttp.Clients.Clear();
		}

		// different path - share client
		[TestCase("https://api.com/some/path", "https://api.com/other/path", true)]
		// different userinfo - share client
		[TestCase("https://api.com", "https://user:pass@api.com", true)]
		// different host - don't share
		[TestCase("https://api.com", "https://www.api.com", false)]
		// different port - don't share
		[TestCase("https://api.com", "https://api.com:3333", false)]
		// different scheme - don't share
		[TestCase("http://api.com", "https://api.com", false)]
		public void default_caching_strategy_provides_same_client_per_host_scheme_port(string url1, string url2, bool sameClient) {
			var cli1 = FlurlHttp.GetClientForRequest(new FlurlRequest(url1));
			var cli2 = FlurlHttp.GetClientForRequest(new FlurlRequest(url2));

			if (sameClient)
				Assert.AreSame(cli1, cli2);
			else
				Assert.AreNotSame(cli1, cli2);
		}

		[Test]
		public async Task can_replace_caching_strategy() {
			FlurlHttp.UseClientCachingStrategy(req => "shared");
			Assert.AreSame(
				FlurlHttp.GetClientForRequest(new FlurlRequest("https://a.com")),
				FlurlHttp.GetClientForRequest(new FlurlRequest("https://b.com")));

			var req1 = new FlurlRequest("https://a.com");
			await req1.GetAsync();
			var req2 = new FlurlRequest("https://b.com");
			await req2.GetAsync();

			Assert.AreSame(req1.Client, req2.Client, "custom strategy uses same client for all calls");

			FlurlHttp.UseClientPerHostStrategy();
			Assert.AreNotSame(
				FlurlHttp.GetClientForRequest(new FlurlRequest("https://a.com")),
				FlurlHttp.GetClientForRequest(new FlurlRequest("https://b.com")));

			req1 = new FlurlRequest("https://a.com");
			await req1.GetAsync();
			req2 = new FlurlRequest("https://b.com");
			await req2.GetAsync();

			Assert.AreNotSame(req1.Client, req2.Client, "default strategy uses different client per host");
		}

		[Test]
		public void can_configure_client_for_url() {
			FlurlHttp.ConfigureClientForUrl("https://api.com/some/path")
				.WithSettings(s => s.Timeout = TimeSpan.FromSeconds(123));

			var cli1 = FlurlHttp.GetClientForRequest(new FlurlRequest("https://api.com"));
			var cli2 = FlurlHttp.GetClientForRequest(new FlurlRequest("https://api.com/differernt/path"));
			var cli3 = FlurlHttp.GetClientForRequest(new FlurlRequest("https://api.com:3333"));

			Assert.AreEqual(123, cli1.Settings.Timeout.Value.TotalSeconds);
			Assert.AreEqual(123, cli2.Settings.Timeout.Value.TotalSeconds);
			Assert.AreNotEqual(123, cli3.Settings.Timeout.Value.TotalSeconds);
		}

		[Test] // #803
		public async Task can_prepend_global_base_url() {
			FlurlHttp.Clients.WithDefaults(builder =>
				builder.ConfigureHttpClient(cli => cli.BaseAddress = new Uri("https://api.com")));

			using var test = new HttpTest();

			await "".GetAsync();
			await "/path/1".GetAsync();
			await "path/2".GetAsync();
			await "https://other.com/path/3".GetAsync();

			test.ShouldHaveCalled("https://api.com/").Times(1);
			test.ShouldHaveCalled("https://api.com/path/1").Times(1);
			test.ShouldHaveCalled("https://api.com/path/2").Times(1);
			test.ShouldHaveCalled("https://other.com/path/3").Times(1);
		}
	}
}
