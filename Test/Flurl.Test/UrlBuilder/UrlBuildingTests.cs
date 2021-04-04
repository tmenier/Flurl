using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace Flurl.Test.UrlBuilder
{
	[TestFixture, Parallelizable]
	public class UrlBuildingTests
	{
		[Test]
		// check that for every Url method, we have an equivalent string extension
		public void extension_methods_consistently_supported() {
			var urlMethods = typeof(Url).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName);
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(Url).GetTypeInfo().Assembly);
			var uriExts = ReflectionHelper.GetAllExtensionMethods<Uri>(typeof(Url).GetTypeInfo().Assembly);
			var whitelist = new[] { "ToString", "IsValid", "ToUri", "Equals", "GetHashCode", "Clone", "Reset" }; // cases where string extension of the same name was excluded intentionally

			foreach (var method in urlMethods) {
				if (whitelist.Contains(method.Name)) continue;

				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent string extension method found for Url.{method.Name}({args})");
				}
				if (!uriExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent System.Uri extension method found for Url.{method.Name}({args})");
				}
			}
		}

		[Test]
		public void can_construct_from_uri() {
			var s = "http://www.mysite.com/more?x=1&y=2#foo";
			var uri = new Uri(s);
			var url = new Url(uri);
			Assert.AreEqual(s, url.ToString());
		}

		[Test]
		public void uri_cannot_be_null() {
			Assert.Throws<ArgumentNullException>(() => new Url((Uri)null));
		}

		[Test]
		public void can_set_query_params() {
			var url = "http://www.mysite.com/more"
				.SetQueryParam("x", 1)
				.SetQueryParam("y", new[] { "2", "4", "6" })
				.SetQueryParam("z", 3)
				.SetQueryParam("abc")
				.SetQueryParam("xyz")
				.SetQueryParam("foo", "")
				.SetQueryParam("", "bar");

			Assert.AreEqual("http://www.mysite.com/more?x=1&y=2&y=4&y=6&z=3&abc&xyz&foo=&=bar", url.ToString());
		}

		[Test] // #301
		public void setting_query_param_array_creates_multiple() {
			var q = "http://www.mysite.com".SetQueryParam("x", new[] { 1, 2, 3 }).QueryParams;
			Assert.AreEqual(3, q.Count);
			Assert.AreEqual(new[] { 1, 2, 3 }, q.Select(p => p.Value));
		}

		[Test]
		public void can_change_query_param() {
			var url = "http://www.mysite.com?x=1".SetQueryParam("x", 2);
			Assert.AreEqual("http://www.mysite.com?x=2", url.ToString());
		}

		[Test]
		public void enumerable_query_param_is_split_into_multiple() {
			var url = "http://www.mysite.com".SetQueryParam("x", new[] { "a", "b", null, "c" });
			Assert.AreEqual("http://www.mysite.com?x=a&x=b&x=c", url.ToString());
		}

		[Test]
		public void can_set_multiple_query_params_from_anon_object() {
			var url = "http://www.mysite.com".SetQueryParams(new {
				x = 1,
				y = 2,
				z = new[] { 3, 4 },
				exclude_me = (string)null
			});
			Assert.AreEqual("http://www.mysite.com?x=1&y=2&z=3&z=4", url.ToString());
		}

		[Test]
		public void can_replace_query_params_from_anon_object() {
			var url = "http://www.mysite.com?x=1&y=2&z=3".SetQueryParams(new {
				x = 8,
				y = new[] { "a", "b" },
				z = (int?)null
			});
			Assert.AreEqual("http://www.mysite.com?x=8&y=a&y=b", url.ToString());
		}

		[Test]
		public void can_set_query_params_from_dictionary() {
			// let's challenge it a little with non-string keys
			var url = "http://www.mysite.com".SetQueryParams(new Dictionary<int, string> { { 1, "x" }, { 2, "y" } });
			Assert.AreEqual("http://www.mysite.com?1=x&2=y", url.ToString());
		}

		[Test, Ignore("tricky to do while maintaining param order. deferring until append param w/o overwriting is fully supported.")]
		public void can_set_query_params_from_kv_pairs() {
			var url = "http://foo.com".SetQueryParams(new[] {
				new { key = "x", value = 1 },
				new { key = "y", value = 2 },
				new { key = "x", value = 3 } // this should append, not overwrite (#370)
			});

			Assert.AreEqual("http://foo.com?x=1&y=2&x=3", url.ToString());
		}

		private IEnumerable<string> GetQueryParamNames() {
			yield return "abc";
			yield return "123";
			yield return null;
			yield return "456";
		}

		[Test]
		public void can_set_multiple_no_value_query_params_from_enumerable_string() {
			var url = "http://www.mysite.com".SetQueryParams(GetQueryParamNames());
			Assert.AreEqual("http://www.mysite.com?abc&123&456", url.ToString());
		}

		[Test]
		public void can_set_multiple_no_value_query_params_from_params() {
			var url = "http://www.mysite.com".SetQueryParams("abc", "123", null, "456");
			Assert.AreEqual("http://www.mysite.com?abc&123&456", url.ToString());
		}

		[Test]
		public void can_remove_query_param() {
			var url = "http://www.mysite.com/more?x=1&y=2".RemoveQueryParam("x");
			Assert.AreEqual("http://www.mysite.com/more?y=2", url.ToString());
		}

		[Test]
		public void can_remove_query_params_by_multi_args() {
			var url = "http://www.mysite.com/more?x=1&y=2".RemoveQueryParams("x", "y");
			Assert.AreEqual("http://www.mysite.com/more", url.ToString());
		}

		[Test]
		public void can_remove_query_params_by_enumerable() {
			var url = "http://www.mysite.com/more?x=1&y=2&z=3".RemoveQueryParams(new[] { "x", "z" });
			Assert.AreEqual("http://www.mysite.com/more?y=2", url.ToString());
		}

		[TestCase("http://www.mysite.com/?x=1&y=2&z=3#foo", "http://www.mysite.com/#foo")]
		[TestCase("http://www.mysite.com/more?x=1&y=2&z=3#foo", "http://www.mysite.com/more#foo")]
		[TestCase("relative/path?foo", "relative/path")]
		public void can_remove_query(string original, string expected) {
			var url = original.RemoveQuery();
			Assert.AreEqual(expected, url.ToString());
		}

		[Test]
		public void removing_nonexisting_query_params_is_ignored() {
			var url = "http://www.mysite.com/more".RemoveQueryParams("x", "y");
			Assert.AreEqual("http://www.mysite.com/more", url.ToString());
		}

		[TestCase(NullValueHandling.Ignore, ExpectedResult = "http://www.mysite.com?y=2&x=1&z=foo")]
		[TestCase(NullValueHandling.NameOnly, ExpectedResult = "http://www.mysite.com?y&x=1&z=foo")]
		[TestCase(NullValueHandling.Remove, ExpectedResult = "http://www.mysite.com?x=1&z=foo")]
		public string can_control_null_value_behavior_in_query_params(NullValueHandling nullValueHandling) {
			return "http://www.mysite.com?y=2".SetQueryParams(new { x = 1, y = (string)null, z = "foo" }, nullValueHandling).ToString();
		}

		[TestCase("http://www.mysite.com", "endpoint", "http://www.mysite.com/endpoint")]
		[TestCase("path1", "path2", "path1/path2")] // #568
		[TestCase("/path1/path2", "path3", "/path1/path2/path3")]
		public void can_append_path_segment(string original, string segment, string result) {
			Assert.AreEqual(result, original.AppendPathSegment(segment).ToString());
			Assert.AreEqual(result, original.AppendPathSegment("/" + segment).ToString());
			Assert.AreEqual(result, (original + "/").AppendPathSegment(segment).ToString());
			Assert.AreEqual(result, (original + "/").AppendPathSegment("/" + segment).ToString());
		}

		[TestCase("http://www.mysite.com/", "http://www.mysite.com/")]
		[TestCase("http://www.mysite.com", "http://www.mysite.com/")]
		public void can_add_trailing_slash(string original, string result) {
			Assert.AreEqual(result, original.AppendPathSegment("/").ToString());
		}

		[Test]
		public void appending_null_path_segment_throws_arg_null_ex() {
			Assert.Throws<ArgumentNullException>(() => "http://www.mysite.com".AppendPathSegment(null));
		}

		[Test]
		public void can_append_multiple_path_segments_by_multi_args() {
			var url = "http://www.mysite.com".AppendPathSegments("category", "/endpoint/");
			Assert.AreEqual("http://www.mysite.com/category/endpoint/", url.ToString());
		}

		[Test]
		public void can_append_multiple_path_segments_by_enumerable() {
			IEnumerable<string> segments = new[] { "/category/", "endpoint" };
			var url = "http://www.mysite.com".AppendPathSegments(segments);
			Assert.AreEqual("http://www.mysite.com/category/endpoint", url.ToString());
		}

		[TestCase("http://www.site.com/path1/path2/?x=y", "http://www.site.com/path1/?x=y")]
		[TestCase("http://www.site.com/path1/path2?x=y", "http://www.site.com/path1?x=y")]
		[TestCase("http://www.site.com/path1/", "http://www.site.com/")]
		[TestCase("http://www.site.com/path1", "http://www.site.com/")]
		[TestCase("http://www.site.com/", "http://www.site.com/")]
		[TestCase("http://www.site.com", "http://www.site.com")]
		[TestCase("/path1/path2", "/path1")]
		[TestCase("path1/path2", "path1")]
		public void can_remove_path_segment(string original, string expected) {
			var url = original.RemovePathSegment();
			Assert.AreEqual(expected, url.ToString());
		}

		[TestCase("http://www.site.com/path1/path2/?x=y", "http://www.site.com?x=y")]
		[TestCase("http://www.site.com/path1/path2?x=y", "http://www.site.com?x=y")]
		[TestCase("http://www.site.com/", "http://www.site.com")]
		[TestCase("http://www.site.com", "http://www.site.com")]
		[TestCase("/path1/path2", "")]
		[TestCase("path1/path2", "")]
		[TestCase("news:comp.infosystems.www.servers.unix", "news:")]
		public void can_remove_path(string original, string expected) {
			var url = original.RemovePath();
			Assert.AreEqual(expected, url.ToString());
		}

