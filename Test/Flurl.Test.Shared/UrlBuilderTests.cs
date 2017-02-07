using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace Flurl.Test
{
	[TestFixture]
	public class UrlBuilderTests
	{
		[Test]
		// check that for every Url method, we have an equivalent string extension
		public void extension_methods_consistently_supported()
		{
			var urlMethods = typeof(Url).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName);
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(Url).GetTypeInfo().Assembly);
			var whitelist = new[] { "ToString", "IsValid" }; // cases where string extension of the same name was excluded intentionally

			foreach (var method in urlMethods)
			{
				if (whitelist.Contains(method.Name))
					continue;

				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m)))
				{
					Assert.Fail("No equivalent string extension method found for Url.{0}", method.Name);
				}
			}
		}

		[Test]
		public void Path_returns_everything_but_querystring()
		{
			var path = new Url("http://www.mysite.com/more?x=1&y=2").Path;
			Assert.AreEqual("http://www.mysite.com/more", path);
		}

		[Test]
		public void query_params_parse_correctly()
		{
			// y has 2 values, which should be grouped into an array
			var q = new Url("http://www.mysite.com/more?x=1&y=2&z=3&y=4").QueryParams;
			Assert.AreEqual("1", q["x"]);
			CollectionAssert.AreEqual(new[] { "2", "4" }, q["y"] as IEnumerable);
			Assert.AreEqual("3", q["z"]);
		}

		[Test]
		public void can_get_query_param_array()
		{
			var url = new Url("http://www.mysite.com/more?x=1&y=2&x=3&z=4&x=5");
			CollectionAssert.AreEqual(new[] { "1", "3", "5" }, url.QueryParams["x"] as object[]);
		}

		[Test]
		public void can_set_query_param_array()
		{
			var url = new Url("http://www.mysite.com/more?x=1&y=2&x=2&z=4");
			// go from 2 values to 3, order should be preserved
			url.QueryParams["x"] = new[] { 8, 9, 10 };
			Assert.AreEqual("http://www.mysite.com/more?x=8&y=2&x=9&z=4&x=10", url.ToString());
			// go from 3 values to 2, order should be preserved
			url.QueryParams["x"] = new[] { 101, 102 };
			Assert.AreEqual("http://www.mysite.com/more?x=101&y=2&x=102&z=4", url.ToString());
			// wipe them out. all of them.
			url.QueryParams["x"] = null;
			Assert.AreEqual("http://www.mysite.com/more?y=2&z=4", url.ToString());
		}

		[Test]
		public void can_sort_query_params()
		{
			var url = new Url("http://www.mysite.com/more?z=1&y=2&x=3");
			url.QueryParams.Sort((x, y) => x.Name.CompareTo(y.Name));
			Assert.AreEqual("http://www.mysite.com/more?x=3&y=2&z=1", url.ToString());
		}

		[Test]
		public void constructor_requires_nonnull_arg()
		{
			Assert.Throws<ArgumentNullException>(() => new Url(null));
		}

		[Test]
		public void Combine_works() {
			var url = Url.Combine("http://www.foo.com/", "/too/", "/many/", "/slashes/", "too", "few", "one/two/");
			Assert.AreEqual("http://www.foo.com/too/many/slashes/too/few/one/two/", url);
		}

		[TestCase("segment?", "foo=bar", "x=1&y=2&")]
		[TestCase("segment", "?foo=bar&x=1", "y=2&")]
		[TestCase("segment", "?", "foo=bar&x=1&y=2&")]
		[TestCase("/segment?foo=bar&", "&x=1&", "&y=2&")]
		[TestCase(null, "segment?foo=bar&x=1&y=2&", "")]
		public void Combine_supports_query(string a, string b, string c) {
			var url = Url.Combine("http://root.com", a, b, c);
			Assert.AreEqual("http://root.com/segment?foo=bar&x=1&y=2&", url);
		}

		[Test]
		public void Combine_encodes_illegal_chars() {
			var url = Url.Combine("http://www.foo.com", "hi there", "?", "x=hi there", "#", "hi there");
			Assert.AreEqual("http://www.foo.com/hi%20there?x=hi%20there#hi%20there", url);
		}

		[Test]
		public void GetRoot_works()
		{
			// simple case
			var root = Url.GetRoot("http://mysite.com/one/two/three");
			Assert.AreEqual("http://mysite.com", root);

			// a litte more fancy
			root = Url.GetRoot("https://me:you@www.site.com:8080/hello/goodbye?foo=bar");
			Assert.AreEqual("https://me:you@www.site.com:8080", root);
		}

		[Test]
		public void can_append_path_segment()
		{
			var url = "http://www.mysite.com".AppendPathSegment("endpoint");
			Assert.AreEqual("http://www.mysite.com/endpoint", url.ToString());
		}

		[Test]
		public void appending_null_path_segment_throws_arg_null_ex()
		{
			Assert.Throws<ArgumentNullException>(() => "http://www.mysite.com".AppendPathSegment(null));
		}

		[Test]
		public void can_append_multiple_path_segments_by_multi_args()
		{
			var url = "http://www.mysite.com".AppendPathSegments("category", "/endpoint/");
			Assert.AreEqual("http://www.mysite.com/category/endpoint/", url.ToString());
		}

		[Test]
		public void can_append_multiple_path_segments_by_enumerable()
		{
			IEnumerable<string> segments = new[] { "/category/", "endpoint" };
			var url = "http://www.mysite.com".AppendPathSegments(segments);
			Assert.AreEqual("http://www.mysite.com/category/endpoint", url.ToString());
		}

		[Test]
		public void can_add_query_param()
		{
			var url = "http://www.mysite.com".SetQueryParam("x", 1);
			Assert.AreEqual("http://www.mysite.com?x=1", url.ToString());
		}

		[Test]
		public void can_add_query_param_without_value()
		{
			var url = new Url("http://example.com?123456");
			Assert.AreEqual("http://example.com", url.Path);
			Assert.AreEqual(1, url.QueryParams.Count);
			Assert.AreEqual(string.Empty, url.QueryParams["123456"]);
		}

		[Test]
		public void can_change_query_param()
		{
			var url = "http://www.mysite.com?x=1".SetQueryParam("x", 2);
			Assert.AreEqual("http://www.mysite.com?x=2", url.ToString());
		}

		[Test]
		public void null_query_param_is_excluded()
		{
			var url = "http://www.mysite.com".SetQueryParam("x", null);
			Assert.AreEqual("http://www.mysite.com", url.ToString());
		}

		[Test]
		public void enumerable_query_param_is_split_into_multiple()
		{
			var url = "http://www.mysite.com".SetQueryParam("x", new[] { "a", "b", null, "c" });
			Assert.AreEqual("http://www.mysite.com?x=a&x=b&x=c", url.ToString());
		}

		[Test]
		public void can_add_multiple_query_params_from_anon_object()
		{
			var url = "http://www.mysite.com".SetQueryParams(new
			{
				x = 1,
				y = 2,
				z = new[] { 3, 4 },
				exclude_me = (string)null
			});
			Assert.AreEqual("http://www.mysite.com?x=1&y=2&z[0]=3&z[1]=4", url.ToString());
		}

		[Test]
		public void can_change_multiple_query_params_from_anon_object()
		{
			var url = "http://www.mysite.com?x=1&y[0]=2&z=3".SetQueryParams(new
			{
				x = 8,
				y = new[] { "a", "b" },
				z = (int?)null
			});
			Assert.AreEqual("http://www.mysite.com?x=8&y[0]=a&y[1]=b", url.ToString());
		}

		[Test]
		public void can_add_multiple_query_params_from_dictionary()
		{
			// let's challenge it a little with non-string keys
			var url = "http://www.mysite.com".SetQueryParams(new Dictionary<int, string> { { 1, "x" }, { 2, "y" } });
			Assert.AreEqual("http://www.mysite.com?1=x&2=y", url.ToString());
		}

		[Test]
		public void can_remove_query_param()
		{
			var url = "http://www.mysite.com/more?x=1&y=2".RemoveQueryParam("x");
			Assert.AreEqual("http://www.mysite.com/more?y=2", url.ToString());
		}

		[Test]
		public void can_remove_query_params_by_multi_args()
		{
			var url = "http://www.mysite.com/more?x=1&y=2".RemoveQueryParams("x", "y");
			Assert.AreEqual("http://www.mysite.com/more", url.ToString());
		}

		[Test]
		public void can_remove_query_params_by_enumerable()
		{
			var url = "http://www.mysite.com/more?x=1&y=2&z=3".RemoveQueryParams(new[] { "x", "z" });
			Assert.AreEqual("http://www.mysite.com/more?y=2", url.ToString());
		}

		[Test]
		public void removing_nonexisting_query_params_is_ignored()
		{
			var url = "http://www.mysite.com/more".RemoveQueryParams("x", "y");
			Assert.AreEqual("http://www.mysite.com/more", url.ToString());
		}

