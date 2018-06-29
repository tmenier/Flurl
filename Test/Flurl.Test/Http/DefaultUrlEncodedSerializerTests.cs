using Flurl.Http.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Test.Http
{
    [TestFixture, Parallelizable]
    public class DefaultUrlEncodedSerializerTests
    {
	    [Test]
	    public void can_serialize_object() {
		    var vals = new {
			    a = "foo",
			    b = 333,
			    c = (string)null, // exlude
			    d = ""
		    };

		    var serialized = new DefaultUrlEncodedSerializer().Serialize(vals);
		    Assert.AreEqual("a=foo&b=333&d=", serialized);
	    }
	}
}
