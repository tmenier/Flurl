---
layout: default
---

## Error Handling

By default, Flurl.Http throws a `FlurlHttpException` on any non-2XX HTTP status.

```c#
try {
    await url.PostJsonAsync(poco);
}
catch (FlurlHttpTimeoutException) {
    // FlurlHttpTimeoutException derives from FlurlHttpException; catch here only
    // if you want to handle timeouts as a special case
    LogError("Request timed out.");
}
catch (FlurlHttpException ex) {
    // ex.Message contains rich details, inclulding the URL, verb, response status,
    // and request and response bodies (if available)
    LogError(ex.Message);
}
```

To inspect individual details of the call, or to provide a custom error message, you do not need to parse the `Message` property. `FlurlHttpException` also contains a `Call` property, which is an instance [the same HttpCall object used by event handlers]({{ site.baseurl }}/configuration/#event-handlers), providing a wealth of details about the call.

`FlurlHttpException` also provides shortcuts for deserializing JSON responses:
 
```c#
catch (FlurlHttpException ex) {
    // For error responses that take a known shape
    TError e = ex.GetResponseJson<TError>();

    // For error responses that take an unknown shape
    dynamic d = ex.GetResponseJson();
}
```

### Allowing Non-2XX Responses

You can allow addtional HTTP statuses (i.e. prevent throwing) fluently per request:

```c#
url.AllowHttpStatus(HttpStatusCode.NotFound, HttpStatusCode.Conflict).GetAsync();
url.AllowHttpStatus("400-404,6xx").GetAsync();
url.AllowAnyHttpStatus().GetAsync();
```

The pattern in the second example is fairly self-explanatory; allowed characters include digits, commas for separators, hyphens for ranges, and wildcards `x` or `X` or `*`.

You can also override the default behavior at [any settings level]({{ site.baseurl }}/configuration) via `Settings.AllowedHttpStatusRange`, which also takes a string pattern. Note that unlike the fluent methods (which are additive), you must include `2XX` in this setting if you want to allow all statuses in that range.
