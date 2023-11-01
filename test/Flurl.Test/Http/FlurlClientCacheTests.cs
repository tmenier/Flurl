using System;
using Flurl.Http;
using Flurl.Http.Configuration;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class FlurlClientCacheTests
	{
		[Test]
		public void can_add_and_get_client() {
			var cache = new FlurlClientCache();
			cache.Add("github", "https://api.github.com").WithSettings(s => s.Timeout = TimeSpan.FromSeconds(123));
			cache.Add("google", "https://api.google.com").WithSettings(s => s.Timeout = TimeSpan.FromSeconds(234));

			var gh = cache.Get("github");
			Assert.AreEqual("https://api.github.com", gh.BaseUrl);
			Assert.AreEqual(123, gh.Settings.Timeout.Value.TotalSeconds);
			Assert.AreSame(gh, cache.Get("github"), "should reuse same-named clients.");

			var goog = cache.Get("google");
			Assert.AreEqual("https://api.google.com", goog.BaseUrl);
			Assert.AreEqual(234, goog.Settings.Timeout.Value.TotalSeconds);
			Assert.AreSame(goog, cache.Get("google"), "should reuse same-named clients.");
		}

		[Test]
		public void can_get_or_add_client() {
			var cache = new FlurlClientCache();
			IFlurlClient firstCli = null;
			var configCalls = 0;

			for (var i = 0; i < 10; i++) {
				var cli = cache.GetOrAdd("foo", "https://api.com", _ => configCalls++);
				if (i == 0)
					firstCli = cli;
				else
					Assert.AreSame(firstCli, cli);
			}

			Assert.AreEqual("https://api.com", firstCli.BaseUrl);
			Assert.AreEqual(1, configCalls);
		}

		[Test]
		public void can_get_unconfigured_client() {
			var cache = new FlurlClientCache();
			var cli = cache.Get("foo");
			Assert.IsNull(cli.BaseUrl);
			Assert.AreEqual(100, cli.Settings.Timeout.Value.TotalSeconds);

			Assert.AreSame(cli, cache.Get("foo"), "should reuse same-named clients.");
			Assert.AreNotSame(cli, cache.Get("bar"), "different-named clients should be different instances.");
		}

		[Test]
		public void cannot_add_same_name_twice() {
			var cache = new FlurlClientCache();
			cache.Add("foo", "https://api.github.com");
			Assert.Throws<ArgumentException>(() => cache.Add("foo", "https://api.google.com"));
		}
		
		[Test]
		public void can_configure_all() {
			var cache = new FlurlClientCache()
				.ConfigureAll(b => b.WithSettings(s => s.Timeout = TimeSpan.FromSeconds(123)));

			var cli1 = cache.Get("foo");

			cache.Add("bar").WithSettings(s => {
				s.Timeout = TimeSpan.FromSeconds(456);
			});
			cache.ConfigureAll(b => b.WithSettings(s => {
				s.Timeout = TimeSpan.FromSeconds(789);
			}));

			var cli2 = cache.Get("bar");
			var cli3 = cache.Get("buzz");

			Assert.AreEqual(123, cli1.Settings.Timeout.Value.TotalSeconds, "fetched the client before changing the defaults, so original should stick");
			Assert.AreEqual(456, cli2.Settings.Timeout.Value.TotalSeconds, "client-specific settings should always win, even if defaults were changed after");
			Assert.AreEqual(789, cli3.Settings.Timeout.Value.TotalSeconds, "newer defaults should squash older");
		}

		[Test]
		public void can_remove() {
			// hard to test directly, but if we get a new instance after deleting, safe to assume it works.
			var cache = new FlurlClientCache();
			cache.Add("foo");

			var cli1 = cache.Get("foo");
			var cli2 = cache.Get("foo");
			cache.Remove("foo");
			var cli3 = cache.Get("foo");

			Assert.AreSame(cli1, cli2);
			Assert.AreNotSame(cli1, cli3);

			Assert.IsTrue(cli1.IsDisposed);
			Assert.IsTrue(cli2.IsDisposed);
			Assert.IsFalse(cli3.IsDisposed);
		}
	}
}
