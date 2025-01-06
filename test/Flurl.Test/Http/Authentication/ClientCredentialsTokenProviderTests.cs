using Flurl.Http;
using Flurl.Http.Authentication;
using Flurl.Http.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flurl.Test.Http.Authentication
{
	[TestFixture]
    public class ClientCredentialsTokenProviderTests : HttpTestFixtureBase
    {
        [TestCase("secret", "client_id=unitTestClient&client_secret=secret&grant_type=client_credentials&scope=unitTestScope")]
        [TestCase("", "client_id=unitTestClient&grant_type=client_credentials&scope=unitTestScope")]
        [TestCase(null, "client_id=unitTestClient&grant_type=client_credentials&scope=unitTestScope")]
        public async Task GetAuthenticationHeader_OnlyPassesClientSecretIfSet(string clientSecret, string expectedBody)
        {
            var body = new
            {
                access_token = "UnitTestAccessToken",
                expires_in = 3600
            };

            HttpTest.RespondWithJson(body);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", clientSecret, cli);

            var scopes = new HashSet<string>(new[] { "unitTestScope" });
            var authHeader = await provider.GetAuthenticationHeader(scopes);

            HttpTest.ShouldHaveCalled("https://flurl.dev/connect/token")
                    .WithVerb(HttpMethod.Post)
                    .WithContentType("application/x-www-form-urlencoded")
                    .WithRequestBody(expectedBody)
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

            var scopes = new HashSet<string>(new[] { "unitTestScope" });
            var authHeader = await provider.GetAuthenticationHeader(scopes);

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

            var scopes = new HashSet<string>(new[] { "unitTestScope" });
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAuthenticationHeader(scopes));
        }

        [Test]
        public void GetAuthenticationHeader_ThrowsUnauthorizedForGarbageResponse()
        {
            HttpTest.RespondWith("garbage", 400);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", "secret", cli);

            var scopes = new HashSet<string>(new[] { "unitTestScope" });
            var expectedMessage = $"Verify the allowed scopes for unitTestClient and try again.";
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAuthenticationHeader(scopes), expectedMessage);
        }

        [Test]
        public void GetAuthenticationHeader_ThrowsUnauthorizedForNon400Response()
        {
            HttpTest.RespondWith("garbage", 500);

            var cli = new FlurlClient("https://flurl.dev");

            var provider = new ClientCredentialsTokenProvider("unitTestClient", "secret", cli);

            var scopes = new HashSet<string>(new[] { "unitTestScope" });
            var expectedMessage = "Unable to acquire OAuth token";
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAuthenticationHeader(scopes), expectedMessage);
        }
    }
}
