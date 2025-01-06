using Flurl.Http.Authentication;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flurl.Test.Http.Authentication
{
	internal class UnitTestTokenProvider : OAuthTokenProvider
    {
        private int _generationCount = 0;
        public UnitTestTokenProvider() : base()
        {
        }

        protected override Task<ExpirableToken> GetToken(ISet<string> scopes)
        {
            return Task.FromResult(new ExpirableToken((++_generationCount).ToString(), DateTimeOffset.Now.AddSeconds(1)));
        }
    }

    [TestFixture]
    public class OAuthTokenProviderTests
    {
        [Test]
        public async Task GetAuthenticationHeader_ReusesValidTokens()
        {
            var provider = new UnitTestTokenProvider();

            var scopes = new HashSet<string>(new[] { "scope1" });
            
            var header1 = await provider.GetAuthenticationHeader(scopes);
            var header2 = await provider.GetAuthenticationHeader(scopes);

            await Task.Delay(TimeSpan.FromSeconds(2));

            var header3 = await provider.GetAuthenticationHeader(scopes);

            Assert.AreEqual(header1.Parameter, header2.Parameter);
            Assert.AreNotEqual(header1.Parameter, header3.Parameter);
        }
    }
}
