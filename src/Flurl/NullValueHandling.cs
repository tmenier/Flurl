using System;
using System.Collections.Generic;
using System.Text;

namespace Flurl
{
	/// <summary>
	/// Describes how to handle null values in query parameters.
	/// </summary>
    public enum NullValueHandling
    {
		/// <summary>
		/// Set as name without value in query string.
		/// </summary>
		NameOnly,
		/// <summary>
		/// Don't add to query string, remove any existing value.
		/// </summary>
		Remove,
		/// <summary>
		/// Don't add to query string, but leave any existing value unchanged.
		/// </summary>
		Ignore
	}
}
