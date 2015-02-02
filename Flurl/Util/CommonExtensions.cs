using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

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
		public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object obj) {
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (obj is IDictionary) {
				foreach (DictionaryEntry kv in (IDictionary)obj)
					yield return new KeyValuePair<string, object>(kv.Key.ToInvariantString(), kv.Value);
			}
			else {
				foreach (var prop in obj.GetType().GetProperties()) {
					var val = prop.GetValue(obj, null);
					yield return new KeyValuePair<string, object>(prop.Name, val);
				}
			}
		}

		/// <summary>
		/// Returns a string that represents the current object, using CultureInfo.InvariantCulture where possible.
		/// </summary>
		public static string ToInvariantString(this object obj) {
			// inspired by: http://stackoverflow.com/a/19570016/62600

			var c = obj as IConvertible;
			if (c != null) 
				return c.ToString(CultureInfo.InvariantCulture);

			var f = obj as IFormattable;
			if (f != null)
				return f.ToString(null, CultureInfo.InvariantCulture);

			return obj.ToString();
		}
	}
}
