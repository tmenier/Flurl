---
layout: default
---

##Configuration

Flurl.Http supports a variety of global configuration options.

````c#
FlurlHttp.Configure(c => {
    c.DefaultTimeout = TimeSpan.FromSeconds(30);
    
    // Statuses (in addition to 2xx) that won't throw exceptions.
    // Set to "*" to allow eveything.
    c.AllowedHttpStatusRange = "100-150,4xx";

    // Global callbacks
    c.BeforeCall = DoSomethingBeforeEveryHttpCall;
    c.AfterCall = DoSomeLogging;
    c.OnError = LogToElmah;
    c.HttpClientFactory = new MyCustomFactory();
});
````

`FlurlHttp.Configure` is a static method that should only be called once (if at all) at application startup, such as in `Application_Start` in global.asax for ASP.NET applications.

Global callbacks are ideal for cross-cutting concerns such as logging. Need async callbacks? No problem: 

````c#
FlurlHttp.Configure(c => {
    c.BeforeCallAsync = DoSomethingBeforeCallAsync;
    c.AfterCallAsync = DoSomethingAfterCallAsync;
    c.OnErrorAsync = HandleErrorAsync;
});
````

The callbacks are all instances of `Action<HttpCall>`, or `Func<HttpCall, Task>` for the async versions.

Example (asynchonous) error callback:

````c#
public static async Task HandleErrorAsync(HttpCall call) {
    await LogErrorAsync(call.Exception, call.Request.Url.AbsoluteUri);
    call.ExceptionHandled = true; // don't propagate up the exception
}
````

`HttpCall` is a diagnostic object that wraps a variety of useful information:

````c#
public class HttpCall
{
    public HttpRequestMessage Request { get; set; }
    public string RequestBody { get; set; }
    public HttpResponseMessage Response { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime? EndedUtc { get; set; }
    public TimeSpan? Duration { get; set; }
    public Exception Exception { get; set; }
    public bool ExceptionHandled { get; set; }
    public string Url { get; }
    public bool Completed { get; }
    public bool Succeeded { get; }
    public HttpStatusCode? HttpStatus { get; }
}
````

`RequestBody` is particularly useful in POST and PUT scenarios to work around the [forward-only, read-once nature](http://stackoverflow.com/questions/12102879/httprequestmessage-content-is-lost-when-it-is-read-in-a-logging-delegatinghandle) of `HttpRequestMessage.Content`. It is important to do a null check on `RequestBody` however, as it is typically only populated when using Flurl's Post* and Put* methods.

The final global configuration option is `HttpClientFactory`, which allows you to change the way HttpClient instances are created. Normally you won't need to set this, and care should be taken when doing so. More details in the [Extensibility]({{ site.baseurl }}/extensibility) section.

###Reverting Config Changes

Although not very common, you can easily undo all customizations and revert back to the default configuration:

````c#
FlurlHttp.ResetDefaults();
````

