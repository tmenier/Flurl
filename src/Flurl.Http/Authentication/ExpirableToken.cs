using System;
using System.Diagnostics.CodeAnalysis;

namespace Flurl.Http.Authentication
{
	/// <summary>
	/// An expirable token used in token-based authentication
	/// </summary>
	[ExcludeFromCodeCoverage]
    public sealed class ExpirableToken
    {
        /// <summary>
        /// Gets and sets the token value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets and sets the expiration date of the token
        /// </summary>
        public DateTimeOffset Expiration { get; }

        /// <summary>
        /// Instantiates a new ExpirableToken, setting value and expiration
        /// </summary>
        /// <param name="value">The value of this token</param>
        /// <param name="expiration">The date and time that this token expires</param>
        public ExpirableToken(string value, DateTimeOffset expiration)
        {
            Value = value;
            Expiration = expiration;
        }
    }
}
