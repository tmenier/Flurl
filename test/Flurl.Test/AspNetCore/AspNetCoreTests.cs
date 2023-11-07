#if NET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Flurl.Test.AspNetCore
{
	[TestFixture]
	public class AspNetCoreTests
	{
		[Test]
		public async Task try_this() {
			var application = new WebApplicationFactory<Program>();
			var client = new FlurlClient(application.CreateClient());
			var result = await client.Request("test").GetStringAsync();
			Assert.AreEqual("I'm from the middleware's dependency!", result);
		}
	}
}

#endif