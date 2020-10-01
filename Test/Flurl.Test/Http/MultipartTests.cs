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
				.AddString("string2", "bar", "text/blah")
				.AddStringParts(new { part1 = 1, part2 = 2, part3 = (string)null }) // part3 should be excluded
				.AddFile("file1", Path.Combine("path", "to", "image1.jpg"), "image/jpeg")
				.AddFile("file2", Path.Combine("path", "to", "image2.jpg"), "image/jpeg", fileName: "new-name.jpg")
				.AddJson("json", new { foo = "bar" })
				.AddUrlEncoded("urlEnc", new { fizz = "buzz" });

			Assert.AreEqual(8, content.Parts.Length);
			AssertStringPart<CapturedStringContent>(content.Parts[0], "string", "foo", null);
			AssertStringPart<CapturedStringContent>(content.Parts[1], "string2", "bar", "text/blah");
			AssertStringPart<CapturedStringContent>(content.Parts[2], "part1", "1", null);
			AssertStringPart<CapturedStringContent>(content.Parts[3], "part2", "2", null);
			AssertFilePart(content.Parts[4], "file1", "image1.jpg", "image/jpeg");
			AssertFilePart(content.Parts[5], "file2", "new-name.jpg", "image/jpeg");
			AssertStringPart<CapturedJsonContent>(content.Parts[6], "json", "{\"foo\":\"bar\"}", "application/json; charset=UTF-8");
			AssertStringPart<CapturedUrlEncodedContent>(content.Parts[7], "urlEnc", "fizz=buzz", "application/x-www-form-urlencoded");
		}

		private void AssertStringPart<TContent>(HttpContent part, string name, string content, string contentType) {
			Assert.IsInstanceOf<TContent>(part);
		    Assert.AreEqual(name, part.Headers.ContentDisposition.Name);
		    Assert.AreEqual(content, (part as CapturedStringContent)?.Content);
		    if (contentType == null)
			    Assert.IsFalse(part.Headers.Contains("Content-Type")); // #392
		    else
			    Assert.AreEqual(contentType, part.Headers.GetValues("Content-Type").SingleOrDefault());
		}

	    private void AssertFilePart(HttpContent part, string name, string fileName, string contentType) {
		    Assert.IsInstanceOf<FileContent>(part);
			Assert.AreEqual(name, part.Headers.ContentDisposition.Name);
			Assert.AreEqual(fileName, part.Headers.ContentDisposition.FileName);
			Assert.AreEqual(contentType, part.Headers.ContentType?.MediaType);
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
