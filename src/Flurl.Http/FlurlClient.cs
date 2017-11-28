using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Interface defining FlurlClient's contract (useful for mocking and DI)
	/// </summary>
	public interface IFlurlClient : IHttpSettingsContainer, IDisposable {
		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this client.
		/// </summary>
		new ClientFlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets the HttpClient to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory. Reused for the life of the FlurlClient.
		/// </summary>
		HttpClient HttpClient { get; }

		/// <summary>
		/// Gets the HttpMessageHandler to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory.
		/// </summary>
		HttpMessageHandler HttpMessageHandler { get; }

		/// <summary>
		/// Gets or sets base URL associated with this client.
		/// </summary>
		string BaseUrl { get; set; }

		/// <summary>
		/// Instantiates a new IFlurClient, optionally appending path segments to the BaseUrl.
		/// </summary>
		/// <param name="urlSegments">The URL or URL segments for the request. If BaseUrl is defined, it is assumed that these are path segments off that base.</param>
		/// <returns>A new IFlurlRequest</returns>
		IFlurlRequest Request(params object[] urlSegments);

		/// <summary>
		/// Checks whether the connection lease timeout (as specified in Settings.ConnectionLeaseTimeout) has passed since
		/// connection was opened. If it has, resets the interval and returns true. 
		/// </summary>
		bool CheckAndRenewConnectionLease();

		/// <summary>
		/// Gets a value indicating whether this instance (and its underlying HttpClient) has been disposed.
		/// </summary>
		bool IsDisposed { get; }
	}

	/// <summary>
	/// A reusable object for making HTTP calls.
	/// </summary>
	public class FlurlClient : IFlurlClient
	{
		private ClientFlurlHttpSettings _settings;
		private readonly Lazy<HttpClient> _httpClient;
		private readonly Lazy<HttpMessageHandler> _httpMessageHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="baseUrl">The base URL associated with this client.</param>
		public FlurlClient(string baseUrl = null) {
			BaseUrl = baseUrl;
			_httpClient = new Lazy<HttpClient>(() => Settings.HttpClientFactory.CreateHttpClient(HttpMessageHandler));
			_httpMessageHandler = new Lazy<HttpMessageHandler>(() => Settings.HttpClientFactory.CreateMessageHandler());
		}

		/// <inheritdoc />
		public string BaseUrl { get; set; }

		/// <inheritdoc />
		public ClientFlurlHttpSettings Settings {
			get => _settings ?? (_settings = new ClientFlurlHttpSettings());
			set => _settings = value;
		}

		/// <inheritdoc />
		public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		/// <inheritdoc />
		public IDictionary<string, Cookie> Cookies { get; } = new Dictionary<string, Cookie>();

		/// <inheritdoc />
		public HttpClient HttpClient => HttpTest.Current?.HttpClient ?? _httpClient.Value;

		/// <inheritdoc />
		public HttpMessageHandler HttpMessageHandler => HttpTest.Current?.HttpMessageHandler ?? _httpMessageHandler.Value;

		/// <inheritdoc />
		public IFlurlRequest Request(params object[] urlSegments) {
			var parts = new List<string>(urlSegments.Select(s => s.ToInvariantString()));
			if (!Url.IsValid(parts.FirstOrDefault()) && !string.IsNullOrEmpty(BaseUrl))
				parts.Insert(0, BaseUrl);

			if (!parts.Any())
				throw new ArgumentException("Cannot create a Request. BaseUrl is not defined and no segments were passed.");
			if (!Url.IsValid(parts[0]))
				throw new ArgumentException("Cannot create a Request. Neither BaseUrl nor the first segment passed is a valid URL.");

			return new FlurlRequest(Url.Combine(parts.ToArray())).WithClient(this);
		}

		FlurlHttpSettings IHttpSettingsContainer.Settings {
			get => Settings;
			set => Settings = value as ClientFlurlHttpSettings;
		}

		private Lazy<DateTime> _connectionLeaseStart = new Lazy<DateTime>(() => DateTime.UtcNow);
		private readonly object _connectionLeaseLock = new object();

		private bool IsConnectionLeaseExpired =>
			Settings.ConnectionLeaseTimeout.HasValue &&
			DateTime.UtcNow - _connectionLeaseStart.Value > Settings.ConnectionLeaseTimeout;

		/// <inheritdoc />
		public bool CheckAndRenewConnectionLease() {
			// do double-check locking to avoid lock overhead most of the time
			if (IsConnectionLeaseExpired) {
				lock (_connectionLeaseLock) {
					if (IsConnectionLeaseExpired) {
						_connectionLeaseStart = new Lazy<DateTime>(() => DateTime.UtcNow);
						return true;
					}
				}
			}
			return false;
		}

		/// <inheritdoc />
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Disposes the underlying HttpClient and HttpMessageHandler.
		/// </summary>
		public virtual void Dispose() {
			if (IsDisposed)
				return;

			if (_httpMessageHandler.IsValueCreated)
				_httpMessageHandler.Value.Dispose();
			if (_httpClient.IsValueCreated)
				_httpClient.Value.Dispose();

			IsDisposed = true;
		}
	}
}