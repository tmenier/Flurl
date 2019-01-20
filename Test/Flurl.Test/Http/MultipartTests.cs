using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http.Content;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
    public class MultipartTests
    {
		[Test]
		public void can_build_multipart_content() {
			var content = new CapturedMultipartContent()
				.AddString("string", "foo")
				.AddStringParts(new { part1 = 1, part2 = 2, part3 = (string)null }) // part3 should be excluded
				.AddFile("file", Path.Combine("path", "to", "image.jpg"), "image/jpeg")
				.AddJson("json", new { foo = "bar" })
				.AddUrlEncoded("urlEnc", new { fizz = "buzz" });

			Assert.AreEqual(6, content.Parts.Length);

			AssertStringPart<CapturedStringContent>(content.Parts[0], "string", "foo");
			AssertStringPart<CapturedStringContent>(content.Parts[1], "part1", "1");
			AssertStringPart<CapturedStringContent>(content.Parts[2], "part2", "2");
			AssertStringPart<CapturedJsonContent>(content.Parts[4], "json", "{\"foo\":\"bar\"}");
			AssertStringPart<CapturedUrlEncodedContent>(content.Parts[5], "urlEnc", "fizz=buzz");

			Assert.AreEqual("file", content.Parts[3].Headers.ContentDisposition.Name);
			Assert.AreEqual("image.jpg", content.Parts[3].Headers.ContentDisposition.FileName);
			Assert.IsInstanceOf<FileContent>(content.Parts[3]);
			Assert.AreEqual("image/jpeg", content.Parts[3].Headers.ContentType?.MediaType);
		}

	    private void AssertStringPart<TContent>(HttpContent part, string name, string content) {
		    Assert.AreEqual(name, part.Headers.ContentDisposition.Name);
		    Assert.AreEqual(content, (part as CapturedStringContent)?.Content);
		    Assert.IsInstanceOf<TContent>(part);
		    Assert.IsFalse(part.Headers.Contains("Content-Type")); // #392
	    }

		[Test]
		public void must_provide_required_args_to_builder() {
			var content = new CapturedMultipartContent();
			Assert.Throws<ArgumentNullException>(() => content.AddStringParts(null));
			Assert.Throws<ArgumentNullException>(() => content.AddString("other", null));
			Assert.Throws<ArgumentException>(() => content.AddString(null, "hello!"));
			Assert.Throws<ArgumentException>(() => content.AddFile("  ", "path"));
		}
	}
}
