using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class HeadTests
	{
		[Test]
		public async Task can_call_head_for_flurlclient()
		{
			var test = new HttpTest();

			await "http://www.api.com".HeadAsync();

			Assert.AreEqual(1, test.CallLog.Count);
			Assert.AreEqual(HttpMethod.Head, test.CallLog.Single().Request.Method);
		}

		[Test]
		public async Task can_call_head_for_string_url()
		{
			var test = new HttpTest();

			await "http://www.api.com".HeadAsync();

			Assert.AreEqual(1, test.CallLog.Count);
			Assert.AreEqual(HttpMethod.Head, test.CallLog.Single().Request.Method);
		}

		[Test]
		public async Task can_call_head_for_url()
		{
			var test = new HttpTest();

			await new Url("http://www.api.com").HeadAsync();

			Assert.AreEqual(1, test.CallLog.Count);
			Assert.AreEqual(HttpMethod.Head, test.CallLog.Single().Request.Method);
		}
	}
}
