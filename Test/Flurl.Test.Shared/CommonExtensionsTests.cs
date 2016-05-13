using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Flurl.Util;

namespace Flurl.Test
{
	[TestFixture]
    public class CommonExtensionsTests
    {
		[Test]
		public void can_parse_object_to_kv() {
			var kv = new { one = 1, two = 2, three = "foo" }.ToKeyValuePairs();
			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", 2 },
				{ "three", "foo" }
			}, kv);
		}

		[Test]
		public void can_parse_dictionary_to_kv() {
			var kv = new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", 2 },
				{ "three", "foo" }
			}.ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", 2 },
				{ "three", "foo" }
			}, kv);
		}

		[Test]
		public void can_parse_collection_of_kvp_to_kv() {
			var kv = new[] {
				new KeyValuePair<object, object>("one", 1),
				new KeyValuePair<object, object>("two", 2),
				new KeyValuePair<object, object>("three", "foo"),
			}.ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", 2 },
				{ "three", "foo" }
			}, kv);
		}

		[Test]
		public void can_parse_collection_of_conventional_objects_to_kv() {
			// convention is to accept collection of any arbitrary type that contains
			// a property called Key or Name and a property called Value
			var kv = new object[] {
				new { Key = "one", Value = 1 },
				new { key = "two", value = 2 }, // lower-case should work too
				new { Key = (string)null, Value = 3 }, // null keys should get skipped
				new { Name = "three", Value = "foo" },
				new { name = "four", value = "bar" } // lower-case should work too
			}.ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", 1 },
				{ "two", 2 },
				{ "three", "foo" },
				{ "four", "bar" }
			}, kv);
		}

		[Test]
		public void can_parse_string_to_kv() {
			var kv = "one=1&two=2&three=foo".ToKeyValuePairs();

			CollectionAssert.AreEquivalent(new Dictionary<string, object> {
				{ "one", "1" },
				{ "two", "2" },
				{ "three", "foo" }
			}, kv);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void cannot_parse_null_to_kv() {
			object obj = null;
			var kv = obj.ToKeyValuePairs();
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void cannot_parse_unknown_collection_to_kv() {
			var kv = new object[] {
				new { Key = "one", Value = 1 },
				new { Foo = "two", value = 2 }
			}.ToKeyValuePairs().ToList(); // need to force it to iterate for the exception to be thrown
		}
	}
}
