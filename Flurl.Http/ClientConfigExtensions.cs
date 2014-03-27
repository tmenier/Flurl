using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Flurl.Http
{
	public static class ClientConfigExtensions
	{
		public static FlurlClient WithTimeout(this FlurlClient client, TimeSpan timespan) {
			client.HttpClient.Timeout = timespan;
			return client;
		}

		public static FlurlClient WithTimeout(this string url, TimeSpan timespan) {
			return new FlurlClient(url).WithTimeout(timespan);
		}

		public static FlurlClient WithTimeout(this Url url, TimeSpan timespan) {
			return new FlurlClient(url).WithTimeout(timespan);
		}

		public static FlurlClient WithTimeout(this FlurlClient client, int seconds) {
			return client.WithTimeout(TimeSpan.FromSeconds(seconds));
		}

		public static FlurlClient WithTimeout(this string url, int seconds) {
			return new FlurlClient(url).WithTimeout(seconds);
		}

		public static FlurlClient WithTimeout(this Url url, int seconds) {
			return new FlurlClient(url).WithTimeout(seconds);
		}
	}
}
