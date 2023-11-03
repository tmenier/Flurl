using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class FlurlClientTests
	{
		[Test]
		// check that for every FlurlClient extension method, we have an equivalent Url and string extension
		public void extension_methods_consistently_supported() {
			var reqExts = ReflectionHelper.GetAllExtensionMethods<IFlurlRequest>(typeof(FlurlClient).GetTypeInfo().Assembly)
				// URL builder methods on IFlurlClient get a free pass. We're looking for things like HTTP calling methods.
				.Where(mi => mi.DeclaringType != typeof(UrlBuilderExtensions))
				.ToList();
			var urlExts = ReflectionHelper.GetAllExtensionMethods<Url>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();
			var uriExts = ReflectionHelper.GetAllExtensionMethods<Uri>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();

			Assert.That(reqExts.Count > 20, $"IFlurlRequest only has {reqExts.Count} extension methods? Something's wrong here.");

			// Url and string should contain all extension methods that IFlurlRequest has
			foreach (var method in reqExts) {
				if (!urlExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent Url extension method found for IFlurlRequest.{method.Name}({args})");
				}
				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent string extension method found for IFlurlRequest.{method.Name}({args})");
				}
				if (!uriExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent System.Uri extension method found for IFlurlRequest.{method.Name}({args})");
				}
			}
		}

		[Test]
		public void can_create_request_without_base_url() {
			var cli = new FlurlClient();
			var req = cli.Request("http://myapi.com/foo?x=1&y=2#foo");
			Assert.AreEqual("http://myapi.com/foo?x=1&y=2#foo", req.Url.ToString());
		}

		[Test]
		public void can_create_request_with_base_url() {
			var cli = new FlurlClient("http://myapi.com");
			var req = cli.Request("foo", "bar");
			Assert.AreEqual("http://myapi.com/foo/bar", req.Url.ToString());
		}

		[Test]
		public void request_with_full_url_overrides_base_url() {
			var cli = new FlurlClient("http://myapi.com");
			var req = cli.Request("http://otherapi.com", "foo");
			Assert.AreEqual("http://otherapi.com/foo", req.Url.ToString());
		}

		[Test]
		public void can_create_request_with_base_url_and_no_segments() {
			var cli = new FlurlClient("http://myapi.com");
			var req = cli.Request();
			Assert.AreEqual("http://myapi.com", req.Url.ToString());
		}

		[Test]
		public void can_create_request_with_Uri() {
			var uri = new System.Uri("http://www.mysite.com/foo?x=1");
			var req = new FlurlClient().Request(uri);
			Assert.AreEqual(uri.ToString(), req.Url.ToString());
		}

		[Test]
		public void cannot_send_invalid_request() {
			var cli = new FlurlClient();
			Assert.ThrowsAsync<ArgumentNullException>(() => cli.SendAsync(null));
			Assert.ThrowsAsync<ArgumentException>(() => cli.SendAsync(new FlurlRequest()));
			Assert.ThrowsAsync<ArgumentException>(() => cli.SendAsync(new FlurlRequest("/relative/url")));
		}

		[Test]
		public async Task default_factory_doesnt_reuse_disposed_clients() {
			var req1 = "http://api.com".WithHeader("foo", "1");
			var req2 = "http://api.com".WithHeader("foo", "2");
			var req3 = "http://api.com".WithHeader("foo", "3");
			
			// client not assigned until request is sent
			using var test = new HttpTest();
			await req1.GetAsync();
			await req2.GetAsync();
			req1.Client.Dispose();
			await req3.GetAsync();

			Assert.AreEqual(req1.Client, req2.Client);
			Assert.IsTrue(req1.Client.IsDisposed);
			Assert.IsTrue(req2.Client.IsDisposed);
			Assert.AreNotEqual(req1.Client, req3.Client);
			Assert.IsFalse(req3.Client.IsDisposed);
		}

		[Test]
		public void can_create_FlurlClient_with_existing_HttpClient() {
			var hc = new HttpClient {
				BaseAddress = new Uri("http://api.com/"),
				Timeout = TimeSpan.FromSeconds(123)
			};
			var cli = new FlurlClient(hc);

			Assert.AreSame(hc, cli.HttpClient);
			Assert.AreEqual("http://api.com/", cli.BaseUrl);
			Assert.AreEqual(123, cli.Settings.Timeout?.TotalSeconds);
		}

		[Test] // #334
		public void can_dispose_FlurlClient_created_with_HttpClient() {
			var hc = new HttpClient();
			var fc = new FlurlClient(hc);
			fc.Dispose();

			// ensure the HttpClient got disposed
			Assert.ThrowsAsync<ObjectDisposedException>(() => hc.GetAsync("http://mysite.com"));
		}
	}
}