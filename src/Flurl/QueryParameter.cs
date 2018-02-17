using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flurl.Util;

namespace Flurl
{
	/// <summary>
	/// Represents an individual name/value pair within a URL query.
	/// </summary>
	public class QueryParameter
	{
		private object _value;
		private string _encodedValue;

		/// <summary>
		/// Creates a new instance of a query parameter. Allows specifying whether string value provided has
		/// already been URL-encoded.
		/// </summary>
		public QueryParameter(string name, object value, bool isEncoded = false) {
			Name = name;
			if (isEncoded && value != null) {
				_encodedValue = value as string;
				_value = Url.Decode(_encodedValue, true);
			}
			else {
				Value = value;
			}
		}

		/// <summary>
		/// The name (left side) of the query parameter.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The value (right side) of the query parameter.
		/// </summary>
		public object Value {
			get => _value;
			set {
				_value = value;
				_encodedValue = null;
			}
		}

		/// <summary>
		/// Returns the string ("name=value") representation of the query parameter.
		/// </summary>
		/// <param name="encodeSpaceAsPlus">Indicates whether to encode space characters with "+" instead of "%20".</param>
		/// <returns></returns>
		public string ToString(bool encodeSpaceAsPlus) {
			if (_value is IEnumerable && !(_value is string)) {
				return string.Join("&",
					from v in (_value as IEnumerable).Cast<object>()
					select BuildPair(Name, v, false, encodeSpaceAsPlus));
			}
			return BuildPair(Name, _encodedValue ?? Value, _encodedValue != null, encodeSpaceAsPlus);
		}

		private static string BuildPair(string name, object value, bool valueIsEncoded, bool encodeSpaceAsPlus) {
			name = Url.EncodeIllegalCharacters(name, encodeSpaceAsPlus);
			if (value == null)
				return name;

			value = valueIsEncoded ? value : Url.Encode(value.ToInvariantString(), encodeSpaceAsPlus);
			return $"{name}={value}";
		}
	}
}
