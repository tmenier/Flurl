---
layout: default
---

##HttpClient Lifetime Management

As of [Flurl.Http](https://www.nuget.org/packages/Flurl.Http) 0.4, instances of the underlying `HttpClient` are disposed immediately after each HTTP call in all typical fluent use cases. However, when making multiple calls to the same host, it is more efficient to re-use `HttpClient`, and hence re-use the open connection. But where's the mechanism to accomplish that in a fluent expression like this?

````c#
var data = await "http://api.com/endpoint".GetJsonAsync();
````

Flurl.Http 0.4 introduces a new pattern for re-using `HttpClient` instances. It involves explicitly creating a `FlurlClient`, and "attaching" to it in your fluent expressions using `Url.WithClient` or `FlurlClient.WithUrl`.

````c#
// method 1:
using (var fc = new FlurlClient())
{
    await url1.WithClient(fc).DoFluentAsyncThing();
    await url2.WithClient(fc).DoOtherFluentAsyncThing();
}

// method 2:
using (var fc = new FlurlClient())
{
    await fc.WithUrl(url1).DoFluentAsyncThing();
    await fc.WithUrl(url2).DoOtherFluentAsyncThing();
}
````

The 2 methods above are equivalent; use whichever you prefer.

One scenario where this comes in handy is when simulating a browser session, where cookies set on the server need to be present in each successive call.

````c#
using (var fc = new FlurlClient().EnableCookies())
{
    await url
        .AppendPathSegment("login")
        .WithClient(fc)
        .PostUrlEncodedAsync(new { user = "user", pass = "pass" });
        
    var page = await url
        .AppendPathSegment("home")
        .WithClient(fc)
        .GetStringAsync();
        
    // need to inspect the cookies?
    var sessionId = fc.Cookies["session_id"].Value;
}
````

In this example, the call to `EnableCookies()` is important - this will initialize the underlying `CookieContainer`, a bit of overhead that is not performed by default.


