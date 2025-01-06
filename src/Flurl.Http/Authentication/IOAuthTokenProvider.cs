using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Flurl.Http.Authentication
{
    /// <summary>
    /// Provides the authentication header
    /// </summary>
    public interface IOAuthTokenProvider
    {
        /// <summary>
        /// Gets the authentication header for a specified set of scopes.
        /// </summary>
        /// <param name="scopes">The desired set of scopes</param>
        /// <returns></returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeader(ISet<string> scopes);
    }
}
