using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class CookieTests : HttpTestFixtureBase
	{
		[Test]
		public async Task can_send_cookies_per_request() {
			HttpTest
				.RespondWith("hi", cookies: new { x = "foo", y = "bar" })
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			var responses = new[] {
				await "https://cookies.com".WithCookies(out var cookies).GetAsync(),
				await "https://cookies.com/1".WithCookies(cookies).GetAsync(),
				await "https://cookies.com".WithCookies(cookies).GetAsync(),
				await "https://cookies.com/2".WithCookies(cookies).GetAsync()
			};

			Assert.AreEqual(2, responses[0].Cookies.Count);
			Assert.AreEqual(0, responses[1].Cookies.Count);
			Assert.AreEqual(1, responses[2].Cookies.Count);
			Assert.AreEqual(0, responses[3].Cookies.Count);

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(2);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

			Assert.AreEqual(2, cookies.Count);
			Assert.AreEqual("foo", cookies["x"].Value);
			Assert.AreEqual("bazz", cookies["y"].Value);
		}

		[Test]
		public void jar_syncs_with_request_cookies() {
			var jar = new CookieJar();
			jar.AddOrUpdate("x", "foo", "https://cookies.com");

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
		public async Task can_send_cookies_per_request_initialized() {
			HttpTest
				.RespondWith("hi")
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			var cookies = new CookieJar()
				.AddOrUpdate("x", "foo", "https://cookies.com")
				.AddOrUpdate("y", "bar", "https://cookies.com");

			await "https://cookies.com".WithCookies(cookies).GetAsync();
			await "https://cookies.com/1".WithCookies(cookies).GetAsync();
			await "https://cookies.com".WithCookies(cookies).GetAsync();
			await "https://cookies.com/2".WithCookies(cookies).GetAsync();

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(3);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

			Assert.AreEqual(2, cookies.Count);
			Assert.AreEqual("foo", cookies["x"].Value);
			Assert.AreEqual("bazz", cookies["y"].Value);
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
