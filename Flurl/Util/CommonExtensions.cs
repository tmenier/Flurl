using System;
using System.Collections;
using System.Collections.Generic;

namespace Flurl.Util
{
	public static class CommonExtensions
	{
		/// <summary>
		/// Converts an object's public properties to a collection of string-based key-value pairs. If the object happens
		/// to be an IDictionary, the IDictionary's keys and values converted to strings and returned.
		/// </summary>
		/// <param name="obj">The object to parse into key-value pairs</param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this object obj) {
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (obj is IDictionary) {
				foreach (DictionaryEntry kv in (IDictionary)obj)
					yield return new KeyValuePair<string, string>(kv.Key.ToInvariantString(), kv.Value.ToInvariantString());
			}
			else {
				foreach (var prop in obj.GetType().GetProperties()) {
					yield return new KeyValuePair<string, string>(prop.Name, prop.GetValue(obj, null).ToInvariantString());
				}
			}
		}

		/// <summary>
		/// Converts an object to string using invariant culture format provider.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static string ToInvariantString(this object obj)
		{
			return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", obj);
		}
	}
}
