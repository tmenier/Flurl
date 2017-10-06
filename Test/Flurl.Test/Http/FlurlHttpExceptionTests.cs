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
    class FlurlHttpExceptionTests : HttpTestFixtureBase
    {
	    [Test]
	    public async Task exception_message_is_nice() {
		    HttpTest.RespondWithJson(new { message = "bad data!" }, 400);

		    try {
			    await "http://myapi.com".PostJsonAsync(new { data = "bad" });
			    Assert.Fail("should have thrown 400.");
		    }
		    catch (FlurlHttpException ex) {
			    Assert.AreEqual("POST http://myapi.com failed with status code 400 (Bad Request).\r\nRequest body:\r\n{\"data\":\"bad\"}\r\nResponse body:\r\n{\"message\":\"bad data!\"}", ex.Message);
		    }
	    }

	    [Test]
	    public async Task exception_message_excludes_request_response_labels_when_body_empty() {
		    HttpTest.RespondWith("", 400);

		    try {
			    await "http://myapi.com".GetAsync();
			    Assert.Fail("should have thrown 400.");
		    }
		    catch (FlurlHttpException ex) {
				// no "Request body:", "Response body:", or line breaks
			    Assert.AreEqual("GET http://myapi.com failed with status code 400 (Bad Request).", ex.Message);
		    }
	    }
	}
}
