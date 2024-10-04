using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Flurl.Http.Authentication
{
    /// <summary>
    ///
    /// </summary>
    public class ClientCredentialsTokenProvider : OAuthTokenProvider
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IFlurlClient _fc;

        /// <summary>
        /// Instantiates a new OAuthTokenProvider that uses clientId/clientSecret as an authentication mechanism
        /// </summary>
        /// <param name="clientId">The client id to send when making requests for the OAuth token</param>
        /// <param name="client">The <see cref="IFlurlClient"/> to use when making requests for the OAuth token. Leave null to omit this property from the body of the request</param>
        /// <param name="clientSecret">The client secret to send when making requests for the OAuth token</param>
        /// <param name="earlyExpiration">The amount of time that defines how much earlier a entry should be considered expired relative to its actual expiration</param>
        /// <param name="authenticationScheme">The authentication scheme this provider will provide in the resolved authentication header. Usually "Bearer" or "OAuth"</param>
        public ClientCredentialsTokenProvider(string clientId,
                                            string clientSecret,
                                            IFlurlClient client,
                                            TimeSpan? earlyExpiration = null,
                                            string authenticationScheme = "Bearer")
            : base(earlyExpiration, authenticationScheme)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _fc = client;
        }

        /// <summary>
        /// Gets the OAuth authentication header for the specified scope
        /// </summary>
        /// <param name="scope">The desired scope</param>
        /// <returns></returns>
        protected override async Task<ExpirableToken> GetToken(string scope)
        {
            var now = DateTimeOffset.Now;

            var body = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["scope"] = scope,
                ["grant_type"] = "client_credentials"
            };

            if (string.IsNullOrWhiteSpace(_clientSecret) == false)
            { body["client_secret"] = _clientSecret; }

            var rawResponse = await _fc.Request("connect", "token")
                                        .WithHeader("accept","application/json")
                                        .AllowAnyHttpStatus()
                                        .PostUrlEncodedAsync(body);

            if (rawResponse.StatusCode >= 200 && rawResponse.StatusCode < 300)
            {
                var tokenResponse = await rawResponse.GetJsonAsync<JsonNode>();
                var expiration = now.AddSeconds(tokenResponse["expires_in"].GetValue<int>());

                return new ExpirableToken(tokenResponse["access_token"].GetValue<string>(), expiration);
            }
            if (rawResponse.StatusCode == 400)
            {
                var errorMessage = default(string);
                try
                {
                    var response = await rawResponse.GetJsonAsync<JsonNode>();
                    errorMessage = response["error"].GetValue<string>();

                    if (errorMessage == "invalid_scope")
                    { errorMessage = $"{_clientId} is not allowed to utilize scope {scope}, or {scope} is not a valid scope. Verify the allowed scopes for {_clientId} and try again."; }
                }
                catch (Exception)
                {
                    errorMessage = $"{_clientId} is not allowed to utilize scope {scope}, or {scope} is not a valid scope. Verify the allowed scopes for {_clientId} and try again.";
                }

                throw new UnauthorizedAccessException(errorMessage);
            }
            else
            {
                throw new UnauthorizedAccessException("Unable to acquire OAuth token");
            }
        }
    }
}


