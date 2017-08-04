using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
    [TestFixture]
    public class ClientConfigTestsBase //: ConfigTestsBase
    {
	    //private FlurlClient _client = new FlurlClient();

	    //protected override FlurlHttpSettings GetSettings(FlurlRequest req) {
		   // return _client.Settings;
	    //}

	    [Test]
        public void can_set_timeout()
        {
	        var fc = new FlurlClient().WithTimeout(TimeSpan.FromSeconds(15));
	        Assert.AreEqual(TimeSpan.FromSeconds(15), fc.Settings.Timeout);
        }

		[Test]
        public void can_set_timeout_in_seconds()
        {
            var client = new FlurlClient().WithTimeout(15);
            Assert.AreEqual(client.Settings.Timeout, TimeSpan.FromSeconds(15));
        }

        [Test]
        public void can_set_header()
        {
            var req = new FlurlClient().WithHeader("a", 1);
            Assert.AreEqual(1, req.Headers.Count);
            Assert.AreEqual(1, req.Headers["a"]);
        }

	    [Test]
        public void can_set_headers_from_anon_object()
        {
            var req = new FlurlClient().WithHeaders(new { a = "b", one = 2 });
	        Assert.AreEqual(2, req.Headers.Count);
	        Assert.AreEqual("b", req.Headers["a"]);
	        Assert.AreEqual(2, req.Headers["one"]);
        }

		[Test]
        public void can_set_headers_from_dictionary()
        {
            var req = new FlurlClient().WithHeaders(new Dictionary<string, object> { { "a", "b" }, { "one", 2 } });
            Assert.AreEqual(2, req.Headers.Count);
            Assert.AreEqual("b", req.Headers["a"]);
            Assert.AreEqual(2, req.Headers["one"]);
        }

	    [Test]
	    public void can_setup_oauth_bearer_token() {
		    var req = new FlurlClient().WithOAuthBearerToken("mytoken");
		    Assert.AreEqual(1, req.Headers.Count);
		    Assert.AreEqual("Bearer mytoken", req.Headers["Authorization"]);
	    }

		[Test]
	    public void can_setup_basic_auth() {
		    var req = new FlurlClient().WithBasicAuth("user", "pass");
			Assert.AreEqual(1, req.Headers.Count);
			Assert.AreEqual("Basic dXNlcjpwYXNz", req.Headers["Authorization"]);
	    }

		[Test]
        public async Task can_allow_specific_http_status()
        {
            using (var test = new HttpTest())
            {
                test.RespondWith("Nothing to see here", 404);
                // no exception = pass
                await "http://www.api.com"
                    .AllowHttpStatus(HttpStatusCode.Conflict, HttpStatusCode.NotFound)
                    .DeleteAsync();
            }
        }

        [Test]
        public void can_clear_non_success_status()
        {
            using (var test = new HttpTest())
            {
                test.RespondWith("I'm a teapot", 418);
                // allow 4xx
                var client = new FlurlClient().AllowHttpStatus("4xx");
                // but then disallow it
                client.Settings.AllowedHttpStatusRange = null;
                Assert.ThrowsAsync<FlurlHttpException>(async () => await client.WithUrl("http://api.com").GetAsync());
            }
        }

        [Test]
        public async Task can_allow_any_http_status()
        {
            using (var test = new HttpTest())
            {
                test.RespondWith("epic fail", 500);
                try
                {
                    var result = await "http://www.api.com".AllowAnyHttpStatus().GetAsync();
                    Assert.IsFalse(result.IsSuccessStatusCode);
                }
                catch (Exception)
                {
                    Assert.Fail("Exception should not have been thrown.");
                }
            }
        }

        [Test]
        public void can_override_settings_fluently()
        {
            using (var test = new HttpTest()) {
	            var client = new FlurlClient().WithSettings(s => s.AllowedHttpStatusRange = "*");
                test.RespondWith("epic fail", 500);
                Assert.ThrowsAsync<FlurlHttpException>(async () => await "http://www.api.com"
					.WithSettings(c => c.AllowedHttpStatusRange = "2xx")
					.WithClient(client) // client-level settings shouldn't win
					.GetAsync());
            }
        }

        [Test]
        public void WithUrl_shares_client_but_not_Url()
        {
            var client = new FlurlClient().WithCookie("mycookie", "123");
            var req1 = client.WithUrl("http://www.api.com/for-req1");
            var req2 = client.WithUrl("http://www.api.com/for-req2");
            var req3 = client.WithUrl("http://www.api.com/for-req3");

            CollectionAssert.AreEquivalent(req1.Cookies, req2.Cookies);
            CollectionAssert.AreEquivalent(req1.Cookies, req3.Cookies);
            var urls = new[] { req1, req2, req3 }.Select(c => c.Url.ToString());
            CollectionAssert.AllItemsAreUnique(urls);
        }

        [Test]
        public void WithClient_shares_client_but_not_Url()
        {
            var client = new FlurlClient().WithCookie("mycookie", "123");
            var req1 = "http://www.api.com/for-req1".WithClient(client);
            var req2 = "http://www.api.com/for-req2".WithClient(client);
            var req3 = "http://www.api.com/for-req3".WithClient(client);

	        CollectionAssert.AreEquivalent(req1.Cookies, req2.Cookies);
	        CollectionAssert.AreEquivalent(req1.Cookies, req3.Cookies);
	        var urls = new[] { req1, req2, req3 }.Select(c => c.Url.ToString());
	        CollectionAssert.AllItemsAreUnique(urls);
        }

        [Test]
        public void can_use_uri_with_WithUrl()
        {
            var uri = new System.Uri("http://www.mysite.com/foo?x=1");
            var req = new FlurlClient().WithUrl(uri);
            Assert.AreEqual(uri.ToString(), req.Url.ToString());
        }
    }
}