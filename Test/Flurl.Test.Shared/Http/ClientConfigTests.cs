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
    public class ClientConfigTestsBase : ConfigTestsBase
    {
        protected override FlurlHttpSettings GetSettings()
        {
            return GetClient().Settings;
        }

        [Test]
        public void can_set_timeout()
        {
            var client = "http://www.api.com".WithTimeout(TimeSpan.FromSeconds(15));
            Assert.AreEqual(client.HttpClient.Timeout, TimeSpan.FromSeconds(15));
        }

        [Test]
        public void can_set_timeout_in_seconds()
        {
            var client = "http://www.api.com".WithTimeout(15);
            Assert.AreEqual(client.HttpClient.Timeout, TimeSpan.FromSeconds(15));
        }

        [Test]
        public void can_set_header()
        {
            var client = "http://www.api.com".WithHeader("a", 1);

            var headers = client.HttpClient.DefaultRequestHeaders.ToList();
            Assert.AreEqual(1, headers.Count);
            Assert.AreEqual("a", headers[0].Key);
            CollectionAssert.AreEqual(headers[0].Value, new[] { "1" });
        }

        [Test]
        public void can_set_headers_from_anon_object()
        {
            var client = "http://www.api.com".WithHeaders(new { a = "b", one = 2 });

            var headers = client.HttpClient.DefaultRequestHeaders.ToList();
            Assert.AreEqual(2, headers.Count);
            Assert.AreEqual("a", headers[0].Key);
            CollectionAssert.AreEqual(headers[0].Value, new[] { "b" });
            Assert.AreEqual("one", headers[1].Key);
            CollectionAssert.AreEqual(headers[1].Value, new[] { "2" });
        }

        [Test]
        public void can_set_headers_from_dictionary()
        {
            var client = "http://www.api.com".WithHeaders(new Dictionary<string, object> { { "a", "b" }, { "one", 2 } });

            var headers = client.HttpClient.DefaultRequestHeaders.ToList();
            Assert.AreEqual(2, headers.Count);
            Assert.AreEqual("a", headers[0].Key);
            CollectionAssert.AreEqual(headers[0].Value, new[] { "b" });
            Assert.AreEqual("one", headers[1].Key);
            CollectionAssert.AreEqual(headers[1].Value, new[] { "2" });
        }

        [Test]
        public void can_setup_basic_auth()
        {
            var client = "http://www.api.com".WithBasicAuth("user", "pass");

            var header = client.HttpClient.DefaultRequestHeaders.First();
            Assert.AreEqual("Authorization", header.Key);
            Assert.AreEqual("Basic dXNlcjpwYXNz", header.Value.First());
        }

        [Test]
        public void can_setup_oauth_bearer_token()
        {
            var client = "http://www.api.com".WithOAuthBearerToken("mytoken");

            var header = client.HttpClient.DefaultRequestHeaders.First();
            Assert.AreEqual("Authorization", header.Key);
            Assert.AreEqual("Bearer mytoken", header.Value.First());
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
                var client = "http://www.api.com".AllowHttpStatus("4xx");
                // but then disallow it
                client.Settings.AllowedHttpStatusRange = null;
                Assert.ThrowsAsync<FlurlHttpException>(async () => await client.GetAsync());
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
            using (var test = new HttpTest())
            {
                FlurlHttp.GlobalSettings.AllowedHttpStatusRange = "*";
                test.RespondWith("epic fail", 500);
                Assert.ThrowsAsync<FlurlHttpException>(async () => await "http://www.api.com".ConfigureClient(c => c.AllowedHttpStatusRange = "2xx").GetAsync());
            }
        }

        [Test]
        public void WithUrl_shares_HttpClient_but_not_Url()
        {
            var client1 = new FlurlClient("http://www.api.com/for-client1").WithCookie("mycookie", "123");
            var client2 = client1.WithUrl("http://www.api.com/for-client2");
            var client3 = client1.WithUrl("http://www.api.com/for-client3");
            var client4 = client2.WithUrl("http://www.api.com/for-client4");

            CollectionAssert.AreEquivalent(client1.Cookies, client2.Cookies);
            CollectionAssert.AreEquivalent(client1.Cookies, client3.Cookies);
            CollectionAssert.AreEquivalent(client1.Cookies, client4.Cookies);
            var urls = new[] { client1, client2, client3, client4 }.Select(c => c.Url.ToString());
            CollectionAssert.AllItemsAreUnique(urls);
        }

        [Test]
        public void WithUrl_doesnt_propagate_HttpClient_disposal()
        {
            var client1 = new FlurlClient("http://www.api.com/for-client1").WithCookie("mycookie", "123");
            var client2 = client1.WithUrl("http://www.api.com/for-client2");
            var client3 = client1.WithUrl("http://www.api.com/for-client3");
            var client4 = client2.WithUrl("http://www.api.com/for-client4");

            client2.Dispose();
            client3.Dispose();

			CollectionAssert.IsEmpty(client2.Cookies);
			CollectionAssert.IsEmpty(client3.Cookies);

			CollectionAssert.IsNotEmpty(client1.Cookies);
            CollectionAssert.IsNotEmpty(client4.Cookies);
        }

        [Test]
        public void WithClient_shares_HttpClient_but_not_Url()
        {
            var client1 = new FlurlClient("http://www.api.com/for-client1").WithCookie("mycookie", "123");
            var client2 = "http://www.api.com/for-client2".WithClient(client1);
            var client3 = "http://www.api.com/for-client3".WithClient(client1);
            var client4 = "http://www.api.com/for-client4".WithClient(client1);

            CollectionAssert.AreEquivalent(client1.Cookies, client2.Cookies);
            CollectionAssert.AreEquivalent(client1.Cookies, client3.Cookies);
            CollectionAssert.AreEquivalent(client1.Cookies, client4.Cookies);
            var urls = new[] { client1, client2, client3, client4 }.Select(c => c.Url.ToString());
            CollectionAssert.AllItemsAreUnique(urls);
        }

        [Test]
        public void WithClient_doesnt_propagate_HttpClient_disposal()
        {
            var client1 = new FlurlClient("http://www.api.com/for-client1").WithCookie("mycookie", "123");
            var client2 = "http://www.api.com/for-client2".WithClient(client1);
            var client3 = "http://www.api.com/for-client3".WithClient(client1);
            var client4 = "http://www.api.com/for-client4".WithClient(client1);

            client2.Dispose();
            client3.Dispose();

			CollectionAssert.IsEmpty(client2.Cookies);
			CollectionAssert.IsEmpty(client3.Cookies);

            CollectionAssert.IsNotEmpty(client1.Cookies);
            CollectionAssert.IsNotEmpty(client4.Cookies);
        }

        [Test]
        public void can_use_uri_with_WithUrl()
        {
            var uri = new System.Uri("http://www.mysite.com/foo?x=1");
            var fc = new FlurlClient().WithUrl(uri);
            Assert.AreEqual(uri.ToString(), fc.Url.ToString());
        }
    }
}