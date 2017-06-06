using System;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
    public class HttpStatusRangeParserTests
    {
		[TestCase("4**", 399, ExpectedResult = false)]
		[TestCase("4**", 400, ExpectedResult = true)]
		[TestCase("4**", 499, ExpectedResult = true)]
		[TestCase("4**", 500, ExpectedResult = false)]

		[TestCase("4xx", 399, ExpectedResult = false)]
		[TestCase("4xx", 400, ExpectedResult = true)]
		[TestCase("4xx", 499, ExpectedResult = true)]
		[TestCase("4xx", 500, ExpectedResult = false)]

		[TestCase("4XX", 399, ExpectedResult = false)]
		[TestCase("4XX", 400, ExpectedResult = true)]
		[TestCase("4XX", 499, ExpectedResult = true)]
		[TestCase("4XX", 500, ExpectedResult = false)]

		[TestCase("400-499", 399, ExpectedResult = false)]
		[TestCase("400-499", 400, ExpectedResult = true)]
		[TestCase("400-499", 499, ExpectedResult = true)]
		[TestCase("400-499", 500, ExpectedResult = false)]

		[TestCase("100,3xx,600", 100, ExpectedResult = true)]
		[TestCase("100,3xx,600", 101, ExpectedResult = false)]
		[TestCase("100,3xx,600", 300, ExpectedResult = true)]
		[TestCase("100,3xx,600", 399, ExpectedResult = true)]
		[TestCase("100,3xx,600", 400, ExpectedResult = false)]
		[TestCase("100,3xx,600", 600, ExpectedResult = true)]

		[TestCase("400-409,490-499", 399, ExpectedResult = false)]
		[TestCase("400-409,490-499", 405, ExpectedResult = true)]
		[TestCase("400-409,490-499", 450, ExpectedResult = false)]
		[TestCase("400-409,490-499", 495, ExpectedResult = true)]
		[TestCase("400-409,490-499", 500, ExpectedResult = false)]

		[TestCase("*", 0, ExpectedResult = true)]
		[TestCase(",,,*", 9999, ExpectedResult = true)]

		[TestCase("", 0, ExpectedResult = false)]
		[TestCase(",,,", 9999, ExpectedResult = false)]
		public bool parser_works(string pattern, int value) {
			return HttpStatusRangeParser.IsMatch(pattern, value);
		}

		[TestCase("-100")]
		[TestCase("100-")]
		[TestCase("1yy")]
		public void parser_throws_on_invalid_pattern(string pattern) {
            Assert.Throws<ArgumentException>(() => HttpStatusRangeParser.IsMatch(pattern, 100));
		}
    }
}
