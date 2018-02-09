﻿using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
    [TestFixture, Parallelizable]
    public class FlurlClientFactoryTests
    {
        private HttpTest httpTest;
        private readonly IFlurlClientFactory flurlClientFactory;
        private readonly FlurlClientFactoryTestService testService;

        public FlurlClientFactoryTests()
        {
            flurlClientFactory = new TestFlurlClientFactory();
            testService = new FlurlClientFactoryTestService(flurlClientFactory);
        }

        [SetUp]
        public void Setup()
        {
            httpTest = new HttpTest();
        }

        [TearDown]
        public void Cleanup()
        {
            httpTest.Dispose();
        }

        [Test]
        public async Task can_make_requests_using_flurlclientfactory()
        {
            var actual = await testService.MakeCall();

            httpTest.ShouldHaveCalled("http://fake-domain.com:8080/some/fake/path")
                .WithRequestJson(new
                {
                    Data = "some data"
                });
        }
    }

    internal class FlurlClientFactoryTestService
    {
        private readonly IFlurlClient _client;

        public FlurlClientFactoryTestService(IFlurlClientFactory clientFactory)
        {
            _client = clientFactory.Get("http://fake-domain.com:8080/")
                .WithHeader("Content-type", "application/json");
        }

        public Task<object> MakeCall()
        {
            return _client.Request("/some/fake/path")
                .PostJsonAsync(new
                {
                    Data = "some data"
                })
                .ReceiveJson<object>();
        }
    }
}
