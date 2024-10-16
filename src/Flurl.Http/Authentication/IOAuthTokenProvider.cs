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
        /// Gets the authentication header for a specified scope.
        /// </summary>
        /// <param name="scope">The desired scope</param>
        /// <returns></returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeader(string scope);
    }
}
