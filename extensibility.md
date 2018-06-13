---
layout: redirect
redirect_to: /docs/extensibility/
---

## Extensibility

Since most of Flurl's functionality is provided through extension methods, it is very easy to extend using the same patterns that Flurl itself uses.

### Extending the URL builder

Chainable URL builder methods generally come in pairs of overloads - one extending `Flurl.Url` and the other extending `String`, both of which should return the modified `Flurl.Url` object:

```c#
public static Url DoMyThing(this Url url) {
    // do something interesting with url
    return url;
}

public static Url DoMyThing(this string url) {
    return new Url(url).DoMyThing(); // construct Url, call the other method
}
```

Now your method is available in 2 scenarios:

```c#
url = "http://api.com".DoMyThing(); // uses string extension
url = "http://api.com"
    .AppendPathSegment("endpoint")
    .DoMyThing(); // uses Url extension
```

### Extending Flurl.Http

Chainable Flurl.Http extension methods generally come in sets of 3 or 4. The first 3, defined on `Url`, `String`, and `IFlurlRequest`, should all return the current `IFlurlRequest` to allow further chaining. Define the `IFlurlRequest` extension first; the others can simply call it, even as a one-line lambda expression in C# 6 or above:

```c#
public static IFlurlRequest DoMyThing(this IFlurlRequest req) {
    // do something interesting with req.Settings, req.Headers, req.Url, etc.
    return req;
}

public static IFlurlRequest DoMyThing(this Url url) => new FlurlRequest(url).DoMyThing();

public static IFlurlRequest DoMyThing(this string url) => new FlurlRequest(url).DoMyThing();
```

Now all of these work:

```c#
result = await "http://api.com"
    .DoMyThing() // string extension
    .GetAsync();

result = "http://api.com"
    .AppendPathSegment("endpoint")
    .DoMyThing() // Url extension
    .GetAsync();

result = "http://api.com"
    .AppendPathSegment("endpoint")
    .WithBasicAuth(u, p)
    .DoMyThing() // IFlurlRequest extension
    .GetAsync();
```

You may want to define a fourth extension method on `IFlurlClient`, if it makes sense for your method to configure some sort of default that should apply to all requests made with that client. This method should return the current `IFlurlClient` for further chaining. `WithHeaders` is an example where Flurl.Http defines all 4 extension methods.

