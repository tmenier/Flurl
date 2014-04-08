using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Flurl.Http
{
	public interface IHttpClientFactory
	{
		HttpClient CreateClient(Url url);
	}
}
