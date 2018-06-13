---
layout: redirect
redirect_to: /docs/configuration/
---

## Configuration

Flurl.Http defines a `Settings` property at 4 levels, each overriding the previous level in this order:

- `FlurlHttp` (global)
- `IFlurlClient`
- `IFlurlRequest`
- `HttpTest`

Properties of `Settings` are mostly the same at all 4 levels, with a few exceptions. Here's a complete list along with where they are and are not supported:

Property                | FlurlHttp (global) | HttpTest | IFlurlClient | IFlurlRequest
------------------------|:------------------:|:--------:|:------------:|:-------------:
Timeout                 |         x          |    x     |      x       |       x         
AllowedHttpStatusRange  |         x          |    x     |      x       |       x         
CookiesEnabled          |         x          |    x     |      x       |       x         
JsonSerializer          |         x          |    x     |      x       |       x         
UrlEncodedSerializer    |         x          |    x     |      x       |       x         
BeforeCall              |         x          |    x     |      x       |       x         
BeforeCallAsync         |         x          |    x     |      x       |       x         
AfterCall               |         x          |    x     |      x       |       x         
AfterCallAsync          |         x          |    x     |      x       |       x         
OnError                 |         x          |    x     |      x       |       x         
OnErrorAsync            |         x          |    x     |      x       |       x         
ConnectionLeaseTimeout  |         x          |    x     |      x       |                 
HttpClientFactory       |         x          |    x     |      x       |                 
FlurlClientFactory      |         x          |    x     |              |                 

A couple important things to note about `Settings` behavior:

- The hierarchy is never actually "flattened"; properties are always aware of what level they inherited their value from, and will continue to reflect changes made at a higher level. For example, if you have a `FlurlRequest` that has inheritted the global `Timeout` setting, _then_ a `FlurlClient` is attached that has its own `Timeout` setting, `request.Settings.Timeout` will now reflect the client-level setting. But if `Timeout` were set _explicitly_ at the request level, that value will always stick.

- An _explicitly set_ value will always override an inherited default, including `null`. (Dictionaries are used internally, and key existence dictates whether a value is set at that level, not a null/default value check.)

### Configuring Settings

`Settings` properties are all read/write, but if you want to change mutiple settings at once (atomically), you should generally do so with one of the `Configure*` methods, which take an `Action<Settings>` lambda.

Configure global defaults:

```c#
// call once at application startup
FlurlHttp.Configure(settings => ...);
```

You can also configure the `FlurlClient` that will be used to call a given URL. It is important to note that the same `FlurlClient` is used for all calls to the same _host_ by default, so this will potentially affect more than just calls to the _specific_ URL provided:

```c#
// call once at application startup
FlurlHttp.ConfigureClient(url, cli => ...);
```

If you're managing `FlurlClient`s explicitly:

```c#
flurlClient.Configure(settings => ...);
```

Fluently configure a single request (via extension method on `string`, `Url`, or `IFlurlRequest`):

```c#
await url.ConfigureRequest(settings => ...).GetAsync();
```

Override any settings from within a [test]({{ site.baseurl }}/testable-http), regardless what level they're set at in the test subject:

```c#
httpTest.Configure(settings => ...);
```

Let's take a look at some specific settings.

### HttpClientFactory

For advanced scenarios, you can customize the way Flurl.Http constructs `HttpClient` and `HttpMessageHandler` instances. Although it is only required that your custom factory implements `Flurl.Http.Configuration.IHttpClientFactory`, it is recommended to inherit from `DefaultHttpClientFactory` and extend only as needed.

```c#
public class MyCustomHttpClientFactory : DefaultHttpClientFactory
{
    // override to customize how HttpClient is created/configured
    public override HttpClient CreateHttpClient(HttpMessageHandler handler);

    // override to customize how HttpMessageHandler is created/configured
    public override HttpMessageHandler CreateMessageHandler();
}
```

Register this globally:

```c#
FlurlHttp.Configure(settings => {
    settings.HttpClientFactory = new MyCustomHttpClientFactory();
});
```

Or (less common) on an individual `FlurlClient`:

```c#
var cli = new FlurlClient(BASE_URL).Configure(settings => {
    settings.HttpClientFactory = new MyCustomHttpClientFactory();
});
```

Note that custom HttpClient factories are _not_ the recommended place to control caching/reusing of created instances. `FlurlClientFactory` is better suited for this.

