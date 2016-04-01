using System.Linq;
using Flurl.Http;
using NUnit.Framework;
using System.Reflection;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class FlurlClientTests
	{
		[Test]
		// check that for every FlurlClient extension method, we have an equivalent Url and string extension
		public void extension_methods_consistently_supported() {
			var fcExts = ReflectionHelper.GetAllExtensionMethods<FlurlClient>(typeof(FlurlClient).GetTypeInfo().Assembly);
			var urlExts = ReflectionHelper.GetAllExtensionMethods<Url>(typeof(FlurlClient).GetTypeInfo().Assembly);
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(FlurlClient).GetTypeInfo().Assembly);
			var whitelist = new[] { "GetCookies", "WithUrl" }; // cases where Url method of the same name was excluded intentionally

			foreach (var method in fcExts) {
				if (whitelist.Contains(method.Name))
					continue;

				if (!urlExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					Assert.Fail("No equivalent URL extension method found for FlurlClient.{0}", method.Name);
				}
				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					Assert.Fail("No equivalent string extension method found for FlurlClient.{0}", method.Name);
				}
			}
		}
	}
}
