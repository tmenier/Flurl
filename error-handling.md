---
layout: default
---

##Error Handling

Flurl.Http deviates from HttpClient conventions a bit when it comes to error responses. HttpClient doesn't throw exceptions upon receiving unsuccessful HTTP response codes; by default, Flurl does. 

````c#
try {
    T poco = await "http://api.foo.com".GetJsonAsync<T>();
}
catch (FlurlHttpTimeoutException) {
    LogError("Timed out!");
}
catch (FlurlHttpException ex) {
    if (ex.Call.Response != null)
        LogError("Failed with response code " + call.Response.StatusCode);
    else
        LogError("Totally failed before getting a response! " + ex.Message);
}
````

###Inspecting Error Response Bodies

You'll often want to inspect the response body, which may or may not take a known shape, from the `catch` block:

````c#
catch (FlurlHttpException ex) {
    // For error responses that take a known shape
    TError e = ex.GetResponseJson<TError>();

    // For error responses that take an unknown shape
    dynamic d = ex.GetResponseJson();
    string s = ex.GetResponseString();
}
````

You might notice that the calls above are not asynchronous. Since you cannot `await` inside a `catch` block as of C# 4.5, Flurl.Http asynchronously captures the response body (for failed status codes only) *before* throwing the exception, enabling the convenience of inspecting it from within the `catch` block without introducing blocking.

###Allowing Non-2XX Responses

As noted in the [Configuration]({{ site.baseurl }}/configuration) section, you can globally configure specific status codes or ranges (in addition to 2xx) that should not throw exceptions. This can also be defined per call:

````c#
await url.AllowHttpStatus("100-299,6xx").GetAsync();
await url.AllowAnyHttpStatus().GetAsync();
````
