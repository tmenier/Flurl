using System;
using System.Collections.Generic;
using System.Linq;

namespace Flurl.Util
{
	/// <summary>
	/// Defines common methods for INameValueList and IReadOnlyNameValueList.
	/// </summary>
	public interface INameValueListBase<TValue>
	{
		/// <summary>
		/// Returns the first Value of the given Name if one exists, otherwise null or default value.
		/// </summary>
		TValue FirstOrDefault(string name);

		/// <summary>
		/// Gets the first Value of the given Name, if one exists.
		/// </summary>
		/// <returns>true if any item of the given name is found, otherwise false.</returns>
		bool TryGetFirst(string name, out TValue value);

		/// <summary>
		/// Gets all Values of the given Name.
		/// </summary>
		IEnumerable<TValue> GetAll(string name);

		/// <summary>
		/// True if any items with the given Name exist.
		/// </summary>
		bool Contains(string name);

		/// <summary>
		/// True if any item with the given Name and Value exists.
		/// </summary>
		bool Contains(string name, TValue value);
	}

	/// <summary>
	/// Defines an ordered collection of Name/Value pairs where duplicate names are allowed but aren't typical.
	/// </summary>
	public interface INameValueList<TValue> : IList<(string Name, TValue Value)>, INameValueListBase<TValue>
	{
		/// <summary>
		/// Adds a new Name/Value pair.
		/// </summary>
		void Add(string name, TValue value);

		/// <summary>
		/// Replaces the first occurrence of the given Name with the given Value and removes any others,
		/// or adds a new Name/Value pair if none exist.
		/// </summary>
		void AddOrReplace(string name, TValue value);

		/// <summary>
		/// Removes all items of the given Name.
		/// </summary>
		/// <returns>true if any item of the given name is found, otherwise false.</returns>
		bool Remove(string name);
	}

	/// <summary>
	/// Defines a read-only ordered collection of Name/Value pairs where duplicate names are allowed but aren't typical.
	/// </summary>
	public interface IReadOnlyNameValueList<TValue> : IReadOnlyList<(string Name, TValue Value)>, INameValueListBase<TValue>
	{
	}

	/// <summary>
	/// An ordered collection of Name/Value pairs where duplicate names are allowed but aren't typical.
	/// Useful for things where a dictionary would work great if not for those pesky edge cases (headers, cookies, etc).
	/// </summary>
	public class NameValueList<TValue> : List<(string Name, TValue Value)>, INameValueList<TValue>, IReadOnlyNameValueList<TValue>
	{
		private bool _caseSensitiveNames;

		/// <summary>
		/// Instantiates a new empty NameValueList.
		/// </summary>
		public NameValueList(bool caseSensitiveNames) {
			_caseSensitiveNames = caseSensitiveNames;
		}

		/// <summary>
		/// Instantiates a new NameValueList with the Name/Value pairs provided.
		/// </summary>
		public NameValueList(IEnumerable<(string Name, TValue Value)> items, bool caseSensitiveNames) {
			_caseSensitiveNames = caseSensitiveNames;
			AddRange(items);
		}

		/// <inheritdoc />
		public void Add(string name, TValue value) => Add((name, value));

		/// <inheritdoc />
		public void AddOrReplace(string name, TValue value) {
			var i = 0;
			var replaced = false;
			while (i < this.Count) {
				if (!this[i].Name.OrdinalEquals(name, !_caseSensitiveNames))
					i++;
				else if (replaced)
					this.RemoveAt(i);
				else {
					this[i] = (name, value);
					replaced = true;
					i++;
				}
			}

			if (!replaced)
				this.Add(name, value);
		}

		/// <inheritdoc />
		public bool Remove(string name) => RemoveAll(x => x.Name.OrdinalEquals(name, !_caseSensitiveNames)) > 0;

		/// <inheritdoc />
		public TValue FirstOrDefault(string name) => GetAll(name).FirstOrDefault();

		/// <inheritdoc />
		public bool TryGetFirst(string name, out TValue value) {
			foreach (var v in GetAll(name)) {
				value = v;
				return true;
			}
			value = default;
			return false;
		}

		/// <inheritdoc />
		public IEnumerable<TValue> GetAll(string name) => this
			.Where(x => x.Name.OrdinalEquals(name, !_caseSensitiveNames))
			.Select(x => x.Value);

		/// <inheritdoc />
		public bool Contains(string name) => this.Any(x => x.Name.OrdinalEquals(name, !_caseSensitiveNames));

		/// <inheritdoc />
		public bool Contains(string name, TValue value) => Contains((name, value));
	}
}
