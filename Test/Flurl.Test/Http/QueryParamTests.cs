using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using NUnit.Framework;

namespace Flurl.Test.Http
{
    public class QueryParamTests : HttpTestFixtureBase
    {
        [Test]
        public void WithQueryParamValues_Has_Same_Default_Value_Handling_As_SetQueryParams()
        {
            var sut = new TestService(new PerBaseUrlFlurlClientFactory());
            var query = new Query
            {
                FirstName = "John"
            };
            var actual = sut.MakeGetCall(query);

            HttpTest.ShouldHaveCalled("http://www.example.com/api/some/path")
                .WithQueryParamValues(query);
        }
    }

    internal class TestService
    {
        private readonly IFlurlClientFactory _factory;

        public TestService(IFlurlClientFactory factory)
        {
            _factory = factory;
        }

        public Task<HttpResponseMessage> MakeGetCall(Query query)
        {
            return _factory.Get("http://www.example.com")
                .Request("api/some/path")
                .SetQueryParams(query)
                .GetAsync();
        }
    }

    internal class Query
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}