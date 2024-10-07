using Flurl.Http.Authentication;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Flurl.Test.Http.Authentication
{
    [TestFixture]
    public class OAuthTokenProviderTests
    {
        internal class UnitTestTokenProvider : OAuthTokenProvider
        {
            private int _generationCount = 0;
            public UnitTestTokenProvider() : base()
            {
            }

            protected override Task<ExpirableToken> GetToken(string scope)
            {
                return Task.FromResult(new ExpirableToken((++_generationCount).ToString(), DateTimeOffset.Now.AddSeconds(1)));
            }
        }

        [Test]
        public async Task GetAuthenticationHeader_ReusesValidTokens()
        {
            var provider = new UnitTestTokenProvider();

            var header1 = await provider.GetAuthenticationHeader("scope");
            var header2 = await provider.GetAuthenticationHeader("scope");

            await Task.Delay(TimeSpan.FromSeconds(1));

            var header3 = await provider.GetAuthenticationHeader("scope");

            Assert.AreEqual(header1.Parameter, header2.Parameter);
            Assert.AreNotEqual(header1.Parameter, header3.Parameter);
        }
    }
}
