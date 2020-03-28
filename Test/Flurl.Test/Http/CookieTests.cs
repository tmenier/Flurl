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

			await "https://cookies.com".WithCookies(out var cookies).GetAsync();
			await "https://cookies.com/1".WithCookies(cookies).GetAsync();
			await "https://cookies.com/2".WithCookies(cookies).GetAsync();
			await "https://cookies.com/3".WithCookies(cookies).GetAsync();

			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(2);
			HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

			Assert.AreEqual(2, cookies.Count);
			Assert.AreEqual("foo", cookies["x"].Value);
			Assert.AreEqual("bazz", cookies["y"].Value);
		}

		[Test]
		public async Task can_send_cookies_per_request_initialized() {
			HttpTest
				.RespondWith("hi")
				.RespondWith("hi")
				.RespondWith("hi", cookies: new { y = "bazz" })
				.RespondWith("hi");

			await "https://cookies.com".WithCookies(new { x = "foo", y = "bar" }, out var cookies).GetAsync();
			await "https://cookies.com/1".WithCookies(cookies).GetAsync();
			await "https://cookies.com/2".WithCookies(cookies).GetAsync();
			await "https://cookies.com/3".WithCookies(cookies).GetAsync();

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

			var client = new FlurlClient("https://cookies.com");
			using (var cs = client.StartCookieSession()) {
				await cs.Request().GetAsync();
				await cs.Request("1").GetAsync();
				await cs.Request("2").GetAsync();
				await cs.Request("3").GetAsync();

				HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bar" }).Times(2);
				HttpTest.ShouldHaveMadeACall().WithCookies(new { x = "foo", y = "bazz" }).Times(1);

				Assert.AreEqual(2, cs.Cookies.Count);
				Assert.AreEqual("foo", cs.Cookies["x"].Value);
				Assert.AreEqual("bazz", cs.Cookies["y"].Value);
			}
		}
	}
}
