using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// A set of properties that affect Flurl.Http behavior specific to auto-redirecting.
	/// </summary>
	public class RedirectSettings
	{
		private readonly FlurlHttpSettings _settings;

		/// <summary>
		/// Creates a new instance of RedirectSettings.
		/// </summary>
		public RedirectSettings(FlurlHttpSettings settings) {
			_settings = settings;
		}

		/// <summary>
		/// If false, all of Flurl's mechanisms for handling redirects, including raising the OnRedirect event,
		/// are disabled entirely. This could also impact cookie functionality. Default is true. If you don't
		/// need Flurl's redirect or cookie functionality, or you are providing an HttpClient whose HttpClientHandler
		/// is providing these services, then it is safe to set this to false.
		/// </summary>
		public bool Enabled {
			get => _settings.Get<bool>("Redirects_Enabled");
			set => _settings.Set(value, "Redirects_Enabled");
		}

		/// <summary>
		/// If true, redirecting from HTTPS to HTTP is allowed. Default is false, as this behavior is considered
		/// insecure.
		/// </summary>
		public bool AllowSecureToInsecure {
			get => _settings.Get<bool>("Redirects_AllowSecureToInsecure");
			set => _settings.Set(value, "Redirects_AllowSecureToInsecure");
		}

		/// <summary>
		/// If true, request-level headers sent in the original request are forwarded in the redirect, with the
		/// exception of Authorization and Cookie, which are configured independently via ForwardAuthorizationHeader
		/// (defaults is false) and ForwardCookies (default is true) respectively. Also, any headers set on
		/// FlurlClient are automatically sent with all requests, including redirects. Default is true.
		/// </summary>
		public bool ForwardHeaders {
			get => _settings.Get<bool>("Redirects_ForwardHeaders");
			set => _settings.Set(value, "Redirects_ForwardHeaders");
		}

		/// <summary>
		/// If true, any Authorization header sent in the original request is forwarded in the redirect.
		/// Default is false, as this behavior is considered insecure.
		/// </summary>
		public bool ForwardAuthorizationHeader {
			get => _settings.Get<bool>("Redirects_ForwardAuthorizationHeader");
			set => _settings.Set(value, "Redirects_ForwardAuthorizationHeader");
		}

		/// <summary>
		/// If true, any Cookie header sent in the original request is forwarded in the redirect.
		/// Default is true.
		/// </summary>
		public bool ForwardCookies {
			get => _settings.Get<bool>("Redirects_ForwardCookies");
			set => _settings.Set(value, "Redirects_ForwardCookies");
		}

		/// <summary>
		/// Maximum number of redirects that Flurl will automatically follow in a single request. Default is 10.
		/// </summary>
		public int MaxAutoRedirects {
			get => _settings.Get<int>("Redirects_MaxRedirects");
			set => _settings.Set(value, "Redirects_MaxRedirects");
		}
	}
}