### FlurlClientFactory

`IFlurlClientFactory` defines one method, `Get(Url)`, which is responsible for providing the `IFlurlClient` instance that should be used to call the `Url`. The implemetation registered globally is `PerHostUrl` which, as discussed [here]({{ site.baseurl }}/client-lifetime), uses a single cached instance of `FlurlClient` per host being called for the lifetime of your application. You could define your own factory by implementing `IFlurlClientFactory` directly, but inheriting from `FlurlClientFactoryBase` is much easier. It allows you to define a caching _strategy_ by returning a cache key based on a Url, without having to implement the cache itself.

```c#
public abstract class FlurlClientFactoryBase : IFlurlClientFactory
{
    // override to customize how FlurlClient instances are cached/reused
    protected abstract string GetCacheKey(Url url);

    // override to customize how FlurlClient is created/configured (called only as needed)
    protected virtual IFlurlClient Create(Url url);
}
```

`FlurlClientFactory` can only be set at the global level or on an `HttpTest`. (`IFlurlClientFactory` is also [useful]({{ site.baseurl }}/client-lifetime) in conjuction with IoC containers.)

### Serializers

Both `JsonSerializer` and `UrlEncodedSerializer` implement `ISerializer`, a simple interface for serializing objects to and from strings.

```C#
public interface ISerializer
{
    string Serialize(object obj);
    T Deserialize<T>(string s);
    T Deserialize<T>(Stream stream);
}
```

Both have a default implementation registered globally, and replacing them is possible but not common. The default `JsonSerializer` implementation is `NewtonsoftJsonSerializer` that, as you probably guessed, uses the ever popular [Json.NET](https://www.newtonsoft.com/json) library. Although it's unlikely that you'd want to _replace_ this implementation, note that it's constructor takes a `Newtonsoft.Json.JsonSerializerSettings` argument, which is a nice hook for tapping into the many [custom serialization settings](https://www.newtonsoft.com/json/help/html/SerializationSettings.htm) that library offers:

```c#
FlurlHttp.Configure(settings => {
    var jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ObjectCreationHandling = ObjectCreationHandling.Replace
    };
    settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
});
```

### [Event Handlers](event-handlers)

Keeping cross-cutting concerns like logging and error handling separated from your normal logic flow often results in cleaner code. Flurl.Http provides an event model for these scenarios. `BeforeCall`, `AfterCall`, `OnError`, and their `Async*` equivalents are typically defined at the global or client level, but can be defined per request if it makes sense. These settings each take an `Action<HttpCall>` delegate (`Func<HttpCall, Task>` for the async versions). `HttpCall` provides rich details about the call that you can act upon:

```c#
public class HttpCall
{
    public IFlurlRequest FlurlRequest { get; }
    public HttpRequestMessage Request { get; }
    public string RequestBody { get; }
    public HttpResponseMessage Response { get; }
    public DateTime StartedUtc { get; }
    public DateTime? EndedUtc { get; }
    public TimeSpan? Duration { get; }
    public bool Completed { get; }
    public bool Succeeded { get; }
    public HttpStatusCode? HttpStatus { get; }
    public string ErrorResponseBody { get; }
    public Exception Exception { get; }
    public bool ExceptionHandled { get; set; }
}
```

`FlurlRequest` provides many Flurl.Http-specific objects like `Url`, `FlurlClient`, and `Settings` assocaited with the request. Many of the other properties are lower-level objects from the `System.Net.Http` world. Not surprisingly, response-related properties will be `null` in `BeforeCall`. `AfterCall` fires after both successful and failed requests. `ExceptionHandled` is useful in `OnError` to prevent exceptions from bubbling up. Here's an example of registering a global async error handler:

```c#
private async Task HandleFlurlErrorAsync(HttpCall call) {
    await LogErrorAsync(call.Exception.Message);
    MessageBox.Show(call.Exception.Message);
    call.ExceptionHandled = true;
}

FlurlHttp.Configure(settings => settings.OnErrorAsync = HandleFlurlErrorAsync);
```

### Reverting Settings Changes

As mentioned earlier, explicitly setting a value at any level means it will no longer inherit its default, even if that value is `null`. If you need to revert a setting, your only option is to revert them all using `Settings.ResetDefaults()`, which is available at any level.
