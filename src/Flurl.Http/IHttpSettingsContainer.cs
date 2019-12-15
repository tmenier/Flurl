using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Flurl.Http.Configuration;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Defines stateful aspects (headers, cookies, etc) common to both IFlurlClient and IFlurlRequest
	/// </summary>
	public interface IHttpSettingsContainer
	{
	    /// <summary>
	    /// Gets or sets the FlurlHttpSettings object used by this client or request.
	    /// </summary>
	    FlurlHttpSettings Settings { get; set; }

	    /// <summary>
	    /// Collection of headers sent on this request or all requests using this client.
	    /// </summary>
	    IDictionary<string, object> Headers { get; }

	    /// <summary>
	    /// Collection of HttpCookies sent and received with all requests using this client.
	    /// </summary>
	    IDictionary<string, Cookie> Cookies { get; }
    }
}
