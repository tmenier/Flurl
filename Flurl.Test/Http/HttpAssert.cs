using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	public static class HttpAssert
	{
		public static void LastRequest(HttpMethod method, string mediaType, string body) {
			var call = FlurlHttp.Testing.CallLog.Last();
			Assert.AreEqual(method, call.Request.Method);
			Assert.AreEqual(mediaType, call.Request.Content == null ? null : call.Request.Content.Headers.ContentType.MediaType);
			Assert.AreEqual(body, call.RequestBody);
		}
	}
}
