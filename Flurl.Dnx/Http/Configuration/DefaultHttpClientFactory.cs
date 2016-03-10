﻿using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Default implementation of IHttpClientFactory used by FlurlHttp. The created HttpClient includes hooks
	/// that enable FlurlHttp's testing features and respect its configuration settings. Therefore, custom factories
	/// should inherit from this class, rather than implementing IHttpClientFactory directly.
	/// </summary>
	public class DefaultHttpClientFactory : IHttpClientFactory
	{
		/// <summary>
		/// Override in custom factory to customize the creation of HttpClient used in all Flurl HTTP calls.
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateClient and
		/// customize the result.
		/// </summary>
		public virtual HttpClient CreateClient(Url url, HttpMessageHandler handler) {
			return new HttpClient(new FlurlMessageHandler(handler));
		}

		/// <summary>
		/// Override in custom factory to customize the creation of HttpClientHandler used in all Flurl HTTP calls.
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateMessageHandler and
		/// customize the result.
		/// </summary>
		public virtual HttpMessageHandler CreateMessageHandler() {
			return new HttpClientHandler();
		}
	}
}
