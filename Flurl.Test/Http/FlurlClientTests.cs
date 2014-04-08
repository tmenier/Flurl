using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Flurl.Http;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class FlurlClientTests
	{
		[Test]
		// check that for every FlurlClient extension method, we have an equivalent Url and string extension
		public void extension_methods_consistently_supported() {
			var allExtMethods = ReflectionHelper.GetAllExtensionMethods(typeof(FlurlClient).Assembly);
			var fcExtensions = allExtMethods.Where(m => m.GetParameters()[0].ParameterType == typeof(FlurlClient)).ToArray();

			foreach (var method in fcExtensions) {
				foreach (var type in new[] { typeof(string), typeof(Url) }) {
					if (!allExtMethods.Except(fcExtensions).Any(m => ReflectionHelper.IsEquivalentExtensionMethod(method, m, type))) {
						Assert.Fail("No equivalent {0} extension method found for FlurlClient.{1}", type.Name, method.Name);
					}
				}
			}
		}
	}
}
