using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class CookieTests : HttpTestFixtureBase
	{
		[Test]
		public async Task can_send_and_receive_cookies_per_request() {
			HttpTest
				.RespondWith("hi", cookies: new { x = "bar" })
				.RespondWith("hi")
				.RespondWith("hi");

			// explicitly reuse client to be extra certain we're NOT maintaining cookie state between calls.
			var cli = new FlurlClient("https://cookies.com");
			var responses = new[] {
				await cli.Request().WithCookie("x", "foo").GetAsync(),
				await cli.Request().WithCookies(new { y = "bar", z = "fizz" }).GetAsync(),
				await cli.Request().GetAsync()
			};

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo" }).Times(1);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { y = "bar", z = "fizz" }).Times(1);
			HttpTest.ShouldHaveMadeACall().WithoutCookies().Times(1);

			Assert.AreEqual("bar", responses[0].Cookies.TryGetValue("x", out var c) ? c.Value : null);
			Assert.IsEmpty(responses[1].Cookies);
			Assert.IsEmpty(responses[2].Cookies);
		}

		[Test]
		public async Task can_send_and_receive_cookies_with_jar() {
			HttpTest
				.RespondWith("hi", cookies: new { x = "foo", y = "bar" })
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			var responses = new[] {
				await "https://cookies.com".WithCookies(out var jar).GetAsync(),
				await "https://cookies.com/1".WithCookies(jar).GetAsync(),
				await "https://cookies.com".WithCookies(jar).GetAsync(),
				await "https://cookies.com/2".WithCookies(jar).GetAsync()
			};

			Assert.AreEqual(2, responses[0].Cookies.Count);
			Assert.AreEqual(0, responses[1].Cookies.Count);
			Assert.AreEqual(1, responses[2].Cookies.Count);
			Assert.AreEqual(0, responses[3].Cookies.Count);

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(2);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

			Assert.AreEqual(2, jar.Count);
			Assert.AreEqual("foo", jar["x"].Value);
			Assert.AreEqual("bazz", jar["y"].Value);
		}

		[Test]
		public async Task can_send_and_receive_cookies_with_jar_initialized() {
			HttpTest
				.RespondWith("hi")
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			var jar = new CookieJar()
				.AddOrUpdate("x", "foo", "https://cookies.com")
				.AddOrUpdate("y", "bar", "https://cookies.com");

			await "https://cookies.com".WithCookies(jar).GetAsync();
			await "https://cookies.com/1".WithCookies(jar).GetAsync();
			await "https://cookies.com".WithCookies(jar).GetAsync();
			await "https://cookies.com/2".WithCookies(jar).GetAsync();

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(3);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

			Assert.AreEqual(2, jar.Count);
			Assert.AreEqual("foo", jar["x"].Value);
			Assert.AreEqual("bazz", jar["y"].Value);
		}

		[Test]
		public async Task can_do_cookie_session() {
			HttpTest
				.RespondWith("hi", cookies: new { x = "foo", y = "bar" })
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			using (var cs = new CookieSession("https://cookies.com")) {
				await cs.Request().GetAsync();
				await cs.Request("1").GetAsync();
				await cs.Request().GetAsync();
				await cs.Request("2").GetAsync();

				var cookies = HttpTest.CallLog.Select(c => c.Request.Cookies).ToList();

				HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(2);
				HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

				Assert.AreEqual(2, cs.Cookies.Count);
				Assert.AreEqual("foo", cs.Cookies["x"].Value);
				Assert.AreEqual("bazz", cs.Cookies["y"].Value);
			}
		}

		[Test]
		public void jar_syncs_with_request_cookies() {
			var jar = new CookieJar().AddOrUpdate("x", "foo", "https://cookies.com");

			var req = new FlurlRequest("http://cookies.com").WithCookies(jar);
			Assert.IsTrue(req.Cookies.ContainsKey("x"));
			Assert.AreEqual("foo", req.Cookies["x"]);

			jar["x"].Value = "new val!";
			Assert.AreEqual("new val!", req.Cookies["x"]);

			jar.AddOrUpdate("y", "bar", "https://cookies.com");
			Assert.IsTrue(req.Cookies.ContainsKey("y"));
			Assert.AreEqual("bar", req.Cookies["y"]);

			jar["x"].Secure = true;
			Assert.IsFalse(req.Cookies.ContainsKey("x"));
		}

		[Test]
		public async Task request_cookies_do_not_sync_to_jar() {
			var jar = new CookieJar().AddOrUpdate("x", "foo", "https://cookies.com");

			var req = new FlurlRequest("http://cookies.com").WithCookies(jar);
			Assert.IsTrue(req.Cookies.ContainsKey("x"));
			Assert.AreEqual("foo", req.Cookies["x"]);

			// changing cookie at request level shouldn't touch jar
			req.Cookies["x"] = "bar";
			Assert.AreEqual("foo", jar["x"].Value);
			
			await req.GetAsync();
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "bar" });

			// re-check after send
			Assert.AreEqual("foo", jar["x"].Value);
		}

		[Test]
		public void request_cookies_sync_with_cookie_header() {
			var req = new FlurlRequest("http://cookies.com").WithCookie("x", "foo");
			Assert.AreEqual("x=foo", req.Headers.TryGetValue("Cookie", out var val) ? val : null);

			// should flow from CookieJar too
			var jar = new CookieJar().AddOrUpdate("y", "bar", "http://cookies.com");
			req = new FlurlRequest("http://cookies.com").WithCookies(jar);
			Assert.AreEqual("y=bar", req.Headers.TryGetValue("Cookie", out val) ? val : null);
		}

		[TestCase("https://domain1.com", "https://domain1.com", true)]
		[TestCase("https://domain1.com", "https://domain1.com/path", true)]
		[TestCase("https://domain1.com", "https://www.domain1.com", false)]
		[TestCase("https://www.domain1.com", "https://domain1.com", false)]
		[TestCase("https://domain1.com", "https://domain2.com", false)]
		public async Task cookies_without_domain_restricted_to_origin_domain(string fromUrl, string toUrl, bool shouldSend) {
			var headers = new Dictionary<string, string> { ["Set-Cookie"] = "x=foo" };
			HttpTest
				.RespondWith("hi", headers: headers)
				.RespondWith("hi");

			await fromUrl.WithCookies(out var cookies).GetAsync();
			Assert.AreEqual(1, cookies.Count);

			await toUrl.WithCookies(cookies).GetAsync();
			if (shouldSend)
				HttpTest.ShouldHaveCalled(toUrl).WithCookie("x");
			else
				HttpTest.ShouldHaveCalled(toUrl).WithoutCookie("x");
		}

		[TestCase("domain.com", "www.domain.com", true)]
		[TestCase("www.domain.com", "domain.com", false)] // not vice-versa
		public async Task cookies_with_domain_sent_to_subdomain(string cookieDomain, string otherDomain, bool shouldSend) {
			var headers = new Dictionary<string, string> { ["Set-Cookie"] = $"x=foo; Domain={cookieDomain}" };
			HttpTest
				.RespondWith("hi", headers: headers)
				.RespondWith("hi");

			await $"https://{cookieDomain}".WithCookies(out var cookies).GetAsync();
			Assert.AreEqual(1, cookies.Count);

			await $"https://{otherDomain}".WithCookies(cookies).GetAsync();
			if (shouldSend)
				HttpTest.ShouldHaveCalled($"https://{otherDomain}").WithCookie("x");
			else
				HttpTest.ShouldHaveCalled($"https://{otherDomain}").WithoutCookie("x");
		}
	}
}
