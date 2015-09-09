using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
    public static class HttpRequestMessageExtensions
    {
	    public static void SetFlurlSettings(this HttpRequestMessage request, FlurlHttpSettings settings) {
			request.Properties["FlurlSettings"] = settings;
	    }

		public static FlurlHttpSettings GetFlurlSettings(this HttpRequestMessage request) {
			object settings;
			return request.Properties.TryGetValue("FlurlSettings", out settings) ? (FlurlHttpSettings)settings : FlurlHttp.GlobalSettings;
		}
	}
}
