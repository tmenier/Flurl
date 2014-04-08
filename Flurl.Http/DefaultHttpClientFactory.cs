using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Flurl.Http
{
	/// <summary>
	/// Default implementation of IHttpClientFactory used by FlurlHttp. The created HttpClient includes hooks
	/// that enable FlurlHttp's testing features and respect its configuration settings. Therefore, custom factories
	/// should inherit from this class (rather than implementing IHttpClientFactory directly) and ensure base.CreateClient
	/// is called before providing enhancements.
	/// </summary>
	public class DefaultHttpClientFactory : IHttpClientFactory
	{
		public virtual HttpClient CreateClient(Url url) {
			return new HttpClient(new FlurlMessageHandler()) {
				Timeout = FlurlHttp.DefaultTimeout
			};
		}
	}
}
