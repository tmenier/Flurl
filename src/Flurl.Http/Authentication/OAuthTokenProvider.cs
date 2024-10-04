using System;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http.Authentication
{
    /// <summary>
    /// The base class for OAuth token providers
    /// </summary>
    public abstract class OAuthTokenProvider : IOAuthTokenProvider
    {
        private class CacheEntry
        {
            public SemaphoreSlim Semaphore { get; }
            public ExpirableToken Token { get; set; }
            public AuthenticationHeaderValue AuthHeader { get; set; }

            public CacheEntry()
            {
                Semaphore = new SemaphoreSlim(1, 1);
            }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _tokens;
        private readonly TimeSpan _earlyExpiration;
        private readonly string _scheme;


        /// <summary>
        /// Instantiates a new OAuthTokenProvider
        /// </summary>
        /// <param name="earlyExpiration">The amount of time that defines how much earlier a entry should be considered expired relative to its actual expiration</param>
        /// <param name="authenticationScheme">The authentication scheme this provider will provide in the resolved authentication header. Usually "Bearer" or "OAuth"</param>
        protected OAuthTokenProvider(
                                    TimeSpan? earlyExpiration = null,
                                    string authenticationScheme = "Bearer")
        {
            _tokens = new ConcurrentDictionary<string, CacheEntry>();
            _earlyExpiration = earlyExpiration ?? TimeSpan.Zero;
            _scheme = authenticationScheme;
        }

        /// <summary>
        /// Gets the OAuth authentication header for the specified scope
        /// </summary>
        /// <param name="scope">The desired scope</param>
        /// <returns></returns>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeader(string scope)
        {
            var now = DateTimeOffset.Now;

            //if the scope is not in the cache, add it as an expired entry so we force a refresh
            var entry = _tokens.GetOrAdd(scope, s =>
            {
                return new CacheEntry
                {
                    Token = new ExpirableToken("", now - TimeSpan.FromMinutes(1))
                };
            });

            var tokenIsValid = (entry.AuthHeader != null) && (now < entry.Token.Expiration);

            if (tokenIsValid == false)
            {
                await entry.Semaphore.WaitAsync();
                try
                {
                    tokenIsValid = (entry.AuthHeader != null) && (now < entry.Token.Expiration);

                    if (tokenIsValid == false)
                    {
                        var generatedToken = await GetToken(scope);
                        entry.Token = new ExpirableToken(generatedToken.Value, generatedToken.Expiration - _earlyExpiration);
                        entry.AuthHeader = new AuthenticationHeaderValue(_scheme, entry.Token.Value);
                    }
                }
                finally
                {
                    entry.Semaphore.Release();
                }
            }

            return entry.AuthHeader;
        }

        /// <summary>
        /// Retrieves the OAuth token for the specified scope
        /// </summary>
        /// <returns>The refreshed OAuth token</returns>
        protected abstract Task<ExpirableToken> GetToken(string scope);
    }
}
