using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class RedirectTests : HttpTestFixtureBase
	{
		[Test]
		public async Task can_auto_redirect() {
			HttpTest
				.RespondWith("", 302, new { Location = "http://redir.com/foo" })
				.RespondWith("", 302, new { Location = "/redir2" })
				.RespondWith("", 302, new { Location = "redir3" })
				.RespondWith("done!");

			var resp = await "http://start.com".PostStringAsync("foo!").ReceiveString();

			Assert.AreEqual("done!", resp);
			HttpTest.ShouldHaveMadeACall().Times(4);
			HttpTest.ShouldHaveCalled("http://start.com").WithVerb(HttpMethod.Post).WithRequestBody("foo!")
				.With(call => call.RedirectedFrom == null);
			HttpTest.ShouldHaveCalled("http://redir.com/foo").WithVerb(HttpMethod.Get).WithRequestBody("")
				.With(call => call.RedirectedFrom.Request.Url.ToString() == "http://start.com");
			HttpTest.ShouldHaveCalled("http://redir.com/redir2").WithVerb(HttpMethod.Get).WithRequestBody("")
				.With(call => call.RedirectedFrom.Request.Url.ToString() == "http://redir.com/foo");
			HttpTest.ShouldHaveCalled("http://redir.com/redir2/redir3").WithVerb(HttpMethod.Get).WithRequestBody("")
				.With(call => call.RedirectedFrom.Request.Url.ToString() == "http://redir.com/redir2");
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task can_enable_auto_redirect_per_request(bool enabled) {
			HttpTest
				.RespondWith("original", 302, new { Location = "http://redir.com/foo" })
				.RespondWith("redirected");

			// whatever we want at the request level, set it the opposite at the client level
			var fc = new FlurlClient().WithAutoRedirect(!enabled);

			var result = await fc.Request("http://start.com").WithAutoRedirect(enabled).GetStringAsync();

			Assert.AreEqual(enabled ? "redirected" : "original", result);
			HttpTest.ShouldHaveMadeACall().Times(enabled ? 2 : 1);
		}

		[Test]
		public async Task redirect_preserves_most_headers() {
			HttpTest
				.RespondWith("", 302, new { Location = "/next" })
				.RespondWith("done!");

			await "http://start.com"
				.WithHeaders(new {
					Authorization = "xyz",
					Transfer_Encoding = "chunked",
					Custom_Header = "foo"
				})
				.PostAsync(null);

			HttpTest.ShouldHaveCalled("http://start.com")
				.WithHeader("Custom-Header")
				.WithHeader("Authorization")
				.WithHeader("Transfer-Encoding");

			HttpTest.ShouldHaveCalled("http://start.com/next")
				.WithHeader("Custom-Header", "foo")
				// except these 2:
				.WithoutHeader("Authorization")
				.WithoutHeader("Transfer-Encoding");
		}

		[TestCase(301, true)]
		[TestCase(302, true)]
		[TestCase(303, true)]
		[TestCase(307, false)]
		[TestCase(308, false)]
		public async Task redirect_preserves_verb_sometimes(int status, bool changeToGet) {
			HttpTest
				.RespondWith("", status, new { Location = "/next" })
				.RespondWith("done!");

			await "http://start.com".PostStringAsync("foo!");

			HttpTest.ShouldHaveCalled("http://start.com/next")
				.WithVerb(changeToGet ? HttpMethod.Get : HttpMethod.Post)
				.WithRequestBody(changeToGet ? "" : "foo!");
		}

		[Test]
		public void can_detect_circular_redirects() {
			HttpTest
				.RespondWith("", 301, new { Location = "/redir1" })
				.RespondWith("", 301, new { Location = "/redir2" })
				.RespondWith("", 301, new { Location = "/redir1" });

			var ex = Assert.ThrowsAsync<FlurlHttpException>(() => "http://start.com".GetAsync());
			StringAssert.Contains("Circular redirect", ex.Message);
		}

		[TestCase(null)] // test the default (10)
		[TestCase(5)]
		public async Task can_limit_redirects(int? max) {
			for (var i = 1; i <= 20; i++)
				HttpTest.RespondWith("", 301, new { Location = "/redir" + i });

			var fc = new FlurlClient();
			if (max.HasValue)
				fc.Settings.Redirects.MaxAutoRedirects = max.Value;

			await fc.Request("http://start.com").GetAsync();

			var count = max ?? 10;
			HttpTest.ShouldHaveCalled("http://start.com/redir*").Times(count);
			HttpTest.ShouldHaveCalled("http://start.com/redir" + count);
			HttpTest.ShouldNotHaveCalled("http://start.com/redir" + (count + 1));
		}

		[Test]
		public async Task can_change_redirect_behavior_from_event() {
			var eventFired = false;

			HttpTest
				.RespondWith("", 301, new { Location = "/next" })
				.RespondWith("done!");

			var fc = new FlurlClient()
				.OnRedirect(call => {
					eventFired = true;

					// assert all the properties of call.Redirect
					Assert.IsTrue(call.Redirect.Follow);
					Assert.AreEqual("http://start.com/next", call.Redirect.Url.ToString());
					Assert.AreEqual(1, call.Redirect.Count);
					Assert.IsTrue(call.Redirect.ChangeVerbToGet);

					// now change the behavior
					call.Redirect.Url.SetQueryParam("x", 999);
					call.Redirect.ChangeVerbToGet = false;
				});

			await fc.Request("http://start.com").PostStringAsync("foo!");

			Assert.IsTrue(eventFired);

			HttpTest.ShouldHaveCalled("http://start.com/next?x=999")
				.WithVerb(HttpMethod.Post)
				.WithRequestBody("foo!");
		}

		[Test]
		public async Task can_block_redirect_from_event() {
			HttpTest
				.RespondWith("", 301, new { Location = "/next" })
				.RespondWith("done!");

			var fc = new FlurlClient();
			await fc.Request("http://start.com")
				.OnRedirect(call => call.Redirect.Follow = false)
				.GetAsync();

			HttpTest.ShouldNotHaveCalled("http://start.com/next");
		}

		[Test]
		public async Task can_disable_redirect() {
			HttpTest
				.RespondWith("", 301, new { Location = "/next" })
				.RespondWith("done!");

			var fc = new FlurlClient();
			fc.Settings.Redirects.Enabled = false;
			await fc.Request("http://start.com").GetAsync();

			HttpTest.ShouldNotHaveCalled("http://start.com/next");
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task can_allow_redirect_secure_to_insecure(bool allow) {
			HttpTest
				.RespondWith("", 301, new { Location = "http://insecure.com/next" })
				.RespondWith("done!");

			var fc = new FlurlClient();
			if (allow) // test that false is default (don't set explicitly)
				fc.Settings.Redirects.AllowSecureToInsecure = true;

			await fc.Request("https://secure.com").GetAsync();

			if (allow)
				HttpTest.ShouldHaveCalled("http://insecure.com/next");
			else
				HttpTest.ShouldNotHaveCalled("http://insecure.com/next");
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task can_allow_forward_auth_header(bool allow) {
			HttpTest
				.RespondWith("", 301, new { Location = "/next" })
				.RespondWith("done!");

			var fc = new FlurlClient();
			if (allow) // test that false is default (don't set explicitly)
				fc.Settings.Redirects.ForwardAuthorizationHeader = true;

			await fc.Request("http://start.com")
				.WithHeader("Authorization", "foo")
				.GetAsync();

			if (allow)
				HttpTest.ShouldHaveCalled("http://start.com/next").WithHeader("Authorization", "foo");
			else
				HttpTest.ShouldHaveCalled("http://start.com/next").WithoutHeader("Authorization");
		}
	}
}
