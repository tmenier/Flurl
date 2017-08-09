using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Test
{
    public class PassThroughDelegatingHandler : DelegatingHandler
    {
        public PassThroughDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }
    }
}
