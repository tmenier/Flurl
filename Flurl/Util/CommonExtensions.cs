using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

			return
				(obj is string) ? QueryParamCollection.Parse((string)obj) :
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

		private static IEnumerable<KeyValuePair<string, object>> ObjectToKV(object obj) {
			return from prop in obj.GetType().GetProperties() 
				   let val = prop.GetValue(obj, null) 
				   select new KeyValuePair<string, object>(prop.Name, val);
		}

		private static IEnumerable<KeyValuePair<string, object>> CollectionToKV(IEnumerable col) {
			// Accepts KeyValuePairs or any aribitray types that contain a property called "Key" or "Name" and a property called "Value".
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
