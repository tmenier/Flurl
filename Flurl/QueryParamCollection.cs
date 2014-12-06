using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Flurl
{
	/// <summary>
	/// Represents a URL query string as a key-value dictionary. Insertion order is preserved.
	/// </summary>
	public class QueryParamCollection : IDictionary<string, object>
	{
		private readonly Dictionary<string, object> _dict = new Dictionary<string, object>();
 		private readonly List<string> _orderedKeys = new List<string>();

		/// <summary>
		/// Parses a query string from a URL to a QueryParamCollection dictionary.
		/// </summary>
		/// <param name="queryString">The query string to parse.</param>
		/// <returns></returns>
		public static QueryParamCollection Parse(string queryString) {
			var result = new QueryParamCollection();

			if (string.IsNullOrEmpty(queryString))
				return result;

			queryString = queryString.TrimStart('?').Split('?')[0];

			foreach (var kv in queryString.Split('&')) {
				var pair = kv.Split('=');
				var key = pair[0];
				var value = pair.Length >= 2 ? pair[1] : "";
				result.Add(key, Uri.UnescapeDataString(value));
			}

			return result;			
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
			return _orderedKeys.Select(k => new KeyValuePair<string, object>(k, _dict[k])).GetEnumerator();
		}

		/// <summary>
		/// Returns serialized, encoded query string. Insertion order is preserved.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return string.Join("&", GetPairs());
		}

		private IEnumerable<string> GetPairs() {
			foreach (var key in _orderedKeys) {
				var val = this[key];
				if (val == null)
					continue;

				if (val is string || !(val is IEnumerable)) {
					yield return key + "=" + Uri.EscapeDataString(val.ToString());
				}
				else {
					// if value is IEnumerable (other than string), break it into multiple
					// values with same param name, i.e. x=1&x2&x=3
					// https://github.com/tmenier/Flurl/issues/15
					foreach (var subval in val as IEnumerable) {
						if (subval == null)
							continue;

						yield return key + "=" + Uri.EscapeDataString(subval.ToString());
					}
				}
			}
		}

		#region IDictionary<string, object> members
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(KeyValuePair<string, object> item) {
			Add(item.Key, item.Value);
		}

		public void Clear() {
			_dict.Clear();
			_orderedKeys.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item) {
			return _dict.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
			((ICollection)_dict).CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, object> item) {
			return Remove(item.Key);
		}

		public int Count {
			get { return _dict.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public void Add(string key, object value) {
			_dict.Add(key, value);
			_orderedKeys.Add(key);
		}

		public bool ContainsKey(string key) {
			return _dict.ContainsKey(key);
		}

		public bool Remove(string key) {
			_orderedKeys.Remove(key);
			return _dict.Remove(key);
		}

		public bool TryGetValue(string key, out object value) {
			return _dict.TryGetValue(key, out value);
		}

		public object this[string key] {
			get {
				return _dict[key];
			}
			set {
				_dict[key] = value;
				if (!_orderedKeys.Contains(key))
					_orderedKeys.Add(key);
			}
		}

		public ICollection<string> Keys {
			get { return _orderedKeys; }
		}

		public ICollection<object> Values {
			get { return _orderedKeys.Select(k => _dict[k]).ToArray(); }
		}
		#endregion
	}
}
