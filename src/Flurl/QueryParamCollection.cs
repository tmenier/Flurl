using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Flurl
{
	/// <summary>
	/// Represents a URL query as a key-value dictionary. Insertion order is preserved.
	/// </summary>
	public class QueryParamCollection : List<QueryParameter>
	{
		/// <summary>
		/// Returns a new instance of QueryParamCollection
		/// </summary>
		/// <param name="query">Optional query string to parse.</param>
		public QueryParamCollection(string query = null) {
			if (query != null)
				AddRange(Url.ParseQueryParams(query));
		}

		/// <summary>
		/// Returns serialized, encoded query string. Insertion order is preserved.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return ToString(false);
		}

		/// <summary>
		/// Returns serialized, encoded query string. Insertion order is preserved.
		/// </summary>
		/// <returns></returns>
		public string ToString(bool encodeSpaceAsPlus) {
			return string.Join("&", this.Where(p => p != null).Select(p => p.ToString(encodeSpaceAsPlus)));
		}

		/// <summary>
		/// Adds a new query parameter.
		/// </summary>
		public void Add(string key, object value) {
			Add(new QueryParameter(key, value));
		}

		/// <summary>
		/// Adds a new query parameter, allowing you to specify whether the value is already encoded.
		/// </summary>
		public void Add(string key, string value, bool isEncoded) {
			Add(new QueryParameter(key, value, isEncoded));
		}

		/// <summary>
		/// True if the collection contains a query parameter with the given name.
		/// </summary>
		public bool ContainsKey(string name) {
			return this.Any(p => p.Name == name);
		}

	    /// <summary>
	    /// Removes all parameters of the given name.
	    /// </summary>
	    /// <returns>The number of parameters that were removed</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="name" /> is null.</exception>
	    public int Remove(string name) {
			return RemoveAll(p => p.Name == name);
		}

		/// <summary>
		/// Replaces an existing QueryParameter or appends one to the end. If object is a collection type (array, IEnumerable, etc.),
		/// multiple parameters are added, i.e. x=1&amp;x=2. If any of the same name already exist, they are overwritten one by one
		/// (preserving order) and any remaining are appended to the end. If fewer values are specified than already exist,
		/// remaining existing values are removed.
		/// </summary>
		public void Merge(string name, object value, bool isEncoded, NullValueHandling nullValueHandling) {
			if (value == null && nullValueHandling != NullValueHandling.NameOnly) {
				if (nullValueHandling == NullValueHandling.Remove)
					Remove(name);
				return;
			}

			// This covers some complex edge cases involving multiple values of the same name.
			// example: x has values at positions 2 and 4 in the query string, then we set x to
			// an array of 4 values. We want to replace the values at positions 2 and 4 with the
			// first 2 values of the new array, then append the remaining 2 values to the end.
			var parameters = this.Where(p => p.Name == name).ToArray();
			var values = (!(value is string) && value is IEnumerable en) ? en.Cast<object>().ToArray() : new[] { value };

			for (int i = 0;; i++) {
				if (i < parameters.Length && i < values.Length) {
					if (values[i] is QueryParameter qp)
						this[IndexOf(parameters[i])] = qp;
					else
						parameters[i].Value = values[i];
				}
				else if (i < parameters.Length)
					Remove(parameters[i]);
				else if (i < values.Length) {
					var qp = values[i] as QueryParameter ?? new QueryParameter(name, values[i], isEncoded);
					Add(qp);
				}
				else
					break;
			}
		}

		/// <summary>
		/// Gets or sets a query parameter value by name. A query may contain multiple values of the same name
		/// (i.e. "x=1&amp;x=2"), in which case the value is an array, which works for both getting and setting.
		/// </summary>
		/// <param name="name">The query parameter name</param>
		/// <returns>The query parameter value or array of values</returns>
		public object this[string name] {
			get {
				var all = this.Where(p => p.Name == name).Select(p => p.Value).ToArray();
				if (all.Length == 0)
					return null;
				if (all.Length == 1)
					return all[0];
				return all;
			}
			set => Merge(name, value, false, NullValueHandling.Remove);
		}
	}
}