using System;
using System.Linq;
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

			Assert.AreEqual("bar", responses[0].Cookies.FirstOrDefault(c => c.Name == "x")?.Value);
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
			Assert.AreEqual(1, jar.Count(c => c.Name == "x" && c.Value == "foo"));
			Assert.AreEqual(1, jar.Count(c => c.Name == "y" && c.Value == "bazz"));
		}

		[Test]
		public async Task can_send_and_receive_cookies_with_jar_initialized() {
			HttpTest
				.RespondWith("hi")
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			var jar = new CookieJar()
				.AddOrReplace("x", "foo", "https://cookies.com")
				.AddOrReplace("y", "bar", "https://cookies.com");

			await "https://cookies.com".WithCookies(jar).GetAsync();
			await "https://cookies.com/1".WithCookies(jar).GetAsync();
			await "https://cookies.com".WithCookies(jar).GetAsync();
			await "https://cookies.com/2".WithCookies(jar).GetAsync();

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(3);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

			Assert.AreEqual(2, jar.Count);
			Assert.AreEqual(1, jar.Count(c => c.Name == "x" && c.Value == "foo"));
			Assert.AreEqual(1, jar.Count(c => c.Name == "y" && c.Value == "bazz"));
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
				Assert.AreEqual(1, cs.Cookies.Count(c => c.Name == "x" && c.Value == "foo"));
				Assert.AreEqual(1, cs.Cookies.Count(c => c.Name == "y" && c.Value == "bazz"));
			}
		}

		[Test]
		public void can_parse_set_cookie_header() {
			var start = DateTimeOffset.UtcNow;
			var cookie = CookieCutter.ParseResponseHeader("https://www.cookies.com/a/b", "x=foo  ; DoMaIn=cookies.com  ;     path=/  ; MAX-AGE=999 ; expires= ;  secure ;HTTPONLY ;samesite=none");
			Assert.AreEqual("https://www.cookies.com/a/b", cookie.OriginUrl.ToString());
			Assert.AreEqual("x", cookie.Name);
			Assert.AreEqual("foo", cookie.Value);
			Assert.AreEqual("cookies.com", cookie.Domain);
			Assert.AreEqual("/", cookie.Path);
			Assert.AreEqual(999, cookie.MaxAge);
			Assert.IsNull(cookie.Expires);
			Assert.IsTrue(cookie.Secure);
			Assert.IsTrue(cookie.HttpOnly);
			Assert.AreEqual(SameSite.None, cookie.SameSite);
			Assert.GreaterOrEqual(cookie.DateReceived, start);
			Assert.LessOrEqual(cookie.DateReceived, DateTimeOffset.UtcNow);

			// simpler case
			start = DateTimeOffset.UtcNow;
			cookie = CookieCutter.ParseResponseHeader("https://www.cookies.com/a/b", "y=bar");
			Assert.AreEqual("https://www.cookies.com/a/b", cookie.OriginUrl.ToString());
			Assert.AreEqual("y", cookie.Name);
			Assert.AreEqual("bar", cookie.Value);
			Assert.IsNull(cookie.Domain);
			Assert.IsNull(cookie.Path);
			Assert.IsNull(cookie.MaxAge);
			Assert.IsNull(cookie.Expires);
			Assert.IsFalse(cookie.Secure);
			Assert.IsFalse(cookie.HttpOnly);
			Assert.IsNull(cookie.SameSite);
			Assert.GreaterOrEqual(cookie.DateReceived, start);
			Assert.LessOrEqual(cookie.DateReceived, DateTimeOffset.UtcNow);
		}

		[Test]
		public void cannot_change_cookie_after_adding_to_jar() {
			var cookie = new FlurlCookie("x", "foo", "https://cookies.com");

			// good
			cookie.Value = "value2";
			cookie.Path = "/";
			cookie.Secure = true;

			var jar = new CookieJar().AddOrReplace(cookie);

			// bad
			Assert.Throws<Exception>(() => cookie.Value = "value3");
			Assert.Throws<Exception>(() => cookie.Path = "/a");
			Assert.Throws<Exception>(() => cookie.Secure = false);
		}

		[Test]
		public void url_decodes_cookie_value() {
			var cookie = CookieCutter.ParseResponseHeader("https://cookies.com", "x=one%3A%20for%20the%20money");
			Assert.AreEqual("one: for the money", cookie.Value);
		}

		[Test]
		public void unquotes_cookie_value() {
			var cookie = CookieCutter.ParseResponseHeader("https://cookies.com", "x=\"hello there\"" );
			Assert.AreEqual("hello there", cookie.Value);
		}

		[Test]
		public void jar_overwrites_request_cookies() {
			var jar = new CookieJar()
				.AddOrReplace("b", 10, "https://cookies.com")
				.AddOrReplace("c", 11, "https://cookies.com");

			var req = new FlurlRequest("http://cookies.com")
				.WithCookies(new { a = 1, b = 2 })
				.WithCookies(jar);

			Assert.AreEqual(3, req.Cookies.Count());
			Assert.IsTrue(req.Cookies.Contains(("a", "1")));
			Assert.IsTrue(req.Cookies.Contains(("b", "10"))); // the important one
			Assert.IsTrue(req.Cookies.Contains(("c", "11")));
		}

		[Test]
		public void request_cookies_do_not_overwrite_jar() {
			var jar = new CookieJar()
				.AddOrReplace("b", 10, "https://cookies.com")
				.AddOrReplace("c", 11, "https://cookies.com");

			var req = new FlurlRequest("http://cookies.com")
				.WithCookies(jar)
				.WithCookies(new { a = 1, b = 2 });

			Assert.AreEqual(3, req.Cookies.Count());
			Assert.IsTrue(req.Cookies.Contains(("a", "1")));
			Assert.IsTrue(req.Cookies.Contains(("b", "2"))); // value overwritten but just for this request
			Assert.IsTrue(req.Cookies.Contains(("c", "11")));

			// b in jar wasn't touched
			Assert.AreEqual("10", jar.FirstOrDefault(c => c.Name == "b")?.Value);
		}

		[Test]
		public void request_cookies_sync_with_cookie_header() {
			var req = new FlurlRequest("http://cookies.com").WithCookie("x", "foo");
			Assert.AreEqual("x=foo", req.Headers.FirstOrDefault("Cookie"));

			// should flow from CookieJar too
			var jar = new CookieJar().AddOrReplace("y", "bar", "http://cookies.com");
			req = new FlurlRequest("http://cookies.com").WithCookies(jar);
			Assert.AreEqual("y=bar", req.Headers.FirstOrDefault("Cookie"));
		}

		[TestCase("https://domain1.com", "https://domain1.com", true)]
		[TestCase("https://domain1.com", "https://domain1.com/path", true)]
		[TestCase("https://domain1.com", "https://www.domain1.com", false)]
		[TestCase("https://www.domain1.com", "https://domain1.com", false)]
		[TestCase("https://domain1.com", "https://domain2.com", false)]
		public void cookies_without_domain_restricted_to_origin_domain(string originUrl, string requestUrl, bool shouldSend) {
			var cookie = new FlurlCookie("x", "foo", originUrl);
			AssertCookie(cookie, true, false, requestUrl, shouldSend);
		}

		[TestCase("domain.com", "https://www.domain.com", true)]
		[TestCase("www.domain.com", "https://domain.com", false)] // not vice-versa
		public void cookies_with_domain_sent_to_subdomain(string cookieDomain, string requestUrl, bool shouldSend) {
			var cookie = new FlurlCookie("x", "foo", $"https://{cookieDomain}") { Domain = cookieDomain };
			AssertCookie(cookie, true, false, requestUrl, shouldSend);
		}

		[TestCase("/a", "/a", true)]
		[TestCase("/a", "/a/", true)]
		[TestCase("/a", "/a/hello", true)]
		[TestCase("/a", "/ab", false)]
		[TestCase("/a", "/", false)]
		public void cookies_with_path_sent_to_subpath(string cookiePath, string requestPath, bool shouldSend) {
			var origin = "https://cookies.com".AppendPathSegment(cookiePath);
			var cookie = new FlurlCookie("x", "foo", origin) { Path = cookiePath };
			var url = "https://cookies.com".AppendPathSegment(requestPath);
			AssertCookie(cookie, true, false, url, shouldSend);
		}

		// default path is /
		[TestCase("", "/", true)]
		[TestCase("", "/a/b/c", true)]
		[TestCase("/", "", true)]
		[TestCase("/", "/a/b/c", true)]
		[TestCase("/a", "", true)]
		[TestCase("/a", "/", true)]
		[TestCase("/a", "/a/b/c", true)]
		[TestCase("/a", "/x", true)]

		// default path is /a
		[TestCase("/a/", "", false)]
		[TestCase("/a/", "/", false)]
		[TestCase("/a/", "/a", true)]
		[TestCase("/a/", "/a/b/c", true)]
		[TestCase("/a/", "/a/x", true)]
		[TestCase("/a/", "/x", false)]
		[TestCase("/a/b", "", false)]
		[TestCase("/a/b", "/", false)]
		[TestCase("/a/b", "/a", true)]
		[TestCase("/a/b", "/a/b/c", true)]
		[TestCase("/a/b", "/a/x", true)]
		[TestCase("/a/b", "/x", false)]
		public void cookies_without_path_sent_to_origin_subpath(string originPath, string requestPath, bool shouldSend) {
			var origin = "https://cookies.com" + originPath;
			var cookie = new FlurlCookie("x", "foo", origin);
			var url = "https://cookies.com".AppendPathSegment(requestPath);
			AssertCookie(cookie, true, false, url, shouldSend);
		}

		[Test]
		public void secure_cookies_not_sent_to_insecure_url() {
			var cookie = new FlurlCookie("x", "foo", "https://cookies.com") { Secure = true };
			AssertCookie(cookie, true, false, "https://cookies.com", true);
			AssertCookie(cookie, true, false, "http://cookies.com", false);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void validates_expired_absolute(bool localTime) {
			var now = localTime ? DateTimeOffset.Now : DateTimeOffset.UtcNow;
			var c1 = new FlurlCookie("x", "foo", "https://cookies.com") { Expires = now.AddSeconds(-2) };
			var c2 = new FlurlCookie("x", "foo", "https://cookies.com") { Expires = now.AddSeconds(2) };
			AssertCookie(c1, true, true, "https://cookies.com", false);
			AssertCookie(c2, true, false, "https://cookies.com", true);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void validates_expired_by_max_age(bool localTime) {
			var now = localTime ? DateTimeOffset.Now : DateTimeOffset.UtcNow;
			var c1 = new FlurlCookie("x", "foo", "https://cookies.com", now.AddSeconds(-3602)) { MaxAge = 3600 };
			var c2 = new FlurlCookie("x", "foo", "https://cookies.com", now.AddSeconds(-3598)) { MaxAge = 3600 };
			AssertCookie(c1, true, true, "https://cookies.com", false);
			AssertCookie(c2, true, false, "https://cookies.com", true);
		}

		[TestCase(null, true)]
		[TestCase("", true)] // spec says SHOULD ignore empty
		[TestCase("cookies", false)]
		[TestCase("cookies.com", true)]
		[TestCase(".cookies.com", true)] // "Contrary to earlier specifications, leading dots in domain names are ignored" https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie
		[TestCase("ww.cookies.com", false)]
		[TestCase("www.cookies.com", true)]
		[TestCase(".www.cookies.com", true)]
		[TestCase("wwww.cookies.com", false)]
		[TestCase("cookies2.com", false)]
		[TestCase("mycookies.com", false)]
		[TestCase("https://www.cookies.com", false)]
		[TestCase("https://www.cookies.com/a", false)]
		[TestCase("www.cookies.com/a", false)]
		public void validates_domain(string domain, bool valid) {
			var cookie = new FlurlCookie("x", "foo", "https://www.cookies.com/a") { Domain = domain };
			AssertCookie(cookie, valid, false);
		}

		[Test]
		public void domain_cannot_be_ip_address() {
			var cookie = new FlurlCookie("x", "foo", "https://1.2.3.4");
			AssertCookie(cookie, true, false);

			// domain can't be set at all if origin is an IP, but that's kind of impossible to
			// test independently because it'll fail the domain match check first
			cookie = new FlurlCookie("x", "foo", "https://1.2.3.4") { Domain = "1.2.3.4" };
			AssertCookie(cookie, false, false);
		}

		[Test]
		public void validates_secure() {
			var cookie = new FlurlCookie("x", "foo", "http://insecure.com") { Secure = true };
			AssertCookie(cookie, false, false);
		}

		// https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies#Cookie_prefixes
		[TestCase("__Host-", "https://cookies.com", true, null, "/", true)]
		[TestCase("__Host-", "http://cookies.com", true, null, "/", false)]
		[TestCase("__Host-", "https://cookies.com", false, null, "/", false)]
		[TestCase("__Host-", "https://cookies.com", true, "cookies.com", "/", false)]
		[TestCase("__Host-", "https://cookies.com", true, null, null, false)]
		[TestCase("__Host-", "https://cookies.com", true, null, "/a", false)]

		[TestCase("__Secure-", "https://cookies.com", true, null, "/", true)]
		[TestCase("__Secure-", "http://cookies.com", true, null, "/", false)]
		[TestCase("__Secure-", "https://cookies.com", false, null, "/", false)]
		[TestCase("__Secure-", "https://cookies.com", true, "cookies.com", "/", true)]
		[TestCase("__Secure-", "https://cookies.com", true, null, null, true)]
		[TestCase("__Secure-", "https://cookies.com", true, null, "/a", true)]
		public void validates_cookie_prefix(string prefix, string origin, bool secure, string domain, string path, bool valid) {
			var cookie = new FlurlCookie(prefix + "x", "foo", origin.AppendPathSegment("a")) {
				Secure = secure,
				Domain = domain,
				Path = path
			};
			AssertCookie(cookie, valid, false);
		}

		[Test]
		public async Task invalid_cookie_in_response_doesnt_throw() {
			HttpTest.RespondWith("hi", headers: new { Set_Cookie = "x=foo; Secure" });
			var resp = await "http://insecure.com".WithCookies(out var jar).GetAsync();

			Assert.IsEmpty(jar);
			// even though the CookieJar rejected the cookie, it doesn't change the fact
			// that it exists in the response.
			Assert.AreEqual("foo", resp.Cookies.FirstOrDefault(c => c.Name == "x")?.Value);
		}

		[Test]
		public async Task multiple_cookies_same_name_picks_longest_path() {
			HttpTest.RespondWith("hi", 200, new[] {
				("Set-Cookie", "x=foo1; Path=/"),
				("Set-Cookie", "x=foo2; Path=/"), // overwrites
				("Set-Cookie", "x=foo3; Path=/a/b"), // doesn't overwrite, longer path should be listed first
				("Set-Cookie", "y=bar")
			});

			var resp = await "https://cookies.com".WithCookies(out var jar).GetAsync();
			Assert.AreEqual(4, resp.Headers.Count(h => h.Name == "Set-Cookie"));
			Assert.AreEqual(4, resp.Cookies.Count);

			var req = "https://cookies.com/a/b".WithCookies(jar);
			Assert.AreEqual("x=foo3; x=foo2; y=bar", req.Headers.FirstOrDefault("Cookie"));
		}

		[Test]
		public async Task expired_deletes_from_jar() {
			// because the standard https://stackoverflow.com/a/53573622/62600
			HttpTest
				.RespondWith("", headers: new[] {
					("Set-Cookie", "x=foo"),
					("Set-Cookie", "y=bar"),
					("Set-Cookie", "z=bazz")
				})
				.RespondWith("", headers: new[] { ("Set-Cookie", $"x=foo; Expires={DateTime.UtcNow.AddSeconds(-1):R}") })
				.RespondWith("", headers: new[] { ("Set-Cookie", "y=bar; Max-Age=0") })
				// not relevant to the request so shouldn't be deleted
				.RespondWith("", headers: new[] { ("Set-Cookie", "z=bazz; Path=/a; Max-Age=0") });

			await "https://cookies.com".WithCookies(out var jar).GetAsync();
			Assert.AreEqual(3, jar.Count);

			await "https://cookies.com".WithCookies(jar).GetAsync();
			Assert.AreEqual(2, jar.Count);
			Assert.AreEqual("y", jar.Select(c => c.Name).OrderBy(n => n).First());

			await "https://cookies.com".WithCookies(jar).GetAsync();
			Assert.AreEqual(1, jar.Count);
			Assert.AreEqual("z", jar.Single().Name);

			await "https://cookies.com".WithCookies(jar).GetAsync();
			Assert.AreEqual(1, jar.Count);
			Assert.AreEqual("z", jar.Single().Name);
		}

		[Test]
		public void names_are_case_sensitive() {
			var req = new FlurlRequest().WithCookie("a", 1).WithCookie("A", 2).WithCookie("a", 3);
			Assert.AreEqual(2, req.Cookies.Count());
			CollectionAssert.AreEquivalent(new[] { "a", "A" }, req.Cookies.Select(c => c.Name));
			CollectionAssert.AreEquivalent(new[] { "3", "2" }, req.Cookies.Select(c => c.Value));

			var jar = new CookieJar()
				.AddOrReplace("a", 1, "https://cookies.com")
				.AddOrReplace("A", 2, "https://cookies.com")
				.AddOrReplace("a", 3, "https://cookies.com");
			Assert.AreEqual(2, jar.Count);
			CollectionAssert.AreEquivalent(new[] { "a", "A" }, jar.Select(c => c.Name));
			CollectionAssert.AreEquivalent(new[] { "3", "2" }, jar.Select(c => c.Value));
		}

		/// <summary>
		/// Performs a series of behavioral checks against a cookie based on its state. Used by lots of tests to make them more robust.
		/// </summary>
		private void AssertCookie(FlurlCookie cookie, bool isValid, bool isExpired, string requestUrl = null, bool shouldSend = false) {
			Assert.AreEqual(isValid, cookie.IsValid(out var reason), reason);
			Assert.AreEqual(isExpired, cookie.IsExpired(out reason), reason);

			var shouldAddToJar = isValid && !isExpired;
			var jar = new CookieJar();
			Assert.AreEqual(shouldAddToJar, jar.TryAddOrReplace(cookie, out reason));

			if (shouldAddToJar)
				Assert.AreEqual(cookie.Name, jar.SingleOrDefault()?.Name);
			else {
				Assert.Throws<InvalidCookieException>(() => jar.AddOrReplace(cookie));
				CollectionAssert.IsEmpty(jar);
			}

			var req = cookie.OriginUrl.WithCookies(jar);
			Assert.AreEqual(shouldAddToJar, req.Cookies.Contains((cookie.Name, cookie.Value)));

			if (requestUrl != null) {
				Assert.AreEqual(shouldSend, cookie.ShouldSendTo(requestUrl, out reason), reason);
				req = requestUrl.WithCookies(jar);
				Assert.AreEqual(shouldSend, req.Cookies.Contains((cookie.Name, cookie.Value)));
			}
		}
	}
}
