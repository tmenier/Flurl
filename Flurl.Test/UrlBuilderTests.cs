using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace Flurl.Test
{
	[TestFixture]
    public class UrlBuilderTests
    {
		[Test]
		public void Url_implicitly_converts_to_string() {
			var url = new Url("http://www.mysite.com/more?x=1&y=2");
			var someMethodThatTakesAString = new Action<string> (s => { });
			someMethodThatTakesAString(url); // if this compiles, test passed.
		}

		[Test]
		public void IsUrl_true_for_valid_url() {
			Assert.IsTrue("http://www.mysite.com/more?x=1&y=2".IsUrl());
		}

		[Test]
		public void IsUrl_false_for_invalid_url() {
			Assert.IsFalse("not_a_url".IsUrl());
		}

		[Test]
		public void constructor_parses_url() {
			var url = new Url("http://www.mysite.com/more?x=1&y=2");
			Assert.AreEqual("http://www.mysite.com/more", url.Path);
			CollectionAssert.AreEqual(new NameValueCollection() {{ "x", "1" }, { "y", "2" }}, url.QueryParams);
		}

		[Test]
		public void can_append_path_segment() {
			var url = "http://www.mysite.com".AppendPathSegment("endpoint");
			Assert.AreEqual("http://www.mysite.com/endpoint", url.ToString());
		}

		[Test]
		public void appending_null_path_segment_throws_arg_null_ex() {
			Assert.Throws<ArgumentNullException>(() => "http://www.mysite.com".AppendPathSegment(null));
		}

		[Test]
		public void can_append_multiple_path_segments_by_multi_args() {
			var url = "http://www.mysite.com".AppendPathSegments("category", "endpoint");
			Assert.AreEqual("http://www.mysite.com/category/endpoint", url.ToString());
		}

		[Test]
		public void can_append_multiple_path_segments_by_enumerable() {
			IEnumerable<string> segments = new[] { "category", "endpoint" };
			var url = "http://www.mysite.com".AppendPathSegments(segments);
			Assert.AreEqual("http://www.mysite.com/category/endpoint", url.ToString());
		}

		[Test]
		public void can_add_query_param() {
			var url = "http://www.mysite.com".AddQueryParam("x", 1);
			Assert.AreEqual("http://www.mysite.com?x=1", url.ToString());
		}

		[Test]
		public void can_add_multiple_query_params_from_anon_object() {
			var url = "http://www.mysite.com".AddQueryParams(new { x = 1, y = 2 });
			Assert.AreEqual("http://www.mysite.com?x=1&y=2", url.ToString());
		}

		[Test]
		public void can_add_multiple_query_params_from_dictionary() {
			// let's challenge it a little with non-string keys
			var url = "http://www.mysite.com".AddQueryParams(new Dictionary<int, string> {{1, "x"}, {2, "y"}});
			Assert.AreEqual("http://www.mysite.com?1=x&2=y", url.ToString());
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
			var url = "http://www.mysite.com/more?x=1&y=2".RemoveQueryParams(new[] {"x", "y"});
			Assert.AreEqual("http://www.mysite.com/more", url.ToString());
		}

		[Test]
		public void can_do_crazy_long_fluent_expression() {
			var url = "http://www.mysite.com"
				.AddQueryParams(new { a = 1, b = 2, c = 999 })
				.AppendPathSegment("category")
				.RemoveQueryParam("c")
				.AddQueryParam("z", 55)
				.RemoveQueryParams("a", "z")
				.AddQueryParams(new { n = "hi", m = "bye" })
				.AppendPathSegment("endpoint");

			Assert.AreEqual("http://www.mysite.com/category/endpoint?b=2&n=hi&m=bye", url.ToString());
		}

		[Test]
		public void encodes_invalid_path_chars() {
			var url = "http://www.mysite.com".AppendPathSegment("hey there how are ya");
			Assert.AreEqual("http://www.mysite.com/hey%20there%20how%20are%20ya", url.ToString());
		}

		[Test]
		public void does_not_reencode_path_escape_chars() {
			var url = "http://www.mysite.com".AppendPathSegment("hey+there+how+are+ya");
			Assert.AreEqual("http://www.mysite.com/hey+there+how+are+ya", url.ToString());
		}

		[Test]
		public void encodes_query_params() {
			var url = "http://www.mysite.com".AddQueryParams(new { x = "$50", y = "2+2=4" });
			Assert.AreEqual("http://www.mysite.com?x=%2450&y=2%2b2%3d4", url.ToString());			
		}
	}
}
