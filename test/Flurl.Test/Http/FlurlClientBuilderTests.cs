using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class FlurlClientBuilderTests
	{
		[Test]
		public void can_configure_settings() {
			var builder = new FlurlClientBuilder();
			var cli = builder.WithSettings(s => s.HttpVersion = "3.0").Build();
			Assert.AreEqual("3.0", cli.Settings.HttpVersion);
		}

		[Test]
		public void can_configure_HttpClient() {
			var builder = new FlurlClientBuilder();
			var cli = builder.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://flurl.dev/docs/fluent-http")).Build();
			Assert.AreEqual("https://flurl.dev/docs/fluent-http", cli.HttpClient.BaseAddress.ToString());
			Assert.AreEqual("https://flurl.dev/docs/fluent-http", cli.BaseUrl);
		}

		[Test]
		public async Task can_configure_inner_handler() {
			var builder = new FlurlClientBuilder();
			// Handlers are not accessible beyond HttpClient constructor, making them hard to assert against!
			var cli = builder.ConfigureInnerHandler(h => h.Dispose()).Build();
			try {
				await cli.Request("https://www.google.com").GetAsync();
				Assert.Fail("Should have failed since the inner handler was disposed.");
			}
			catch (FlurlHttpException ex) {
				Assert.IsInstanceOf<ObjectDisposedException>(ex.InnerException);
				StringAssert.EndsWith("Handler", (ex.InnerException as ObjectDisposedException).ObjectName);
			}
		}

		[Test]
		public async Task can_add_middleware() {
			var builder = new FlurlClientBuilder();
			var cli = builder.AddMiddleware(() => new BlockingHandler("blocked by flurl!")).Build();
			var resp = await cli.Request("https://www.google.com").GetStringAsync();
			Assert.AreEqual("blocked by flurl!", resp);
		}

		[Test]
		public void uses_HttpClientHandler_by_default() {
			HttpMessageHandler handler = null;
			new FlurlClientBuilder()
				.ConfigureInnerHandler(h => handler = h)
				.Build();
			Assert.IsInstanceOf<HttpClientHandler>(handler);
		}

#if NET
		[Test]
		public void can_use_SocketsHttpHandler() {
			HttpMessageHandler handler = null;
			new FlurlClientBuilder()
				.UseSocketsHttpHandler(h => handler = h)
				.Build();
			Assert.IsInstanceOf<SocketsHttpHandler>(handler);
		}

		[Test]
		public void cannot_mix_handler_types() {
			Assert.Throws<FlurlConfigurationException>(() => new FlurlClientBuilder()
				.ConfigureInnerHandler(_ => { })
				.UseSocketsHttpHandler(_ => { }));

			// reverse
			Assert.Throws<FlurlConfigurationException>(() => new FlurlClientBuilder()
				.UseSocketsHttpHandler(_ => { })
				.ConfigureInnerHandler(_ => { }));
		}
#endif

		class BlockingHandler : DelegatingHandler
		{
			private readonly string _msg;

			public BlockingHandler(string msg) {
				_msg = msg;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				return Task.FromResult(new HttpResponseMessage { Content = new StringContent(_msg) });
			}
		}
	}
}
