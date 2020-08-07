using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Flurl.Http
{
	/// <summary>
	/// Corresponds to the possible values of the SameSite attribute of the Set-Cookie header.
	/// </summary>
	public enum SameSite
	{
		/// <summary>
		/// Indicates a browser should only send cookie for same-site requests.
		/// </summary>
		Strict,
		/// <summary>
		/// Indicates a browser should send cookie for cross-site requests only with top-level navigation. 
		/// </summary>
		Lax,
		/// <summary>
		/// Indicates a browser should send cookie for same-site and cross-site requests.
		/// </summary>
		None
	}

	/// <summary>
	/// Represents an HTTP cookie. Closely matches Set-Cookie response header.
	/// </summary>
	public class FlurlCookie
	{
		private string _value;
		private DateTimeOffset? _expires;
		private int? _maxAge;
		private string _domain;
		private string _path;
		private bool _secure;
		private bool _httpOnly;
		private SameSite? _sameSite;

		private bool _locked;

		/// <summary>
		/// Creates a new FlurlCookie.
		/// </summary>
		/// <param name="name">Name of the cookie.</param>
		/// <param name="value">Value of the cookie.</param>
		/// <param name="originUrl">URL of request that sent the original Set-Cookie header.</param>
		/// <param name="dateReceived">Date/time that original Set-Cookie header was received. Defaults to current date/time. Important for Max-Age to be enforced correctly.</param>
		public FlurlCookie(string name, string value, string originUrl = null, DateTimeOffset? dateReceived = null) {
			Name = name;
			Value = value;
			OriginUrl = originUrl;
			DateReceived = dateReceived ?? DateTimeOffset.UtcNow;
		}

		/// <summary>
		/// The URL that originally sent the Set-Cookie response header. If adding to a CookieJar, this is required unless
		/// both Domain AND Path are specified.
		/// </summary>
		public Url OriginUrl { get; }

		/// <summary>
		/// Date and time the cookie was received. Defaults to date/time this FlurlCookie was created.
		/// Important for Max-Age to be enforced correctly.
		/// </summary>
		public DateTimeOffset DateReceived { get; }

		/// <summary>
		/// The cookie name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The cookie value.
		/// </summary>
		public string Value {
			get => _value;
			set => Update(ref _value, value);
		}

		/// <summary>
		/// Corresponds to the Expires attribute of the Set-Cookie header.
		/// </summary>
		public DateTimeOffset? Expires {
			get => _expires;
			set => Update(ref _expires, value);
		}

		/// <summary>
		/// Corresponds to the Max-Age attribute of the Set-Cookie header.
		/// </summary>
		public int? MaxAge {
			get => _maxAge;
			set => Update(ref _maxAge, value);
		}

		/// <summary>
		/// Corresponds to the Domain attribute of the Set-Cookie header.
		/// </summary>
		public string Domain {
			get => _domain;
			set => Update(ref _domain, value);
		}

		/// <summary>
		/// Corresponds to the Path attribute of the Set-Cookie header.
		/// </summary>
		public string Path {
			get => _path;
			set => Update(ref _path, value);
		}

		/// <summary>
		/// Corresponds to the Secure attribute of the Set-Cookie header.
		/// </summary>
		public bool Secure {
			get => _secure;
			set => Update(ref _secure, value);
		}

		/// <summary>
		/// Corresponds to the HttpOnly attribute of the Set-Cookie header.
		/// </summary>
		public bool HttpOnly {
			get => _httpOnly;
			set => Update(ref _httpOnly, value);
		}

		/// <summary>
		/// Corresponds to the SameSite attribute of the Set-Cookie header.
		/// </summary>
		public SameSite? SameSite {
			get => _sameSite;
			set => Update(ref _sameSite, value);
		}

		/// <summary>
		/// Generates a key based on cookie Name, Domain, and Path (using OriginalUrl in the absence of Domain/Path).
		/// Used by CookieJar to determine whether to add a cookie or update an existing one.
		/// </summary>
		public string GetKey() {
			var domain = string.IsNullOrEmpty(Domain) ? "*" + OriginUrl.Host : Domain;
			var path = string.IsNullOrEmpty(Path) ? OriginUrl.Path : Path;
			if (path.Length == 0) path = "/";
			return $"{domain}{path}:{Name.ToLowerInvariant()}";
		}

		/// <summary>
		/// Makes this cookie immutable. Call when added to a jar.
		/// </summary>
		internal void Lock() {
			_locked = true;
		}

		private void Update<T>(ref T field, T newVal, [CallerMemberName]string propName = null) {
			// == throws with generics (strangely), and .Equals needs a null check. Jon Skeet to the rescue.
			// https://stackoverflow.com/a/390974/62600
			if (EqualityComparer<T>.Default.Equals(field, newVal))
				return;

			if (_locked)
				throw new Exception("After a cookie has been added to a CookieJar, it becomes immutable and cannot be changed.");

			field = newVal;
		}
	}
}