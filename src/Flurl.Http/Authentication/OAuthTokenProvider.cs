using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// The default set of empty scopes
        /// </summary>
        protected static readonly ISet<string> EmptyScope = new HashSet<string>();

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
        /// <param name="scopes">The desired set of scopes</param>
        /// <returns></returns>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeader(ISet<string> scopes)
        {
            var now = DateTimeOffset.Now;

            scopes??= EmptyScope;
            
            var cacheKey = string.Join(" ", scopes);
            
            //if the scope is not in the cache, add it as an expired entry so we force a refresh
            var entry = _tokens.GetOrAdd(cacheKey, s =>
            {
                return new CacheEntry
                {
                    Token = new ExpirableToken("", now)
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
                        var generatedToken = await GetToken(scopes);

                        //if we're configured to expire tokens early, adjust the expiration time
                        if (_earlyExpiration > TimeSpan.Zero)
                        { generatedToken = new ExpirableToken(generatedToken.Value, generatedToken.Expiration - _earlyExpiration); }

                        entry.Token = generatedToken;
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
        /// Retrieves the OAuth token for the specified scopes
        /// </summary>
        /// <returns>The refreshed OAuth token</returns>
        protected abstract Task<ExpirableToken> GetToken(ISet<string> scopes);
    }
}
