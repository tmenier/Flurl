using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// An EventHandlers collection is available on IFlurlRequest, IFlurlClient, and IFlurlClientBuilder.
	/// This abstract class allows the same tests to be run against all 3.
	/// </summary>
	public abstract class EventHandlerTestsBase<T> where T : IEventHandlerContainer
	{
		protected abstract T CreateContainer();
		protected abstract IFlurlRequest GetRequest(T container);

		[Test]
		public async Task can_set_pre_callback() {
			var callbackCalled = false;
			using var test = new HttpTest();

			test.RespondWith("ok");
			var c = CreateContainer().BeforeCall(call => {
				Assert.Null(call.Response); // verifies that callback is running before HTTP call is made
				callbackCalled = true;
			});
			Assert.IsFalse(callbackCalled);
			await GetRequest(c).GetAsync();
			Assert.IsTrue(callbackCalled);
		}

		[Test]
		public async Task can_set_post_callback() {
			var callbackCalled = false;
			using var test = new HttpTest();

			test.RespondWith("ok");
			var c = CreateContainer().AfterCall(call => {
				Assert.NotNull(call.Response); // verifies that callback is running after HTTP call is made
				callbackCalled = true;
			});
			Assert.IsFalse(callbackCalled);
			await GetRequest(c).GetAsync();
			Assert.IsTrue(callbackCalled);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task can_set_error_callback(bool markExceptionHandled) {
			var callbackCalled = false;
			using var test = new HttpTest();

			test.RespondWith("server error", 500);
			var c = CreateContainer().OnError(call => {
				Assert.NotNull(call.Response); // verifies that callback is running after HTTP call is made
				callbackCalled = true;
				call.ExceptionHandled = markExceptionHandled;
			});
			Assert.IsFalse(callbackCalled);
			try {
				await GetRequest(c).GetAsync();
				Assert.IsTrue(callbackCalled, "OnError was never called");
				Assert.IsTrue(markExceptionHandled, "ExceptionHandled was marked false in callback, but exception was not propagated.");
			}
			catch (FlurlHttpException) {
				Assert.IsTrue(callbackCalled, "OnError was never called");
				Assert.IsFalse(markExceptionHandled, "ExceptionHandled was marked true in callback, but exception was propagated.");
			}
		}

		[Test]
		public async Task can_disable_exception_behavior() {
			using var test = new HttpTest();

			var c = CreateContainer().OnError(call => {
				call.ExceptionHandled = true;
			});
			test.RespondWith("server error", 500);
			try {
				var result = await GetRequest(c).GetAsync();
				Assert.AreEqual(500, result.StatusCode);
			}
			catch (FlurlHttpException) {
				Assert.Fail("Flurl should not have thrown exception.");
			}
		}
	}

	[TestFixture]
	public class RequestEventHandlersTests : EventHandlerTestsBase<IFlurlRequest>
	{
		protected override IFlurlRequest CreateContainer() => new FlurlRequest("http://api.com");
		protected override IFlurlRequest GetRequest(IFlurlRequest req) => req;
	}

	[TestFixture]
	public class ClientEventHandlersTests : EventHandlerTestsBase<IFlurlClient>
	{
		protected override IFlurlClient CreateContainer() => new FlurlClient();
		protected override IFlurlRequest GetRequest(IFlurlClient cli) => cli.Request("http://api.com");
	}

	[TestFixture]
	public class ClientBuilderEventHandlersTests : EventHandlerTestsBase<IFlurlClientBuilder>
	{
		protected override IFlurlClientBuilder CreateContainer() => new FlurlClientBuilder();
		protected override IFlurlRequest GetRequest(IFlurlClientBuilder builder) => builder.Build().Request("http://api.com");
	}
}