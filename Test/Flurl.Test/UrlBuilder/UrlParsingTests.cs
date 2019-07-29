using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Flurl.Test.UrlBuilder
{
	[TestFixture, Parallelizable]
	public class UrlParsingTests
	{
		// relative
		[TestCase("//relative/with/authority", "", "relative", "", "relative", null, "/with/authority", "", "")]
		[TestCase("/relative/without/authority", "", "", "", "", null, "/relative/without/authority", "", "")]
		[TestCase("relative/without/path/anchor", "", "", "", "", null, "relative/without/path/anchor", "", "")]
		// absolute
		[TestCase("http://www.mysite.com/with/path?x=1", "http", "www.mysite.com", "", "www.mysite.com", null, "/with/path", "x=1", "")]
		[TestCase("https://www.mysite.com/with/path?x=1#foo", "https", "www.mysite.com", "", "www.mysite.com", null, "/with/path", "x=1", "foo")]
		[TestCase("http://user:pass@www.mysite.com:8080/with/path?x=1?y=2", "http", "user:pass@www.mysite.com:8080", "user:pass", "www.mysite.com", 8080, "/with/path", "x=1?y=2", "")]
		[TestCase("http://www.mysite.com/#with/path?x=1?y=2", "http", "www.mysite.com", "", "www.mysite.com", null, "/", "", "with/path?x=1?y=2")]
		// from https://en.wikipedia.org/wiki/Uniform_Resource_Identifier#Examples
		[TestCase("https://john.doe@www.example.com:123/forum/questions/?tag=networking&order=newest#top", "https", "john.doe@www.example.com:123", "john.doe", "www.example.com", 123, "/forum/questions/", "tag=networking&order=newest", "top")]
		[TestCase("ldap://[2001:db8::7]/c=GB?objectClass?one", "ldap", "[2001:db8::7]", "", "[2001:db8::7]", null, "/c=GB", "objectClass?one", "")]
		[TestCase("mailto:John.Doe@example.com", "mailto", "", "", "", null, "John.Doe@example.com", "", "")]
		[TestCase("news:comp.infosystems.www.servers.unix", "news", "", "", "", null, "comp.infosystems.www.servers.unix", "", "")]
		[TestCase("tel:+1-816-555-1212", "tel", "", "", "", null, "+1-816-555-1212", "", "")]
		[TestCase("telnet://192.0.2.16:80/", "telnet", "192.0.2.16:80", "", "192.0.2.16", 80, "/", "", "")]
		[TestCase("urn:oasis:names:specification:docbook:dtd:xml:4.1.2", "urn", "", "", "", null, "oasis:names:specification:docbook:dtd:xml:4.1.2", "", "")]
		public void can_parse_url_parts(string url, string scheme, string authority, string userInfo, string host, int? port, string path, string query, string fragment) {
			// there's a few ways to get Url object so let's check all of them
			foreach (var parsed in new[] { new Url(url), Url.Parse(url), new Url(new Uri(url, UriKind.RelativeOrAbsolute)) }) {
				Assert.AreEqual(scheme, parsed.Scheme);
				Assert.AreEqual(authority, parsed.Authority);
				Assert.AreEqual(userInfo, parsed.UserInfo);
				Assert.AreEqual(host, parsed.Host);
				Assert.AreEqual(port, parsed.Port);
				Assert.AreEqual(path, parsed.Path);
				Assert.AreEqual(query, parsed.Query);
				Assert.AreEqual(fragment, parsed.Fragment);
			}
		}

		[TestCase("http://www.trailing-slash.com/", "/")]
		[TestCase("http://www.trailing-slash.com/a/b/", "/a/b/")]
		[TestCase("http://www.trailing-slash.com/a/b/?x=y", "/a/b/")]
		[TestCase("http://www.no-trailing-slash.com", "")]
		[TestCase("http://www.no-trailing-slash.com/a/b", "/a/b")]
		[TestCase("http://www.no-trailing-slash.com/a/b?x=y", "/a/b")]
		public void retains_trailing_slash_on_path(string url, string path) {
			Assert.AreEqual(path, new Url(url).Path);
		}

		[Test]
		public void can_parse_query_params() {
			var q = new Url("http://www.mysite.com/more?x=1&y=2&z=3&y=4&abc&xyz&foo=&=bar&y=6").QueryParams;

			Assert.AreEqual(9, q.Count);
			Assert.AreEqual(new[] { "x", "y", "z", "y", "abc", "xyz", "foo", "", "y", }, q.Select(p => p.Name));
			Assert.AreEqual(new[] { "1", "2", "3", "4", null, null, "", "bar", "6", }, q.Select(p => p.Value));

			Assert.AreEqual("1", q["x"]);
			Assert.AreEqual(new[] { "2", "4", "6" }, q["y"]); // group values of same name into array
			Assert.AreEqual("3", q["z"]);
			Assert.AreEqual(null, q["abc"]);
			Assert.AreEqual(null, q["xyz"]);
			Assert.AreEqual("", q["foo"]);
			Assert.AreEqual("bar", q[""]);
		}

		[TestCase("http://www.mysite.com/more?x=1&y=2", true)]
		[TestCase("//how/about/this#hi", false)]
		[TestCase("/how/about/this#hi", false)]
		[TestCase("how/about/this#hi", false)]
		[TestCase("", false)]
		public void Url_converts_to_uri(string s, bool isAbsolute) {
			var url = new Url(s);
			var uri = url.ToUri();
			Assert.AreEqual(s, uri.OriginalString);
			Assert.AreEqual(isAbsolute, uri.IsAbsoluteUri);
		}

		[Test]
		public void interprets_plus_as_space() {
			var url = new Url("http://www.mysite.com/foo+bar?x=1+2");
			Assert.AreEqual("1 2", url.QueryParams["x"]);
		}

		[Test] // #437
		public void interprets_encoded_plus_as_plus() {
			var urlStr = "http://google.com/search?q=param_with_%2B";
			var url = new Url(urlStr);
			var paramValue = url.QueryParams["q"];
			Assert.AreEqual("param_with_+", paramValue);
		}

		[Test] // #56
		public void does_not_alter_url_passed_to_constructor() {
			var expected = "http://www.mysite.com/hi%20there/more?x=%CD%EE%E2%FB%E9%20%E3%EE%E4";
			var url = new Url(expected);
			Assert.AreEqual(expected, url.ToString());
		}
	}
}