#if !NETCOREAPP1_1
		[Test]
		public void url_ToString_uses_invariant_culture() {
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
			var url = "http://www.mysite.com".SetQueryParam("x", 1.1);
			Assert.AreEqual("http://www.mysite.com?x=1.1", url.ToString());
		}
#endif

		[Test]
		public void can_reset_to_root() {
			var url = "http://www.mysite.com/more?x=1&y=2#foo".ResetToRoot();
			Assert.AreEqual("http://www.mysite.com", url.ToString());
		}

		[Test]
		public void can_reset() {
			var orignal = "http://www.mysite.com/more?x=1&y=2#foo";
			var url = orignal.AppendPathSegment("path2").RemoveQueryParam("y").SetFragment("bar");
			url.Scheme = "https";
			url.Port = 1234;
			url.Host = "www.yoursite.com";
			Assert.AreEqual("https://www.yoursite.com:1234/more/path2?x=1#bar", url.ToString());
			Assert.AreEqual(orignal, url.Reset().ToString());
		}

		[Test]
		public void can_parse_path_segments() {
			var path = "/segment 1//segment 3/?segment#4/";
			CollectionAssert.AreEqual(new[] { "segment%201", "", "segment%203", "%3Fsegment%234" }, Url.ParsePathSegments(path));
		}

		[Test]
		public void can_do_crazy_long_fluent_expression() {
			var url = "http://www.mysite.com"
				.SetQueryParams(new { a = 1, b = 2, c = 999 })
				.SetFragment("fooey")
				.AppendPathSegment("category")
				.RemoveQueryParam("c")
				.SetQueryParam("z", 55)
				.RemoveQueryParams("a", "z")
				.SetQueryParams(new { n = "hi", m = "bye" })
				.AppendPathSegment("endpoint");

			Assert.AreEqual("http://www.mysite.com/category/endpoint?b=2&n=hi&m=bye#fooey", url.ToString());
		}

		[Test]
		public void encodes_illegal_path_chars() {
			// should not encode '/'
			var url = "http://www.mysite.com".AppendPathSegment("hi there/bye now");
			Assert.AreEqual("http://www.mysite.com/hi%20there/bye%20now", url.ToString());
		}

		[Test]
		public void can_encode_reserved_path_chars() {
			// should encode '/' (tests optional fullyEncode arg)
			var url = "http://www.mysite.com".AppendPathSegment("hi there/bye now", true);
			Assert.AreEqual("http://www.mysite.com/hi%20there%2Fbye%20now", url.ToString());
		}

		[Test]
		public void does_not_reencode_path_escape_chars() {
			var url = "http://www.mysite.com".AppendPathSegment("hi%20there/inside%2foutside");
			Assert.AreEqual("http://www.mysite.com/hi%20there/inside%2foutside", url.ToString());
		}

		[Test]
		public void encodes_query_params() {
			var url = "http://www.mysite.com".SetQueryParams(new { x = "$50", y = "2+2=4" });
			Assert.AreEqual("http://www.mysite.com?x=%2450&y=2%2B2%3D4", url.ToString());
		}

		[Test] // #582
		public void encodes_date_type_query_param() {
			var date = new DateTime(2020, 12, 6, 10, 45, 1);
			var url = "http://www.mysite.com".SetQueryParam("date", date);
			Assert.AreEqual("http://www.mysite.com?date=2020-12-06T10%3A45%3A01.0000000", url.ToString());
		}

		[Test]
		public void does_not_reencode_encoded_query_values() {
			var url = "http://www.mysite.com".SetQueryParam("x", "%CD%EE%E2%FB%E9%20%E3%EE%E4", true);
			Assert.AreEqual("http://www.mysite.com?x=%CD%EE%E2%FB%E9%20%E3%EE%E4", url.ToString());
		}

		[Test]
		public void reencodes_encoded_query_values_when_isEncoded_false() {
			var url = "http://www.mysite.com".SetQueryParam("x", "%CD%EE%E2%FB%E9%20%E3%EE%E4", false);
			Assert.AreEqual("http://www.mysite.com?x=%25CD%25EE%25E2%25FB%25E9%2520%25E3%25EE%25E4", url.ToString());
		}

		[Test]
		public void does_not_encode_reserved_chars_in_query_param_name() {
			var url = "http://www.mysite.com".SetQueryParam("$x", 1);
			Assert.AreEqual("http://www.mysite.com?$x=1", url.ToString());
		}

		[Test]
		public void Url_implicitly_converts_to_string() {
			var url = new Url("http://www.mysite.com/more?x=1&y=2");
			var someMethodThatTakesAString = new Action<string>(s => { });
			someMethodThatTakesAString(url); // if this compiles, test passed.
		}

		[Test]
		public void encodes_plus() {
			var url = new Url("http://www.mysite.com").SetQueryParam("x", "1+2");
			Assert.AreEqual("http://www.mysite.com?x=1%2B2", url.ToString());
		}

		[Test]
		public void can_encode_space_as_plus() {
			var url = new Url("http://www.mysite.com").AppendPathSegment("a b").SetQueryParam("c d", "1 2");
			Assert.AreEqual("http://www.mysite.com/a%20b?c%20d=1%202", url.ToString()); // but not by default
			Assert.AreEqual("http://www.mysite.com/a+b?c+d=1+2", url.ToString(true));
		}

		[Test] // #29
		public void can_add_and_remove_fragment_fluently() {
			var url = "http://www.mysite.com".SetFragment("foo");
			Assert.AreEqual("http://www.mysite.com#foo", url.ToString());
			url = "http://www.mysite.com#foo".RemoveFragment();
			Assert.AreEqual("http://www.mysite.com", url.ToString());
			url = "http://www.mysite.com"
				.SetFragment("foo")
				.SetFragment("bar")
				.AppendPathSegment("more")
				.SetQueryParam("x", 1);
			Assert.AreEqual("http://www.mysite.com/more?x=1#bar", url.ToString());
			url = "http://www.mysite.com".SetFragment("foo").SetFragment("bar").RemoveFragment();
			Assert.AreEqual("http://www.mysite.com", url.ToString());
		}

		[Test]
		public void has_fragment_after_SetQueryParam() {
			var expected = "http://www.mysite.com/more?x=1#first";
			var url = new Url(expected)
				.SetQueryParam("x", 3)
				.SetQueryParam("y", 4);
			Assert.AreEqual("http://www.mysite.com/more?x=3&y=4#first", url.ToString());
		}

		[Test]
		public void Equals_returns_true_for_same_values() {
			var url1 = new Url("http://mysite.com/hello");
			var url2 = new Url("http://mysite.com").AppendPathSegment("hello");
			var url3 = new Url("http://mysite.com/hello/"); // trailing slash - not equal

			Assert.IsTrue(url1.Equals(url2));
			Assert.IsTrue(url2.Equals(url1));
			Assert.AreEqual(url1.GetHashCode(), url2.GetHashCode());

			Assert.IsFalse(url1.Equals(url3));
			Assert.IsFalse(url3.Equals(url1));
			Assert.AreNotEqual(url1.GetHashCode(), url3.GetHashCode());

			Assert.IsFalse(url1.Equals("http://mysite.com/hello"));
			Assert.IsFalse(url1.Equals(null));
		}

		[Test]
		public void equality_operator_always_false_for_different_instances() {
			var url1 = new Url("http://mysite.com/hello");
			var url2 = new Url("http://mysite.com/hello");
			Assert.IsFalse(url1 == url2);
		}

		[Test]
		public void clone_creates_copy() {
			var url1 = new Url("http://mysite.com").SetQueryParam("x", 1);
			var url2 = url1.Clone().AppendPathSegment("foo").SetQueryParam("y", 2);
			url1.SetQueryParam("z", 3);

			Assert.AreEqual("http://mysite.com?x=1&z=3", url1.ToString());
			Assert.AreEqual("http://mysite.com/foo?x=1&y=2", url2.ToString());
		}

		#region writable properties
		[Test]
		public void can_write_scheme() {
			var url = new Url("https://api.com/foo");
			url.Scheme = "ftp";
			Assert.AreEqual("ftp://api.com/foo", url.ToString());
		}

		[Test]
		public void can_write_user_info() {
			var url = new Url("https://api.com/foo");
			url.UserInfo = "user:pass";
			Assert.AreEqual("https://user:pass@api.com/foo", url.ToString());

			// null or empty should remove it
			url.UserInfo = null;
			Assert.AreEqual("https://api.com/foo", url.ToString());
			url.UserInfo = "";
			Assert.AreEqual("https://api.com/foo", url.ToString());
		}

		[Test]
		public void can_write_host() {
			var url = new Url("https://api.com/foo");
			url.Host = "www.othersite.net";
			Assert.AreEqual("https://www.othersite.net/foo", url.ToString());

			// don't squash userinfo/port
			url = new Url("https://user:pass@api.com:8080/foo");
			url.Host = "www.othersite.net";
			Assert.AreEqual("https://user:pass@www.othersite.net:8080/foo", url.ToString());
		}

		[Test]
		public void can_write_port() {
			var url = new Url("https://api.com/foo");
			url.Port = 1234;
			Assert.AreEqual("https://api.com:1234/foo", url.ToString());

			// null should remove it
			url.Port = null;
			Assert.AreEqual("https://api.com/foo", url.ToString());
		}

		[Test]
		public void can_write_path() {
			var url = new Url("https://api.com/foo");
			url.Path = "/a/b/c/";
			Assert.AreEqual("https://api.com/a/b/c/", url.ToString());
			url.Path = "a/b/c";
			Assert.AreEqual("https://api.com/a/b/c", url.ToString());
			url.Path = "/";
			Assert.AreEqual("https://api.com/", url.ToString());

			// null or empty should remove it (including leading slash)
			url.Path = null;
			Assert.AreEqual("https://api.com", url.ToString());
			url.Path = "";
			Assert.AreEqual("https://api.com", url.ToString());
		}

		[Test]
		public void can_manipulate_path_segments() {
			var url = new Url("https://api.com/foo?x=0");
			url.PathSegments.Add("bar");
			Assert.AreEqual("https://api.com/foo/bar?x=0", url.ToString());
			// when manipulating PathSegments directly, there's no double-slash check like with AppendPathSegment
			url.PathSegments.Add("/hi/there/");
			Assert.AreEqual("https://api.com/foo/bar//hi/there/?x=0", url.ToString());
			url.PathSegments.RemoveAt(1);
			Assert.AreEqual("https://api.com/foo//hi/there/?x=0", url.ToString());
			url.PathSegments.RemoveAt(1);
			Assert.AreEqual("https://api.com/foo?x=0", url.ToString());
			url.PathSegments.Clear();
			Assert.AreEqual("https://api.com/?x=0", url.ToString());
		}

		[Test]
		public void can_write_query() {
			var url = new Url("https://api.com/foo?x=0#bar");
			url.Query = "y=1&z=2";
			Assert.AreEqual("https://api.com/foo?y=1&z=2#bar", url.ToString());

			// null or empty should remove
			url.Query = null;
			Assert.AreEqual("https://api.com/foo#bar", url.ToString());
			url.Query = "";
			Assert.AreEqual("https://api.com/foo#bar", url.ToString());
		}

		[Test]
		public void can_manipulate_query_params() {
			var url = new Url("https://api.com/foo#bar");
			url.QueryParams.Add("x", 0);
			Assert.AreEqual("https://api.com/foo?x=0#bar", url.ToString());
			url.QueryParams.Add("y", 1);
			Assert.AreEqual("https://api.com/foo?x=0&y=1#bar", url.ToString());
			url.QueryParams.AddOrReplace("x", "hi!");
			Assert.AreEqual("https://api.com/foo?x=hi%21&y=1#bar", url.ToString());
			url.QueryParams.AddOrReplace("z", new[] { 8, 9, 10 });
			Assert.AreEqual("https://api.com/foo?x=hi%21&y=1&z=8&z=9&z=10#bar", url.ToString());
			url.QueryParams.Remove("y");
			Assert.AreEqual("https://api.com/foo?x=hi%21&z=8&z=9&z=10#bar", url.ToString());
			url.QueryParams.AddOrReplace("x", null);
			Assert.AreEqual("https://api.com/foo?z=8&z=9&z=10#bar", url.ToString());
			url.QueryParams.Clear();
			Assert.AreEqual("https://api.com/foo#bar", url.ToString());
		}

		[Test]
		public void can_modify_query_param_array() {
			var url = new Url("http://www.mysite.com/more?x=1&y=2&x=2&z=4");
			// go from 2 values to 3, order should be preserved
			url.QueryParams.AddOrReplace("x", new[] { 8, 9, 10 });
			Assert.AreEqual("http://www.mysite.com/more?x=8&y=2&x=9&z=4&x=10", url.ToString());
			// go from 3 values to 2, order should be preserved
			url.QueryParams.AddOrReplace("x", new[] { 101, 102 });
			Assert.AreEqual("http://www.mysite.com/more?x=101&y=2&x=102&z=4", url.ToString());
			// wipe them out. all of them.
			url.QueryParams.AddOrReplace("x", null);
			Assert.AreEqual("http://www.mysite.com/more?y=2&z=4", url.ToString());
		}

		[Test]
		public void can_write_fragment() {
			var url = new Url("https://api.com/");
			url.Fragment = "hello";
			Assert.AreEqual("https://api.com/#hello", url.ToString());
			url.Fragment = "#goodbye"; // # isn't technically part of the fragment, so this should give us 2 of them
			Assert.AreEqual("https://api.com/##goodbye", url.ToString());

			// null or empty should remove
			url.Fragment = null;
			Assert.AreEqual("https://api.com/", url.ToString());
			url.Fragment = "";
			Assert.AreEqual("https://api.com/", url.ToString());
		}
		#endregion

		[Test]
		public void can_build_url_from_uri() {
			// shouldn't need to test System.Uri extensions exhaustively. they're auto-generated, and we already
			// verified the set of them is uniform and complete as compared to the string extensions
			// (see extension_methods_consistently_supported). just check a couple.

			var url = new Uri("http://mysite.com").AppendPathSegment("hello");
			Assert.IsInstanceOf<Url>(url);
			Assert.AreEqual("http://mysite.com/hello", url.ToString());

			url = new Uri("http://mysite.com/hello").ResetToRoot();
			Assert.IsInstanceOf<Url>(url);
			Assert.AreEqual("http://mysite.com", url.ToString());
		}

		[Test] // https://github.com/tmenier/Flurl/issues/510
		public void uri_with_default_port_parses_correctly() {
			var originalString = "https://someurl.net:443/api/somepath";
			var uri = new Uri(originalString);
			var url = new Url(uri);
			Assert.AreEqual(443, url.Port);
			Assert.AreEqual(originalString, url.ToString());
		}

		[Test]
		public void can_have_empty_ctor() {
			var url1 = new Url();
			Assert.AreEqual("", url1.ToString());

			var url2 = new Url {
				Host = "192.168.1.1",
				Scheme = "http"
			};
			Assert.AreEqual("http://192.168.1.1", url2.ToString());
		}
	}
}
