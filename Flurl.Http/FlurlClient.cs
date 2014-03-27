using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// A simple container for a Url and an HttpClient, used for fluent chaining.
	/// </summary>
	public class FlurlClient
	{
		public FlurlClient(string url) : this(new Url(url)) { }

		public FlurlClient(Url url) {
			this.Url = url;
		}

		private HttpClient _httpClient;

		public Url Url { get; private set; }

		public HttpClient HttpClient {
			get {
				if (_httpClient == null) {
					var handler = new FlurlMesageHandler();
					_httpClient = new HttpClient(handler) {
						Timeout = FlurlHttp.DefaultTimeout
					};
				}
				return _httpClient;
			}
			set {
				if (!FlurlHttp.TestMode)
					_httpClient = value;
			}
		}
	}
}
