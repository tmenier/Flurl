using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
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

			Assert.AreEqual("string", content.Parts[0].Headers.ContentDisposition.Name.Trim('"'));
			Assert.IsInstanceOf<CapturedStringContent>(content.Parts[0]);
			Assert.AreEqual("foo", (content.Parts[0] as CapturedStringContent).Content);

			Assert.AreEqual("part1", content.Parts[1].Headers.ContentDisposition.Name.Trim('"'));
			Assert.IsInstanceOf<CapturedStringContent>(content.Parts[1]);
			Assert.AreEqual("1", (content.Parts[1] as CapturedStringContent).Content);

			Assert.AreEqual("part2", content.Parts[2].Headers.ContentDisposition.Name.Trim('"'));
			Assert.IsInstanceOf<CapturedStringContent>(content.Parts[2]);
			Assert.AreEqual("2", (content.Parts[2] as CapturedStringContent).Content);

			Assert.AreEqual("file", content.Parts[3].Headers.ContentDisposition.Name.Trim('"'));
			Assert.AreEqual("image.jpg", content.Parts[3].Headers.ContentDisposition.FileName.Trim('"'));
			Assert.IsInstanceOf<FileContent>(content.Parts[3]);

			Assert.AreEqual("json", content.Parts[4].Headers.ContentDisposition.Name.Trim('"'));
			Assert.IsInstanceOf<CapturedJsonContent>(content.Parts[4]);
			Assert.AreEqual("{\"foo\":\"bar\"}", (content.Parts[4] as CapturedJsonContent).Content);

			Assert.AreEqual("urlEnc", content.Parts[5].Headers.ContentDisposition.Name.Trim('"'));
			Assert.IsInstanceOf<CapturedUrlEncodedContent>(content.Parts[5]);
			Assert.AreEqual("fizz=buzz", (content.Parts[5] as CapturedUrlEncodedContent).Content);
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
