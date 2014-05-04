using System;
using System.Collections;
using System.Collections.Generic;

namespace Flurl.Common
{
	public static class CommonExtensions
	{
		public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this object obj) {
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (obj is IDictionary) {
				foreach (DictionaryEntry kv in (IDictionary)obj)
					yield return new KeyValuePair<string, string>(kv.Key.ToString(), kv.Value.ToString());
			}
			else {
				foreach (var prop in obj.GetType().GetProperties()) {
					yield return new KeyValuePair<string, string>(prop.Name, prop.GetValue(obj, null).ToString());
				}
			}
		}
	}
}
