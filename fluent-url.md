---
layout: default
---

##Fluent URL Building

Flurl's URL builder is best explained with an example:

````c#
using Flurl;

var url = "http://www.some-api.com"
	.AppendPathSegment("endpoint")
	.SetQueryParams(new {
		api_key = ConfigurationManager.AppSettings["SomeApiKey"],
		max_results = 20,
		q = "Don't worry, I'll get encoded!"
	});
````

At its core is the `Url` class, which is designed to work seamlessly with strings, as demonstrated above. Creating a `Url` via a string extension is purely optional though; you can create one explicitly if you prefer:

````c#
var url = new Url("http://www.some-api.com").AppendPathSegment(...
````

A `Url` also converts back to a string implicitly, so you can use it directly in any method that takes a string:

````c#
Response.Redirect(url);
````

Flurl also contains the handy `Url.Combine` static method, which is basically a [Path.Combine](http://msdn.microsoft.com/en-us/library/dd991142.aspx) for URLs, ensuring one and only one separator character between segments:

````c#
var url = Url.Combine("http://foo.com/", "/too/", "/many/", "too", "few");
// result: "http://www.foo.com/too/many/too/few"
````

###Encoding

Flurl takes care of encoding characters in URLs but takes a different approach with path segments than it does with query string values. The assumption is that query string values are highly variable (such as from user input), whereas path segments tend to be more "fixed" and may already be encoded, in which case you don't want to double-encode. Here are the rules Flurl follows:

- Query string values are fully URL-encoded.
- For path segments, *reserved* characters such as `/` and `%` are *not* encoded.
- For path segments, *illegal* characters such as spaces are encoded.
- For path segments, the `?` character is encoded, since query strings get special treatment.

###Url API

The `Url` API is small, discoverable, and fairly self-explanatory. For completeness, here are all public methods and properties:

````c#
// Static method:

static string Combine(string url, params string[] segments);

// Instance methods (each with equivalent string extension):

Url AppendPathSegment(string segment);
Url AppendPathSegments(params string[] segments);
Url AppendPathSegments(IEnumerable<string> segments);
Url SetQueryParam(string name, object value);
Url SetQueryParams(object values);
Url SetQueryParams(IDictionary values);
Url RemoveQueryParam(string name);
Url RemoveQueryParams(params string[] names);
Url RemoveQueryParams(IEnumerable<string> names);

// Properties:

string Path { get; }
IDictionary<string, object> QueryParams { get; }
````

