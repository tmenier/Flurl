using System;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// Types of events raised by Flurl over the course of a call that can be handled via event handlers.
	/// </summary>
	public enum FlurlEventType
	{
		/// <summary>
		/// Fired immediately before an HTTP request is sent.
		/// </summary>
		BeforeCall,

		/// <summary>
		/// Fired immediately after an HTTP response is received.
		/// </summary>
		AfterCall,

		/// <summary>
		/// Fired when an HTTP error response is received, just before AfterCall is fired. Error
		/// responses include any status in 4xx or 5xx range by default, configurable via AllowHttpStatus.
		/// You can inspect call.Exception for details, and optionally set call.ExceptionHandled to
		/// true to prevent the exception from bubbling up after the handler exits.
		/// </summary>
		OnError,

		/// <summary>
		/// Fired when any 3xx response with a Location header is received, just before AfterCall is fired
		/// and before the subsequent (redirected) request is sent. You can inspect/manipulate the
		/// call.Redirect object to determine what will happen next. An auto-redirect will only happen if
		/// call.Redirect.Follow is true upon exiting the callback.
		/// </summary>
		OnRedirect
	}

	/// <summary>
	/// Defines a handler for Flurl events such as BeforeCall, AfterCall, and OnError
	/// </summary>
	public interface IFlurlEventHandler
	{
		/// <summary>
		/// Action to take when a Flurl event fires. Prefer HandleAsync if async calls need to be made.
		/// </summary>
		void Handle(FlurlEventType eventType, FlurlCall call);

		/// <summary>
		/// Asynchronous action to take when a Flurl event fires.
		/// </summary>
		Task HandleAsync(FlurlEventType eventType, FlurlCall call);
	}

	/// <summary>
	/// Default implementation of IFlurlEventHandler. Typically, you should override Handle or HandleAsync.
	/// Both are noops by default.
	/// </summary>
	public class FlurlEventHandler : IFlurlEventHandler
	{
		/// <summary>
		/// Override to define an action to take when a Flurl event fires. Prefer HandleAsync if async calls need to be made.
		/// </summary>
		public virtual void Handle(FlurlEventType eventType, FlurlCall call) { }

		/// <summary>
		/// Override to define an asynchronous action to take when a Flurl event fires.
		/// </summary>
		public virtual Task HandleAsync(FlurlEventType eventType, FlurlCall call) => Task.CompletedTask;

		internal class FromAction : FlurlEventHandler
		{
			private readonly Action<FlurlCall> _act;
			public FromAction(Action<FlurlCall> act) => _act = act;
			public override void Handle(FlurlEventType eventType, FlurlCall call) => _act(call);
		}

		internal class FromAsyncAction : FlurlEventHandler
		{
			private readonly Func<FlurlCall, Task> _act;
			public FromAsyncAction(Func<FlurlCall, Task> act) => _act = act;
			public override Task HandleAsync(FlurlEventType eventType, FlurlCall call) => _act(call);
		}
	}
}
