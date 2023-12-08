using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// A common interface for Flurl.Http objects that contain event handlers.
	/// </summary>
	public interface IEventHandlerContainer
	{
		/// <summary>
		/// A collection of Flurl event handlers.
		/// </summary>
		IList<(FlurlEventType EventType, IFlurlEventHandler Handler)> EventHandlers { get; }
	}

	/// <summary>
	/// Fluent extension methods for tweaking FlurlHttpSettings
	/// </summary>
	public static class EventHandlerContainerExtensions
	{
		/// <summary>
		/// Adds an event handler that is invoked when a BeforeCall event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T BeforeCall<T>(this T obj, Action<FlurlCall> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.BeforeCall, act);

		/// <summary>
		/// Adds an asynchronous event handler that is invoked when a BeforeCall event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T BeforeCall<T>(this T obj, Func<FlurlCall, Task> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.BeforeCall, act);

		/// <summary>
		/// Adds an event handler that is invoked when an AfterCall event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T AfterCall<T>(this T obj, Action<FlurlCall> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.AfterCall, act);

		/// <summary>
		/// Adds an asynchronous event handler that is invoked when an AfterCall event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T AfterCall<T>(this T obj, Func<FlurlCall, Task> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.AfterCall, act);

		/// <summary>
		/// Adds an event handler that is invoked when an OnError event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T OnError<T>(this T obj, Action<FlurlCall> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.OnError, act);

		/// <summary>
		/// Adds an asynchronous event handler that is invoked when an OnError event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T OnError<T>(this T obj, Func<FlurlCall, Task> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.OnError, act);

		/// <summary>
		/// Adds an event handler that is invoked when an OnRedirect event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T OnRedirect<T>(this T obj, Action<FlurlCall> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.OnRedirect, act);

		/// <summary>
		/// Adds an asynchronous event handler that is invoked when an OnRedirect event is fired.
		/// </summary>
		/// <returns>This event handler container.</returns>
		public static T OnRedirect<T>(this T obj, Func<FlurlCall, Task> act) where T : IEventHandlerContainer => AddHandler(obj, FlurlEventType.OnRedirect, act);

		private static T AddHandler<T>(T obj, FlurlEventType eventType, Action<FlurlCall> act) where T : IEventHandlerContainer {
			obj.EventHandlers.Add((eventType, new FlurlEventHandler.FromAction(act)));
			return obj;
		}

		private static T AddHandler<T>(T obj, FlurlEventType eventType, Func<FlurlCall, Task> act) where T : IEventHandlerContainer {
			obj.EventHandlers.Add((eventType, new FlurlEventHandler.FromAsyncAction(act)));
			return obj;
		}
	}
}
