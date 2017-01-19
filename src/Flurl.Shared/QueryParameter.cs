using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flurl
{
	/// <summary>
	/// Represents an individual name/value pair within a URL query.
	/// </summary>
	public class QueryParameter
	{
		private object _value;
		private string _encodedValue;
	    private readonly bool _isWithoutValue = false;

		/// <summary>
		/// Creates a new instance of a query parameter.
		/// </summary>
		public QueryParameter(string name, object value) {
			Name = name;
			Value = value;
		}

		/// <summary>
		/// Creates a new instance of a query parameter. Allows specifying whether string value provided has
		/// already been URL-encoded.
		/// </summary>
		public QueryParameter(string name, string value, bool isEncoded) {
			Name = name;
			if (isEncoded) {
				_encodedValue = value as string;
				_value = Url.DecodeQueryParamValue(_encodedValue);
			}
			else {
				Value = value;
			}
		}

        /// <summary>
        /// Creates a new instance of a query parameter without a value.
        /// </summary>
        public QueryParameter(string name) {
            Name = name;
            Value = "";
            _isWithoutValue = true;
        }

        /// <summary>
        /// The name (left side) of the query parameter.
        /// </summary>
        public string Name { get; set; }

		/// <summary>
		/// The value (right side) of the query parameter.
		/// </summary>
		public object Value
        {
			get { return _isWithoutValue ? true : _value; }
			set
			{
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
		    if (Value is IEnumerable && !(Value is string)) {
		        return string.Join("&",
		            from v in (Value as IEnumerable).Cast<object>()
		            where v != null
		            let encoded = Url.EncodeQueryParamValue(v, encodeSpaceAsPlus)
		            select $"{Name}={encoded}");
		    }
		    else if (_isWithoutValue) {
		        return $"{Name}";
		    }
			else {
				var encoded = _encodedValue ?? Url.EncodeQueryParamValue(_value, encodeSpaceAsPlus);
				return $"{Name}={encoded}";
			}
		}
	}
}
