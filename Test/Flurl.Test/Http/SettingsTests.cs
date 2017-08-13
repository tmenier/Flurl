using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class SettingsTests
	{
		[Test, NonParallelizable] // tests that mess with global settings shouldn't be parallelized
		public void settings_propagate_correctly() {
			try {
				FlurlHttp.GlobalSettings.CookiesEnabled = false;
				FlurlHttp.GlobalSettings.AllowedHttpStatusRange = "4xx";

				var fc1 = new FlurlClient();
				fc1.Settings.CookiesEnabled = true;
				Assert.AreEqual("4xx", fc1.Settings.AllowedHttpStatusRange);
				fc1.Settings.AllowedHttpStatusRange = "5xx";

				var req = fc1.WithUrl("http://myapi.com");
				Assert.IsTrue(req.Settings.CookiesEnabled, "request should inherit client settings when not set at request level");
				Assert.AreEqual("5xx", req.Settings.AllowedHttpStatusRange, "request should inherit client settings when not set at request level");

				var fc2 = new FlurlClient();
				fc2.Settings.CookiesEnabled = false;

				req.WithClient(fc2);
				Assert.IsFalse(req.Settings.CookiesEnabled, "request should inherit client settings when not set at request level");
				Assert.AreEqual("4xx", req.Settings.AllowedHttpStatusRange, "request should inherit global settings when not set at request or client level");

				fc2.Settings.CookiesEnabled = true;
				fc2.Settings.AllowedHttpStatusRange = "3xx";
				Assert.IsTrue(req.Settings.CookiesEnabled, "request should inherit client sttings when not set at request level");
				Assert.AreEqual("3xx", req.Settings.AllowedHttpStatusRange, "request should inherit client sttings when not set at request level");

				req.Settings.CookiesEnabled = false;
				req.Settings.AllowedHttpStatusRange = "6xx";
				Assert.IsFalse(req.Settings.CookiesEnabled, "request-level settings should override any defaults");
				Assert.AreEqual("6xx", req.Settings.AllowedHttpStatusRange, "request-level settings should override any defaults");

				req.Settings.ResetDefaults();
				Assert.IsTrue(req.Settings.CookiesEnabled, "request should inherit client sttings when cleared at request level");
				Assert.AreEqual("3xx", req.Settings.AllowedHttpStatusRange, "request should inherit client sttings when cleared request level");

				fc2.Settings.ResetDefaults();
				Assert.IsFalse(req.Settings.CookiesEnabled, "request should inherit global settings when cleared at request and client level");
				Assert.AreEqual("4xx", req.Settings.AllowedHttpStatusRange, "request should inherit global settings when cleared at request and client level");
			}
			finally {
				FlurlHttp.GlobalSettings.ResetDefaults();
			}
		}
	}
}
