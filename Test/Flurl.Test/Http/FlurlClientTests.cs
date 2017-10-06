using System;
using System.Linq;
using System.Reflection;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class FlurlClientTests
	{
		[Test]
		// check that for every FlurlClient extension method, we have an equivalent Url and string extension
		public void extension_methods_consistently_supported() {
			var frExts = ReflectionHelper.GetAllExtensionMethods<IFlurlRequest>(typeof(FlurlClient).GetTypeInfo().Assembly)
				// URL builder methods on IFlurlClient get a free pass. We're looking for things like HTTP calling methods.
				.Where(mi => mi.DeclaringType != typeof(UrlBuilderExtensions))
				.ToList();
			var urlExts = ReflectionHelper.GetAllExtensionMethods<Url>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();

			Assert.That(frExts.Count > 20, $"IFlurlRequest only has {frExts.Count} extension methods? Something's wrong here.");

			// Url and string should contain all extension methods that IFlurlRequest has
			foreach (var method in frExts) {
				if (!urlExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent Url extension method found for IFlurlRequest.{method.Name}({args})");
				}
				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent string extension method found for IFlurlRequest.{method.Name}({args})");
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
		public void cannot_create_request_without_base_url_or_segments() {
			var cli = new FlurlClient();
			Assert.Throws<ArgumentException>(() => {
				var req = cli.Request();
			});
		}

		[Test]
		public void cannot_create_request_without_base_url_or_segments_comprising_full_url() {
			var cli = new FlurlClient();
			Assert.Throws<ArgumentException>(() => {
				var req = cli.Request("foo", "bar");
			});
		}

		[Test]
		public void default_factory_doesnt_reuse_disposed_clients() {
			var cli1 = "http://api.com".WithHeader("foo", "1").Client;
			var cli2 = "http://api.com".WithHeader("foo", "2").Client;
			cli1.Dispose();
			var cli3 = "http://api.com".WithHeader("foo", "3").Client;

			Assert.AreEqual(cli1, cli2);
			Assert.IsTrue(cli1.IsDisposed);
			Assert.IsTrue(cli2.IsDisposed);
			Assert.AreNotEqual(cli1, cli3);
			Assert.IsFalse(cli3.IsDisposed);
		}
	}
}