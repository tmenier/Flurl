---
layout: default
---

## Fluent HTTP

Flurl allows you to perform many common HTTP tasks directly off the fluent URL builder chain. Barely under the hood is [HttpClient](http://blogs.msdn.com/b/henrikn/archive/2012/02/11/httpclient-is-here.aspx) and related classes. As you'll see, Flurl enhances HttpClient with convenience methods and fluent goodness but doesn't try to abstract it away completely.

````c#
using Flurl;
using Flurl.Http;
````

Send a GET or HEAD request and get back a raw [HttpResponseMessage](http://msdn.microsoft.com/en-us/library/system.net.http.httpresponsemessage):

````c#
var getResp = await "http://api.foo.com".GetAsync();
var headResp = await "http://api.foo.com".HeadAsync();
````

Get a strongly-typed poco from a JSON API:

````c#
T poco = await "http://api.foo.com".GetJsonAsync<T>();
````

When creating classes to match the JSON seems like overkill, the non-generic version returns a dynamic:

````c#
dynamic d = await "http://api.foo.com".GetJsonAsync();
````

Or get a list of dynamics from an API that returns a JSON array:

````c#
var list = await "http://api.foo.com".GetJsonListAsync();
````

Get strings, bytes, and streams:

````c#
string text = await "http://site.com/readme.txt".GetStringAsync();
byte[] bytes = await "http://site.com/image.jpg".GetBytesAsync();
Stream stream = await "http://site.com/music.mp3".GetStreamAsync();
````

Download a file:

````c#
// filename is optional here; it will default to the remote file name
var path = await "http://files.foo.com/image.jpg"
    .DownloadFileAsync("c:\\downloads", filename);
````

Post some JSON data:

````c#
await "http://api.foo.com".PostJsonAsync(new { a = 1, b = 2 });
````

Simulate an HTML form post:

````c#
await "http://site.com/login".PostUrlEncodedAsync(new { 
    user = "user", 
    pass = "pass"
});
````

The Post methods above return a `Task<HttpResponseMessage>`. You may of course expect some data to be returned in the response body:

````c#
T poco = await url.PostJsonAsync(data).ReceiveJson<T>();
dynamic d = await url.PostUrlEncodedAsync(data).ReceiveJson();
string s = await url.PostUrlEncodedAsync(data).ReceiveString();
````

### Configuring Client Calls

There are a variety of ways to set up HTTP calls without breaking the fluent chain.

Set request headers:

````c#
// one:
await url.WithHeader("someheader", "foo").GetJsonAsync();
// multiple:
await url.WithHeaders(new { h1 = "foo", h2 = "bar" }).GetJsonAsync();
````

Authenticate with Basic Authentication:

````c#
await url.WithBasicAuth("username", "password").GetJsonAsync();
````

Authenticate with an OAuth bearer token:

````c#
await url.WithOAuthBearerToken("mytoken").GetJsonAsync();
````

Specify a timeout:

````c#
await url.WithTimeout(10).DownloadFileAsync(); // 10 seconds
await url.WithTimeout(TimeSpan.FromMinutes(2)).DownloadFileAsync();
````

Set some cookies:

````c#
// one:
await url.WithCookie("name", "value", expDate).HeadAsync();
// multiple:
await url.WithCookies(new { c1 = 1, c2 = 2 }, expDate).HeadAsync();
// date is optional; excluding it makes it a session cookie.
````

Get at the underlying HttpClient directly if none of the other convenience methods meet your needs:

````c#
await url.ConfigureHttpClient(http => /* configure away! */).GetJsonAsync();
````

`ConfigureHttpClient` is a nice hook for extending Flurl with your own extension methods.