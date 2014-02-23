#Flurl

Flurl is a tiny library for building URLs. It is best explained with an example:

````C#
var url = "http://www.some-api.com"
	.AppendPathSegment("endpoint")
	.SetQueryParams(new {
		api_key = ConfigurationManager.AppSettings["SomeApiKey"],
		max_results = 20,
		q = "Don't worry, I'll get encoded!"
	});
````

At its core is the `Url` class, which is designed to work seamlessly with strings, as demonstrated with the extension method above. Creating a `Url` via a string extension is purly optional though; you can create one explicitly if you prefer:

````C#
var url = new Url("http://www.some-api.com").AppendPathSegment(...
````

A `Url` also converts back to a string implicitly, so you can use it directly in any method that takes a string:

````C#
var result = await new HttpClient.GetAsync(url);
````

Flurl also contains the handy `Url.Combine` method, which is basically a `[Path.Combine](http://msdn.microsoft.com/en-us/library/dd991142.aspx)` for URLs, ensuring one and only one separator character between segments:

````C#
var url = Url.Combine("http://www.foo.com/", "/too/", "/many/", "/slashes/", "too", "few");
// result: "http://www.foo.com/too/many/slashes/too/few"
````

###Encoding

Flurl takes care of encoding characters in URLs but takes a different approach with path segments than it does with query string values. The assumption is that query string values are highly variable (such as from user input), whereas path segments tend to be more "fixed" and may already be encoded, in which case you don't want to double-encode. Here are the rule Flurl follows:

- Query string values are fully URL-encoded.
- For path segments, *reserved* characters such as `/`, `%`, and `+` are *not* encoded.
- For path segments, *illegal* characters such as spaces are encoded.
- For path segments, the `?` character is encoded, since query strings get special treatment.

###Url API

The `Url` API is small, discoverable, and fairly self-explanatory. For completeness, here are all public methods and properties:

````C#
// Static method:

static string Combine(string url, params string[] segments)

// Instance methods (each with equivalent string extension):

Url AppendPathSegment(string segment)
Url AppendPathSegments(params string[] segments)
Url AppendPathSegments(IEnumerable<string> segments)
Url SetQueryParam(string name, object value)
Url SetQueryParams(object values)
Url SetQueryParams(IDictionary values)
Url RemoveQueryParam(string name)
Url RemoveQueryParams(params string[] names)
Url RemoveQueryParams(IEnumerable<string> names)

// Properties:

string Path { get; }
NameValueCollection QueryParams { get; }
````

###Get it on NuGet

````
PM> Install-Package Flurl
````

###Roadmap

The next version of Flurl will add a set of asynchronous HTTP methods to the `Url` API. Things like `GetJsonAsync<T>`, `DownloadFileAsync`, and `PostFormDataAsync` are in the works. These will be thin wrappers around `HttpClient` that encapsulate best practices, and they will be fully testable. (The core URL builder will always be available as a stand-alone package.) Please follow this project to stay current with the progress. If you have a feature request or suggestion for improvement, please create an issue.