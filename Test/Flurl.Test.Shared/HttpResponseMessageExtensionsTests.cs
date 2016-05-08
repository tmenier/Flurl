using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test
{
	[TestFixture]
	public class HttpResponseMessageExtensionsTests
	{
		[Test]
		public void StripCharsetQuotes__Given_Response__When_ContentTypeCharsetHasQuotes__Then_RemoveQuotes()
		{
			var response = new HttpResponseMessage
			{
				Content = new StringContent("Doesn't matter")
				{
					Headers =
					{
						ContentType =
						{
							MediaType = "text/javascript",
							CharSet = "\"UTF-8\""
						}
					}
				}
			};

			var result = response.StripCharsetQuotes();

			Assert.AreEqual("UTF-8", result.Content.Headers.ContentType.CharSet);
		}

		[Test]
		public async Task ReceiveString__Given_ResponseTask__When_ContentTypeCharsetNonQuoted__Then_GetContentString()
		{
			var expectedContent = Guid.NewGuid().ToString();

			var response = new HttpResponseMessage
			{
				Content = new StringContent(expectedContent)
				{
					Headers =
					{
						ContentType =
						{
							MediaType = "text/javascript",
							CharSet = "UTF-8"
						}
					}
				}
			};

			var subject = Task.FromResult(response);
			var result = await subject.ReceiveString();

			Assert.AreEqual(expectedContent, result);
		}

		[Test]
		public async Task ReceiveString__Given_ResponseTask__When_ContentTypeCharsetQuoted__Then_GetContentString()
		{
			var expectedContent = Guid.NewGuid().ToString();

			var response = new HttpResponseMessage
			{
				Content = new StringContent(expectedContent)
				{
					Headers =
					{
						ContentType =
						{
							MediaType = "text/javascript",
							CharSet = "\"UTF-8\""
						}
					}
				}
			};

			var subject = Task.FromResult(response);
			var result = await subject.ReceiveString();

			Assert.AreEqual(expectedContent, result);
		}
	}
}
