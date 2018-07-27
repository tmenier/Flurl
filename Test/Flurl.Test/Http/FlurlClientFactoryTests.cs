using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
    public class FlurlClientFactoryTests
    {
	    [Test]
	    public void per_host_factory_provides_same_client_per_host() {
		    var fac = new PerHostFlurlClientFactory();
		    var cli1 = fac.Get("http://api.com/foo");
		    var cli2 = fac.Get("https://api.com/bar");
		    Assert.AreSame(cli1, cli2);
	    }

	    [Test]
	    public void per_base_url_factory_provides_same_client_per_provided_url() {
		    var fac = new PerBaseUrlFlurlClientFactory();
		    var cli1 = fac.Get("http://api.com/foo");
		    var cli2 = fac.Get("http://api.com/bar");
		    var cli3 = fac.Get("http://api.com/foo");
		    Assert.AreNotSame(cli1, cli2);
		    Assert.AreSame(cli1, cli3);
	    }

	    [Test]
	    public void can_configure_client_from_factory() {
		    var fac = new PerHostFlurlClientFactory();
		    fac.ConfigureClient("http://api.com/foo", c => c.Settings.CookiesEnabled = true);
		    Assert.IsTrue(fac.Get("https://api.com/bar").Settings.CookiesEnabled);
		    Assert.IsFalse(fac.Get("http://api2.com/foo").Settings.CookiesEnabled);
	    }

	    [Test]
	    public void ConfigureClient_is_thread_safe() {
		    var fac = new PerHostFlurlClientFactory();

		    var x = "";

		    var t1 = Task.Run(() => fac.ConfigureClient("http://api.com", c => {
			    x = "in thread 1";
			    Thread.Sleep(200);
			    Assert.AreEqual("in thread 1", x);
			    x = "still in thread 1";
		    }));

		    Thread.Sleep(100);
		    var t2 = Task.Run(() => fac.ConfigureClient("http://api.com", c => {
			    Assert.AreEqual("still in thread 1", x);
			    x = "in thread 2";
		    }));

		    Task.WaitAll(t1, t2);
		    Assert.AreEqual("in thread 2", x);
	    }
	}
}
