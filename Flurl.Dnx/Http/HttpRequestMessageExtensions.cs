using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
    public static class HttpRequestMessageExtensions
    {
	    private const string SETTINGS_KEY = "FlurlSettings";

	    internal static void SetFlurlSettings(this HttpRequestMessage request, FlurlHttpSettings settings) {
			request.Properties[SETTINGS_KEY] = settings;
	    }

		/// <summary>
		/// Gets the FlurlSettings object associated with this HttpRequestMessage.
		/// </summary>
		public static FlurlHttpSettings GetFlurlSettings(this HttpRequestMessage request) {
			object settings;
			return request.Properties.TryGetValue(SETTINGS_KEY, out settings) ? (FlurlHttpSettings)settings : FlurlHttp.GlobalSettings;
		}
	}
}
