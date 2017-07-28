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
			var fcExts = ReflectionHelper.GetAllExtensionMethods<FlurlClient>(typeof(FlurlClient).GetTypeInfo().Assembly);
			var urlExts = ReflectionHelper.GetAllExtensionMethods<Url>(typeof(FlurlClient).GetTypeInfo().Assembly);
			var stringExts = ReflectionHelper.GetAllExtensionMethods<string>(typeof(FlurlClient).GetTypeInfo().Assembly);
			var whitelist = new[] { "WithUrl" }; // cases where Url method of the same name was excluded intentionally

			foreach (var method in fcExts) {
				if (whitelist.Contains(method.Name))
					continue;

				if (!urlExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent Url extension method found for FlurlClient.{method.Name}({args})");
				}
				if (!stringExts.Any(m => ReflectionHelper.AreSameMethodSignatures(method, m))) {
					var args = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
					Assert.Fail($"No equivalent string extension method found for FlurlClient.{method.Name}({args})");
				}
			}
		}

		[Test]
		public void clone_default_shares_settings() {
			var client = new FlurlClient();
			var clone = client.Clone();
			Assert.AreNotSame(client, clone);
			Assert.AreSame(client.Settings, clone.Settings);
		}

		[Test]
		public void clone_configure_copies_settings() {
			var client = new FlurlClient();
			var clone = client.Clone(settings => { });
			Assert.AreNotSame(client, clone);
			Assert.AreNotSame(client.Settings, clone.Settings);
		}
	}
}