#if !NETCOREAPP1_0
		[Test]
		public void url_ToString_uses_invariant_culture()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
			var url = "http://www.mysite.com".SetQueryParam("x", 1.1);
			Assert.AreEqual("http://www.mysite.com?x=1.1", url.ToString());
		}
#endif

		[Test]
		public void can_reset_to_root()
		{
			var url = "http://www.mysite.com/more?x=1&y=2#foo".ResetToRoot();
			Assert.AreEqual("http://www.mysite.com", url.ToString());
		}

		[Test]
		public void can_do_crazy_long_fluent_expression()
		{
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
		public void encodes_invalid_path_chars()
		{
			var url = "http://www.mysite.com".AppendPathSegment("hey there how are ya");
			Assert.AreEqual("http://www.mysite.com/hey%20there%20how%20are%20ya", url.ToString());
		}

		[Test]
		public void does_not_reencode_path_escape_chars()
		{
			var url = "http://www.mysite.com".AppendPathSegment("hey%20there%20how%20are%20ya");
			Assert.AreEqual("http://www.mysite.com/hey%20there%20how%20are%20ya", url.ToString());
		}

		[Test]
		public void encodes_query_params()
		{
			var url = "http://www.mysite.com".SetQueryParams(new { x = "$50", y = "2+2=4" });
			Assert.AreEqual("http://www.mysite.com?x=%2450&y=2%2B2%3D4", url.ToString());
		}

		[Test]
		public void does_not_reencode_encoded_query_values()
		{
			var url = "http://www.mysite.com".SetQueryParam("x", "%CD%EE%E2%FB%E9%20%E3%EE%E4", true);
			Assert.AreEqual("http://www.mysite.com?x=%CD%EE%E2%FB%E9%20%E3%EE%E4", url.ToString());
		}

		[Test]
		public void reencodes_encoded_query_values_when_isEncoded_false()
		{
			var url = "http://www.mysite.com".SetQueryParam("x", "%CD%EE%E2%FB%E9%20%E3%EE%E4", false);
			Assert.AreEqual("http://www.mysite.com?x=%25CD%25EE%25E2%25FB%25E9%2520%25E3%25EE%25E4", url.ToString());
		}

		[Test]
		public void Url_implicitly_converts_to_string()
		{
			var url = new Url("http://www.mysite.com/more?x=1&y=2");
			var someMethodThatTakesAString = new Action<string>(s => { });
			someMethodThatTakesAString(url); // if this compiles, test passed.
		}

		[Test]
		public void interprets_plus_as_space()
		{
			var url = new Url("http://www.mysite.com/foo+bar?x=1+2");
			Assert.AreEqual("1 2", url.QueryParams["x"]);
		}

		[Test]
		public void can_encode_space_as_plus()
		{
			var url = new Url("http://www.mysite.com/foo+bar?x=1+2");
			Assert.AreEqual("http://www.mysite.com/foo+bar?x=1+2", url.ToString(true));
		}

		[Test]
		public void encodes_plus()
		{
			var url = new Url("http://www.mysite.com").SetQueryParam("x", "1+2");
			Assert.AreEqual("http://www.mysite.com?x=1%2B2", url.ToString());
		}

		[TestCase("http://www.mysite.com/more", true)]
		[TestCase("http://www.mysite.com/more?x=1&y=2", true)]
		[TestCase("http://www.mysite.com/more?x=1&y=2#frag", true)]
		[TestCase("http://www.mysite.com#frag", true)]
		[TestCase("", false)]
		[TestCase("blah", false)]
		[TestCase("http:/www.mysite.com", false)]
		[TestCase("www.mysite.com", false)]
		public void IsUrl_works(string s, bool isValid)
		{
			Assert.AreEqual(isValid, Url.IsValid(s));
			Assert.AreEqual(isValid, new Url(s).IsValid());
		}

		// #56
		[Test]
		public void does_not_alter_url_passed_to_constructor()
		{
			var expected = "http://www.mysite.com/hi%20there/more?x=%CD%EE%E2%FB%E9%20%E3%EE%E4";
			var url = new Url(expected);
			Assert.AreEqual(expected, url.ToString());
		}

		// #29
		[Test]
		public void can_add_and_remove_fragment_fluently()
		{
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
		public void has_fragment_after_SetQueryParam()
		{
			var expected = "http://www.mysite.com/more?x=1#first";
			var url = new Url(expected)
				.SetQueryParam("x", 3)
				.SetQueryParam("y", 4);
			Assert.AreEqual("http://www.mysite.com/more?x=3&y=4#first", url.ToString());
		}

		[TestCase("http://www.mysite.com/with/path?x=1", "http://www.mysite.com/with/path", "x=1", "")]
		[TestCase("http://www.mysite.com/with/path?x=1#foo", "http://www.mysite.com/with/path", "x=1", "foo")]
		[TestCase("http://www.mysite.com/with/path?x=1?y=2", "http://www.mysite.com/with/path", "x=1?y=2", "")]
		[TestCase("http://www.mysite.com/#with/path?x=1?y=2", "http://www.mysite.com/", "", "with/path?x=1?y=2")]
		public void constructor_parses_url_correctly(string full, string path, string query, string fragment)
		{
			var url = new Url(full);
			Assert.AreEqual(path, url.Path);
			Assert.AreEqual(query, url.Query);
			Assert.AreEqual(fragment, url.Fragment);
			Assert.AreEqual(full, url.ToString());
		}

		//public void QueryParams
	}
}
