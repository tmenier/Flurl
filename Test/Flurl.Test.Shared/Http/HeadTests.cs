using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class HeadTests : HttpTestFixtureBase
	{
		[Test]
		public async Task can_call_head_for_flurlclient() {
			await "http://www.api.com".HeadAsync();

			Assert.AreEqual(1, HttpTest.CallLog.Count);
			Assert.AreEqual(HttpMethod.Head, HttpTest.CallLog.Single().Request.Method);
		}

		[Test]
		public async Task can_call_head_for_string_url() {
			await "http://www.api.com".HeadAsync();

			Assert.AreEqual(1, HttpTest.CallLog.Count);
			Assert.AreEqual(HttpMethod.Head, HttpTest.CallLog.Single().Request.Method);
		}

		[Test]
		public async Task can_call_head_for_url() {
			await new Url("http://www.api.com").HeadAsync();

			Assert.AreEqual(1, HttpTest.CallLog.Count);
			Assert.AreEqual(HttpMethod.Head, HttpTest.CallLog.Single().Request.Method);
		}
	}
}
