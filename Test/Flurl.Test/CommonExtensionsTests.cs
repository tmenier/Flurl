using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Flurl.Util;

namespace Flurl.Test
{
	[TestFixture, Parallelizable]
	public class CommonExtensionsTests
	{
		[Test]
		public void can_parse_object_to_kv()
		{
			var kv = new {
				one = 1,
				two = "foo",
				three = (string)null
			}.ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", "foo" },
				{ "three", null }
			}, kv);
		}

		[Test]
		public void can_parse_dictionary_to_kv()
		{
			var kv = new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", "foo" },
				{ "three", null }
			}.ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", "foo" },
				{ "three", null }
			}, kv);
		}

		[Test]
		public void can_parse_collection_of_kvp_to_kv()
		{
			var kv = new[] {
				new KeyValuePair<object, object>("one", 1),
				new KeyValuePair<object, object>("two", "foo"),
				new KeyValuePair<object, object>("three", null),
				new KeyValuePair<object, object>(null, "four"),
			}.ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", "foo" },
				{ "three", null }
			}, kv);
		}

		[Test]
		public void can_parse_collection_of_conventional_objects_to_kv()
		{
			// convention is to accept collection of any arbitrary type that contains
			// a property called Key or Name and a property called Value
			var kv = new object[] {
				new { Key = "one", Value = 1 },
				new { key = "two", value = "foo" }, // lower-case should work too
				new { Key = (string)null, Value = 3 }, // null keys should get skipped
				new { Name = "three", Value = (string)null },
				new { name = "four", value = "bar" } // lower-case should work too
			}.ToKeyValuePairs().ToList();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", "foo" },
				{ "three", null },
				{ "four", "bar" }
			}, kv);
		}

		[Test]
		public void can_parse_string_to_kv()
		{
			var kv = "one=1&two=foo&three".ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", "1" },
				{ "two", "foo" },
				{ "three", null }
			}, kv);
		}

		[Test]
		public void cannot_parse_null_to_kv()
		{
			object obj = null;
			Assert.Throws<ArgumentNullException>(() => obj.ToKeyValuePairs());
		}

		[Test]
		public void SplitOnFirstOccurence_works() {
			var result = "hello/how/are/you".SplitOnFirstOccurence('/');
			Assert.AreEqual(new[] { "hello", "how/are/you" }, result);
		}
	}
}
