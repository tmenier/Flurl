using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Flurl.Http")]

namespace Flurl.Util
{
	/// <summary>
	/// CommonExtensions for objects.
	/// </summary>
	public static class CommonExtensions
	{
		/// <summary>
		/// Returns a key-value-pairs representation of the object.
		/// For strings, URL query string format assumed and pairs are parsed from that.
		/// For objects that already implement IEnumerable&lt;KeyValuePair&gt;, the object itself is simply returned.
		/// For all other objects, all publicly readable properties are extracted and returned as pairs.
		/// </summary>
		/// <param name="obj">The object to parse into key-value pairs</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
		public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs(this object obj) {
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			return
				obj is string s ? StringToKV(s) :
				obj is IEnumerable e ? CollectionToKV(e) :
				ObjectToKV(obj);
		}

		/// <summary>
		/// Returns a string that represents the current object, using CultureInfo.InvariantCulture where possible.
		/// Dates are represented in IS0 8601.
		/// </summary>
		public static string ToInvariantString(this object obj) {
			// inspired by: http://stackoverflow.com/a/19570016/62600
			return
				obj == null ? null :
				obj is DateTime dt ? dt.ToString("o", CultureInfo.InvariantCulture) :
				obj is DateTimeOffset dto ? dto.ToString("o", CultureInfo.InvariantCulture) :
				obj is IConvertible c ? c.ToString(CultureInfo.InvariantCulture) :
				obj is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) :
				obj.ToString();
		}

		internal static bool OrdinalEquals(this string s, string value, bool ignoreCase = false) =>
			s != null && s.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

		internal static bool OrdinalContains(this string s, string value, bool ignoreCase = false) =>
			s != null && s.IndexOf(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;

		internal static bool OrdinalStartsWith(this string s, string value, bool ignoreCase = false) =>
			s != null && s.StartsWith(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

		internal static bool OrdinalEndsWith(this string s, string value, bool ignoreCase = false) =>
			s != null && s.EndsWith(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

		/// <summary>
		/// Splits at the first occurrence of the given separator.
		/// </summary>
		/// <param name="s">The string to split.</param>
		/// <param name="separator">The separator to split on.</param>
		/// <returns>Array of at most 2 strings. (1 if separator is not found.)</returns>
		public static string[] SplitOnFirstOccurence(this string s, string separator) {
			// Needed because full PCL profile doesn't support Split(char[], int) (#119)
			if (string.IsNullOrEmpty(s))
				return new[] { s };

			var i = s.IndexOf(separator);
			return (i == -1) ?
				new[] { s } :
				new[] { s.Substring(0, i), s.Substring(i + separator.Length) };
		}

		private static IEnumerable<KeyValuePair<string, object>> StringToKV(string s) {
			return Url.ParseQueryParams(s).Select(p => new KeyValuePair<string, object>(p.Name, p.Value));
		}

		private static IEnumerable<KeyValuePair<string, object>> ObjectToKV(object obj) =>
			from prop in obj.GetType().GetProperties()
			let getter = prop.GetGetMethod(false)
			where getter != null
			let val = getter.Invoke(obj, null)
			select new KeyValuePair<string, object>(prop.Name, val);

		private static IEnumerable<KeyValuePair<string, object>> CollectionToKV(IEnumerable col) {
			bool TryGetProp(object obj, string name, out object value) {
				var prop = obj.GetType().GetProperty(name);
				var field = obj.GetType().GetField(name);

				if (prop != null) {
					value = prop.GetValue(obj, null);
					return true;
				}
				if (field != null) {
					value = field.GetValue(obj);
					return true;
				}
				value = null;
				return false;
			}

			bool IsTuple2(object item, out object name, out object val) {
				name = null;
				val = null;
				return
					item.GetType().Name.OrdinalContains("Tuple") &&
					TryGetProp(item, "Item1", out name) &&
					TryGetProp(item, "Item2", out val) &&
					!TryGetProp(item, "Item3", out _);
			}

			bool LooksLikeKV(object item, out object name, out object val) {
				name = null;
				val = null;
				return
					(TryGetProp(item, "Key", out name) || TryGetProp(item, "key", out name) || TryGetProp(item, "Name", out name) || TryGetProp(item, "name", out name)) &&
					(TryGetProp(item, "Value", out val) || TryGetProp(item, "value", out val));
			}

			foreach (var item in col) {
				if (item == null)
					continue;
				if (!IsTuple2(item, out var name, out var val) && !LooksLikeKV(item, out name, out val))
					yield return new KeyValuePair<string, object>(item.ToInvariantString(), null);
				else if (name != null)
					yield return new KeyValuePair<string, object>(name.ToInvariantString(), val);
			}
		}

		/// <summary>
		/// Merges the key/value pairs from d2 into d1, without overwriting those already set in d1.
		/// </summary>
		public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2) {
			foreach (var kv in d2.Where(x => !d1.ContainsKey(x.Key)).ToList()) {
				d1[kv.Key] = kv.Value;
			}
		}

		/// <summary>
		/// Strips any single quotes or double quotes from the beginning and end of a string.
		/// </summary>
		public static string StripQuotes(this string s) => Regex.Replace(s, "^\\s*['\"]+|['\"]+\\s*$", "");

		/// <summary>
		/// True if the given string is a valid IPv4 address.
		/// </summary>
		public static bool IsIP(this string s) {
			// based on https://stackoverflow.com/a/29942932/62600
			if (string.IsNullOrEmpty(s))
				return false;

			var parts = s.Split('.');
			return parts.Length == 4 && parts.All(x => byte.TryParse(x, out _));
		}
	}
}