using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Flurl.Test
{
    public class DelegatingHandlerHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var handler = base.CreateMessageHandler();

            return new PassThroughDelegatingHandler(new PassThroughDelegatingHandler(handler));
        }
    }
}
