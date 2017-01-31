using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
#if NETSTANDARD1_4
using System.Reflection;
#endif

namespace Flurl.Util
{
	/// <summary>
	/// CommonExtensions for objects.
	/// </summary>
	public static class CommonExtensions
	{
	    /// <summary>
	    /// Converts an object's public properties to a collection of string-based key-value pairs. If the object happens
	    /// to be an IDictionary, the IDictionary's keys and values converted to strings and returned.
	    /// </summary>
	    /// <param name="obj">The object to parse into key-value pairs</param>
	    /// <returns></returns>
	    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
	    public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object obj) {
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			return
				(obj is string) ? StringToKV((string)obj) :
				(obj is IEnumerable) ? CollectionToKV((IEnumerable)obj) :
				ObjectToKV(obj);
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

		/// <summary>
		/// Splits at the first occurence of the given seperator.
		/// </summary>
		/// <param name="s">The string to split.</param>
		/// <param name="separator">The separator to split on.</param>
		/// <returns>Array of at most 2 strings. (1 if separator is not found.)</returns>
		public static string[] SplitOnFirstOccurence(this string s, char separator) {
			// Needed because full PCL profile doesn't support Split(char[], int) (#119)
			if (string.IsNullOrEmpty(s))
				return new[] { s };

			var i = s.IndexOf(separator);
			if (i == -1)
				return new[] { s };

			return new[] { s.Substring(0, i), s.Substring(i + 1) };
		}

		private static IEnumerable<KeyValuePair<string, object>> StringToKV(string s) {
			return Url.ParseQueryParams(s).Select(p => new KeyValuePair<string, object>(p.Name, p.Value));
		}

		private static IEnumerable<KeyValuePair<string, object>> ObjectToKV(object obj) {
            //return from prop in obj.GetType().GetProperties()
            //       let val = prop.GetValue(obj, null)
            //       select new KeyValuePair<string, object>(prop.Name, val);
            JToken token = obj as JToken;
            if (token == null)
            {
                return ObjectToKV(JObject.FromObject(obj));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, object>();
                foreach (var child in token.Children().ToList())
                {
                    contentData = contentData.Concat(ObjectToKV(child)).ToDictionary(k => k.Key, v => v.Value);
                }

                return contentData;
            }

            var jValue = token as JValue;
            return new Dictionary<string, object> { { token.Path, jValue?.Value } };
        }

		private static IEnumerable<KeyValuePair<string, object>> CollectionToKV(IEnumerable col) {
			// Accepts KeyValuePairs or any arbitrary types that contain a property called "Key" or "Name" and a property called "Value".
			foreach (var item in col) {
				if (item == null)
					continue;

				var type = item.GetType();
				var keyProp = type.GetProperty("Key") ?? type.GetProperty("key") ?? type.GetProperty("Name") ?? type.GetProperty("name");
				if (keyProp == null)
					throw new ArgumentException("Cannot parse " + type.Name + " to key-value pair. Type must contain a property called 'Key' or 'Name'.");

				var valProp = type.GetProperty("Value") ?? type.GetProperty("value");
				if (valProp == null)
					throw new ArgumentException("Cannot parse " + type.Name + " to key-value pair. Type must contain a property called 'Value'.");

				var key = keyProp.GetValue(item, null);
				if (key == null)
					continue;

				var val = valProp.GetValue(item, null);
				yield return new KeyValuePair<string, object>(key.ToInvariantString(), val);
			}
		}
	}
}