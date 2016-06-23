---
layout: default
---

##HttpClient Lifetime Management

As of [Flurl.Http](https://www.nuget.org/packages/Flurl.Http) 0.4, instances of the underlying `HttpClient` are disposed immediately after each HTTP call by default. This is the case with almost every example on this site, and is suitable for most typical use cases. However, when making multiple calls to the same host, there are small performance gains to be had by re-using `HttpClient`, and hence re-using the underlying open connection. But where's the mechanism to accomplish that in a fluent expression like this?

````c#
var data = await "http://api.com/endpoint".GetJsonAsync();
````

Flurl.Http 0.4 introduces an optional new pattern for re-using `HttpClient` instances. It involves explicitly creating a `FlurlClient`, and "attaching" to it in your fluent expressions using `Url.WithClient` or `FlurlClient.WithUrl`.

````c#
// method 1:
using (var fc = new FlurlClient())
{
    result1 = await url1.WithClient(fc).GetJsonAsync();
    result2 = await url2.WithClient(fc).GetJsonAsync();
}

// method 2:
using (var fc = new FlurlClient())
{
    result1 = await fc.WithUrl(url1).GetJsonAsync();
    result2 = await fc.WithUrl(url2).GetJsonAsync();
}
````

The 2 methods result in equivalent behavior. The first is more suitable if you're fluently building and calling the URL inline; either works when the URL is fixed or built ahead of time. Use whichever you prefer

One scenario where this comes in handy is simulating a browser session, where cookies set on the server need to be present in each successive call.

````c#
using (var fc = new FlurlClient(url).EnableCookies())
{
    await url
        .AppendPathSegment("login")
        .WithClient(fc)
        .PostUrlEncodedAsync(new { user = "user", pass = "pass" });
        
    var page = await url
        .AppendPathSegment("home")
        .WithClient(fc)
        .GetStringAsync();
        
    // Need to inspect the cookies? FlurlClient exposes them as a dictionary.
    var sessionId = fc.Cookies["session_id"].Value;
}
````

In this example, the call to `EnableCookies()` is important. Cookie functionality carries a small amount of overhead that is avoided by default.

###How important is it to dispose of HttpClient?

The general consensus is that [it's not very important](http://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed), particularly in cases where you have relatively few instances and are re-using them. So although the above examples demonstrate disposing `FlurlClient` via `using` statements, it may be appropriate to let them live much longer. For example, you may want to hold a reference to one in a member variable of a class where many calls are made to the same host.

###Should I hold static instances of FlurlClient?

This is *not* recommended for anything other than single-threaded applications. `FlurlClient` and `HttpClient` are far from stateless; if you have multiple threads setting headers at the same time, for example, the results could be very unpredictable. And if you try to solve this problem through locking, you'll likely create a much bigger performance bottleneck than if you simply allowed multiple instances to be created.

