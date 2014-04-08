using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class DownloadTests
	{
		[Test]
		public async Task can_download_file() {
			var path = await "http://www.google.com".DownloadFileAsync(@"c:\a\b", "google.txt");
			Assert.That(File.Exists(path));
			File.Delete(path);
			Directory.Delete(@"c:\a", true);
		}
	}
}
