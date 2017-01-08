---
layout: default
---

## Fluent URL Building

Flurl's URL builder is best explained with an example:

````c#
using Flurl;

var url = "http://www.some-api.com"
	.AppendPathSegment("endpoint")
	.SetQueryParams(new {
		api_key = ConfigurationManager.AppSettings["SomeApiKey"],
		max_results = 20,
		q = "Don't worry, I'll get encoded!"
	})
    .SetFragment("after-hash");
````

At its core is the `Url` class, which is designed to work seamlessly with strings, as demonstrated above. Creating a `Url` via a string extension is purely optional though; you can create one explicitly if you prefer:

````c#
var url = new Url("http://www.some-api.com").AppendPathSegment(...
````

`SetQueryParam` and `SetQueryParams` overwrite any previously set values of the same name, but you can set multiple values of the same name by passing an `IEnumerable`:

````c#
var url = "http://www.mysite.com".SetQueryParam("x", new[] { 1, 2, 3 });
Assert.AreEqual("http://www.mysite.com?x=1&x=2&x=3", url)
````

Note that `Url` converts to a string implicitly, so you can use it directly in any method that takes a string:

````c#
Response.Redirect(url);
````

### Encoding

Flurl takes care of encoding characters in URLs but takes a different approach with path segments than it does with query string values. The assumption is that query string values are highly variable (such as from user input), whereas path segments tend to be more "fixed" and may already be encoded, in which case you don't want to double-encode. Here are the rules Flurl follows:

- Query string values are fully URL-encoded.
- For path segments, *reserved* characters such as `/` and `%` are *not* encoded.
- For path segments, *illegal* characters such as spaces are encoded.
- For path segments, the `?` character is encoded, since query strings get special treatment.

In some cases, you might want to set a query parameter that you to know to be already encoded. `SetQueryParam` has optional `isEncoded` argument:

````c#
url.SetQueryParam("x", "don%27t%20touch%20me", true);
````

While the official URL encoding for the space character is `%20`, it's very common to see `+` used in query parameters. You can tell `Url.ToString` to do this with its optional `encodeSpaceAsPlus` argument:

````c#
var url = "http://foo.com".SetQueryParam("x", "hi there");
Assert.AreEqual("http://foo.com?x=hi%20there", url.ToString());
Assert.AreEqual("http://foo.com?x=hi+there", url.ToString(true));
````

### Parsing

In addition to building URLs, `Flurl.Url` is effective at picking apart an existing one:

````c#
var url = new Url("http://www.mysite.com/with/path?x=1&y=2#foo");
Assert.AreEqual("http://www.mysite.com/with/path", url.Path);
Assert.AreEqual("x=1&y=2", url.Query);
Assert.AreEqual("2", url.QueryParams["y"]);
Assert.AreEqual("foo", url.Fragment);
````

### Utility Methods

Flurl also contains some handy static methods, most notably `Url.Combine`, which is basically a [Path.Combine](http://msdn.microsoft.com/en-us/library/dd991142.aspx) for URLs, ensuring one and only one separator character between parts:

````c#
var url = Url.Combine(
    "http://foo.com/",
    "/too/", "/many/", "/slashes/",
    "too", "few?",
    "x=1", "y=2"
// result: "http://www.foo.com/too/many/slashes/too/few?x=1&y=2"
````

### Url API

For completeness, here are all public methods and properties of `URL`:

````c#
// Properties:

string Path { get; set; }
string Query { get; set; }
string Fragment { get; set; }
QueryParamCollection QueryParams { get; }

// Fluent (chainable) instance methods, each with equivalent string extension:

Url AppendPathSegment(string segment);
Url AppendPathSegments(params string[] segments);
Url AppendPathSegments(IEnumerable<string> segments);
Url SetQueryParam(string name, object value);
Url SetQueryParam(string name, string value, bool isEncoded);
Url SetQueryParams(object values);
Url SetQueryParams(IDictionary values);
Url RemoveQueryParam(string name);
Url RemoveQueryParams(params string[] names);
Url RemoveQueryParams(IEnumerable<string> names);
Url SetFragment(string fragment);
Url RemoveFragment();
Url ResetToRoot();

// Additional instance methods:

bool IsValid();
string ToString(bool encodeSpaceAsPlus);

// Static utility methods:

string Combine(params string[] parts);
QueryParamCollection ParseQueryParams(string query);
string GetRoot(string url);
string DecodeQueryParamValue(string value);
string EncodeQueryParamValue(object value, bool encodeSpaceAsPlus);
string EncodeIllegalCharacters(string urlPart);
````

`QueryParamCollection` implents `IList` and maintains the order of parameters, but it also supports indexing by name like a dictionary. If the same parameter name appears mutiple times in the list, an array is returned.

````c#
var url = new Url("http://foo.com?x=1&x=2");
Assert.AreEqual(new[] { "1", "2" }, url.QueryParams["x"]);
````
