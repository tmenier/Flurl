using System;
using System.Linq;
using System.Reflection;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class FlurlClientTests
	{
		[Test]
		// check that for every FlurlClient extension method, we have an equivalent Url and string extension
		public void extension_methods_consistently_supported() {
			var frExts = ReflectionHelper.GetAllExtensionMethods<IFlurlRequest>(typeof(FlurlClient).GetTypeInfo().Assembly)
				// URL builder methods on IFlurlClient get a free pass. We're looking for things like HTTP calling methods.
				.Where(mi => mi.DeclaringType != typeof(UrlBuilderExtensions))
				.ToList();
			var urlExts = ReflectionHelper.GetAllExtensionMethods<Url>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(FlurlClient).GetTypeInfo().Assembly).ToList();

			Assert.That(frExts.Count > 20, $"IFlurlRequest only has {frExts.Count} extension methods? Something's wrong here.");

			// Url and string should contain all extension methods that IFlurlRequest has
			foreach (var method in frExts) {
				if (!urlExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent Url extension method found for IFlurlRequest.{method.Name}({args})");
				}
				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent string extension method found for IFlurlRequest.{method.Name}({args})");
				}
			}
		}
	}
}