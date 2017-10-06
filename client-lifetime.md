---
layout: default
---

## HttpClient Lifetime Management

Flurl.Http uses [HttpClient](https://msdn.microsoft.com/en-us/library/system.net.http.httpclient.aspx) under the hood. If you're familiar with this object, you probably already know this [advice](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client):

> HttpClient is intended to be instantiated once and re-used throughout the life of an application. Especially in server applications, creating a new HttpClient instance for every request will exhaust the number of sockets available under heavy loads. This will result in SocketException errors.

Starting in version 2.0, Flurl.Http adheres to this guidance by default. Fluent methods like this will create an HTTP client lazily, cache it, and resue it for every call to the same _host_:

```c#
var data = await "http://api.com/endpoint".GetJsonAsync();
```

(Note that this is _not_ the default behavior in 1.x. If you want this behavior without having to manage client objects explicitly, [upgrading](https://www.nuget.org/packages/Flurl.Http/) is highly recommended.)

## Managing Instances Explicitly

`FlurlClient` is a lightweight wrapper around `HttpClient` and is tightly bound to its lifetime. (It implements `IDisposable`, and when disposed will also dispose `HttpClient`.) `FlurlClient` includes a `BaseUrl` property, as well as `Headers`, `Cookies`, `Settings`, and many of the [fluent methods]({{ site.baseurl }}/fluent-http) you may already be familiar with. Most of these properties and methods are used to set defaults that can be overridden at the request level.

So if you want to use Flurl's goodness while maintaining tight control over `HttpClient` instances and their lifetime, `FlurlClient` is the object to use. The `Request` method (new in 2.0) is the preferred way to begin fluently building, configuring and making a call off a `FlurlClient`.

```c#
using (var cli = new FlurlClient("https://api.com").WithOAUthBearerToken(token))
{
    await cli.Request("path", "to", "endpoioint").PostJsonAsync(thing);
    var stuff = await cli.Request("things").SetQueryParam("id", thing.Id).GetAsync();
}
```

## Using Flurl With an IoC Container

Flurl.Http is well suited for use with IoC containers and dependency injection. It provides interfaces for its core classes, most notably `IFlurlClient`. Here are some options for registering `IFlurlClient` with your container:

1. Register `IFlurlClient` as a singleton. The drawback here is if your application interacts with more than one web service, you can't use stateful properties like `BaseUrl` or `Headers` or they'll likely collide.

2. Register `IFlurlClient` and the services that depend on it as transients. The problem here is that the onus is now on the application to ensure the service class instances are long lived and re-used. In something like an ASP.NET application where controllers are frequently created/disposed, that's tricky, and really _should_ be the job of the container.

3. Register `IFlurlClient` as a transient and your services as singletons. This is getting closer to what you want - a single instance per web service. However, a singleton that depends on a transient [is considered bad practice](http://simpleinjector.readthedocs.io/en/latest/LifestyleMismatches.html) and some containers won't allow it.

So how do you get a single instance per web service? The answer lies with the `IFlurlClientFactory` interface (new in 2.0), which exists primarily so that Flurl's instance-per-host default behavior can be overridden. This is also the ideal interface to implement an [instance-per-key](http://simpleinjector.readthedocs.io/en/latest/howto.html#resolve-instances-by-key) IoC strategy. Here's what an implementation might look like:

```c#
public class MyService : IMyService
{
    private readonly IFlurlClient _flurlClient;
    
    public MyService(IFlurlClientFactory flurlClientFac) {
        _flurlClient = flurlClientFac.Get(SERVICE_BASE_URL);
        // configure _flurlClient as needed
    }
    
    public Task<Thing> GetThingAsync(int id) {
        return _flurlClient.Request("things", id).GetAsync<Thing>();
    }
}
```

Now simply register `IFlurlClientFactory` as a singleton and you'll get a single `FlurlClient` instance for this service, regardless of how the service itself is registered.

As for the `IFlurlClientFactory` implementation, `PerHostFlurlClientFactory` (used internally to implement the default implicit behavior) would probably work, but it isn't perfect. Say you have services that wrap 2 versions of the same API, where the base URLs are `https://api.com/v1` and `https://api.com/v2`. Since they have the same host, you'll get the same `FlurlClient` instance for both, which could lead to unexpected behavior.

Flurl comes with an alternative implementation that is better for this scenario: `PerBaseUrlClientFactory`. Rather than picking the host out of the URL you pass to `Get`, it considers the entire URL to be the key. And as a bonus, it will set `IFlurlClient.BaseUrl` to that URL in the returned instance. So, depending on your container, the registration will look something like this:

```c#
container.RegisterSingleton<IFlurlClientFactory, PerBaseUrlClientFactory>();
```