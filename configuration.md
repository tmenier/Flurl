---
layout: default
---

##Configuration

Flurl.Http supports a variety of global configuration options.

````c#
FlurlHttp.Configure(c => {
    c.DefaultTimeout = TimeSpan.FromSeconds(30);
    c.BeforeCall = DoSomethingBeforeEveryHttpCall;
    c.AfterCall = DoSomeLogging;
    c.OnError = LogToElmah;
    c.HttpClientFactory = new MyCustomFactory();
});
````

`FlurlHttp.Configure` is a static method that should only be called once (if at all) at application startup, such as in `Application_Start` in global.asax for ASP.NET applications.

Need async callbacks? No problem: 

````c#
FlurlHttp.Configure(c => {
    c.BeforeCallAsync = DoSomethingBeforeCallAsync;
    c.AfterCallAsync = DoSomethingAfterCallAsync;
    c.OnErrorAsync = HandleErrorAsync;
});
````

The 3 callbacks are instances of `Action<HttpCall>` (`Func<HttpCall, Task>` for the async versions).

Example (asynchonous) error callback:

````c#
public static async Task HandleErrorAsync(HttpCall call) {
    await LogErrorAsync(call.Exception, call.Request.Url.AbsoluteUri);
    call.ExceptionHandled = true; // don't propagate up the exception
}
````

`HttpCall` wraps a variety of useful objects as specified [here]({{ site.baseurl }}/fluent-http/#httpcall).

````c#
FlurlHttp.ResetDefaults();
````

The final global configuration option is `HttpClientFactory`, which allows you to change the way HttpClient instances are created. Normally you won't need to set this, and care should be taken when doing so. More details in the [Extensibility]({{ site.baseurl }}/extensibility) section.
