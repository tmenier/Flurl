using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class ClientConfigTests
	{
		[Test]
		public void can_set_timeout() {
			var client = "http://www.api.com".WithTimeout(TimeSpan.FromSeconds(15));
			Assert.AreEqual(client.HttpClient.Timeout, TimeSpan.FromSeconds(15));
		}

		[Test]
		public void can_set_timeout_in_seconds() {
			var client = "http://www.api.com".WithTimeout(15);
			Assert.AreEqual(client.HttpClient.Timeout, TimeSpan.FromSeconds(15));
		}

		[Test]
		public void can_set_header() {
			var client = "http://www.api.com".WithHeader("a", 1);

			var headers = client.HttpClient.DefaultRequestHeaders.ToList();
			Assert.AreEqual(1, headers.Count);
			Assert.AreEqual("a", headers[0].Key);
			CollectionAssert.AreEqual(headers[0].Value, new[] { "1" });
		}

		[Test]
		public void can_set_headers_from_anon_object() {
			var client = "http://www.api.com".WithHeaders(new { a = "b", one = 2 });

			var headers = client.HttpClient.DefaultRequestHeaders.ToList();
			Assert.AreEqual(2, headers.Count);
			Assert.AreEqual("a", headers[0].Key);
			CollectionAssert.AreEqual(headers[0].Value, new[] { "b" });
			Assert.AreEqual("one", headers[1].Key);
			CollectionAssert.AreEqual(headers[1].Value, new[] { "2" });
		}

		[Test]
		public void can_set_headers_from_dictionary() {
			var client = "http://www.api.com".WithHeaders(new Dictionary<string, object> { { "a", "b" }, { "one", 2 } });

			var headers = client.HttpClient.DefaultRequestHeaders.ToList();
			Assert.AreEqual(2, headers.Count);
			Assert.AreEqual("a", headers[0].Key);
			CollectionAssert.AreEqual(headers[0].Value, new[] { "b" });
			Assert.AreEqual("one", headers[1].Key);
			CollectionAssert.AreEqual(headers[1].Value, new[] { "2" });
		}

		[Test]
		public void can_setup_basic_auth() {
			var client = "http://www.api.com".WithBasicAuth("user", "pass");

			var header = client.HttpClient.DefaultRequestHeaders.First();
			Assert.AreEqual("Authorization", header.Key);
			Assert.AreEqual("Basic dXNlcjpwYXNz", header.Value.First());
		}

		[Test]
		public void can_setup_oauth_bearer_token() {
			var client = "http://www.api.com".WithOAuthBearerToken("mytoken");

			var header = client.HttpClient.DefaultRequestHeaders.First();
			Assert.AreEqual("Authorization", header.Key);
			Assert.AreEqual("Bearer mytoken", header.Value.First());
		}

		[Test]
		public async Task can_allow_non_success_status() {
			using (var test = new HttpTest()) {
				test.RespondWith(418, "I'm a teapot");
				try {
					var result = await "http://www.api.com".AllowHttpStatus("1xx,300-500").GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (Exception) {
					Assert.Fail("Exception should not have been thrown.");
				}
			}
		}

		[Test]
		public async Task can_allow_any_http_status() {
			using (var test = new HttpTest()) {
				test.RespondWith(500, "epic fail");
				try {
					var result = await "http://www.api.com".AllowAnyHttpStatus().GetAsync();
					Assert.IsFalse(result.IsSuccessStatusCode);
				}
				catch (Exception) {
					Assert.Fail("Exception should not have been thrown.");
				}
			}
		}

		[Test, ExpectedException(typeof(FlurlHttpException))]
		public async Task can_clear_non_success_status() {
			using (var test = new HttpTest()) {
				test.RespondWith(418, "I'm a teapot");
				// allow 4xx
				var client = "http://www.api.com".AllowHttpStatus("4xx");
				// but then disallow it
				client.AllowedHttpStatusRanges.Clear();
				await client.GetAsync();
			}
		}
	}
}
