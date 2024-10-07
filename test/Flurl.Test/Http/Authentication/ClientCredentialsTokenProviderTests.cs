using Flurl.Http;
using Flurl.Http.Authentication;
using Flurl.Http.Testing;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flurl.Test.Http.Authentication
{
    [TestFixture]
    public class ClientCredentialsTokenProviderTests : HttpTestFixtureBase
    {
        [TestCase("secret")]
        [TestCase("")]
        [TestCase(null)]
        public async Task GetAuthenticationHeader_OnlyPassesClientSecretIfSet(string clientSecret)
        {
            var body = new
            {
                access_token = "UnitTestAccessToken",
                expires_in = 3600
            };

            HttpTest.RespondWithJson(body);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", clientSecret, cli);

            var authHeader = await provider.GetAuthenticationHeader("scope");

            HttpTest.ShouldHaveCalled("https://flurl.dev/connect/token")
                    .WithVerb(HttpMethod.Post)
                    .WithContentType("application/x-www-form-urlencoded")
                    .WithRequestBody($"client_id=clientId&scope=scope&grant_type=client_credentials{(string.IsNullOrWhiteSpace(clientSecret) ? "" : $"&client_secret={clientSecret}")}")
                    .WithHeader("accept", "application/json")
                    .Times(1);
        }

        [Test]
        public async Task GetAuthenticationHeader_ReturnsTokenForSuccessfulResponse()
        {
            var body = new
            {
                access_token = "UnitTestAccessToken",
                expires_in = 3600
            };

            HttpTest.RespondWithJson(body);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", "secret", cli);

            var authHeader = await provider.GetAuthenticationHeader("scope");

            Assert.AreEqual("UnitTestAccessToken", authHeader.Parameter);
        }

        [Test]
        public void GetAuthenticationHeader_ThrowsUnauthorizedForErrorMessageResponse()
        {
            var body = new
            {
                error = "invalid_scope",
            };

            HttpTest.RespondWithJson(body, 400);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", "secret", cli);

            Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAuthenticationHeader("testScope"));
        }

        [Test]
        public void GetAuthenticationHeader_ThrowsUnauthorizedForGarbageResponse()
        {
            HttpTest.RespondWith("garbage", 400);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", "secret", cli);

            var expectedMessage = $"unitTestClient is not allowed to utilize scope testScope, or testScope is not a valid scope. Verify the allowed scopes for unitTestClient and try again.";
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAuthenticationHeader("testScope"), expectedMessage);
        }

        [Test]
        public void GetAuthenticationHeader_ThrowsUnauthorizedForNon400Response()
        {
            HttpTest.RespondWith("garbage", 500);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", "secret", cli);

            var expectedMessage = "Unable to acquire OAuth token";
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAuthenticationHeader("testScope"), expectedMessage);
        }
    }
}
