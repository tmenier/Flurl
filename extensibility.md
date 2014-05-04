---
layout: default
---

##Extensibility

Since most of Flurl's functionality is provided through extension methods, it is very easy to extend using the same patterns that Flurl itself uses.

###Extending the URL builder

Chainable URL builder methods generally come in pairs of overloads - one extending `Flurl.Url` and the other extending `String`, both of which should return the modified `Flurl.Url` object:

````c#
public static Url DoMyThing(this Url url) {
    // do something interesting with url
    return url;
}

public static Url DoMyThing(this string url) {
    return new Url(url).DoMyThing(); // construct Url, call the other method
}
````

Now your method is available in 2 scenarios:

````c#
url = "http://api.com".DoMyThing(); // uses string extension
url = "http://api.com"
    .AppendPathSegment("endpoint")
    .DoMyThing(); // uses Url extension
````

###Extending Flurl.Http

Chainable Flurl.Http methods generally come in threes - one extending `Flurl.Url`, one extending `string`, and one extending `FlurlClient`, which is just a wrapper around a Flurl.Url and an HttpClient. All return a `FlurlClient`.

````c#
public static FlurlClient DoMyThing(this FlurlClient fc) {
    // do something interesting with fc.Url and fc.HttpClient
    return fc;
}

public static Url DoMyThing(this Url url) {
    return new FlurlClient(url).DoMyThing();
}

public static Url DoMyThing(this string url) {
    return new FlurlClient(url).DoMyThing();
}
````

Now all of these work:

````c#
url = "http://api.com".DoMyThing(); // uses string extension
url = "http://api.com"
    .AppendPathSegment("endpoint")
    .DoMyThing(); // uses Url extension
url = "http://api.com"
    .AppendPathSegment("endpoint")
    .WithBasicAuth(u, p)
    .DoMyThing(); // uses FlurlClient extension
````

###Providing a custom HttpClient factory

For advanced scenarios, you can customize the way Flurl.Http constructs HttpClient instances. Although it is only required that your custom factory implements `Flurl.Http.Configuration.IHttpClientFactory`, it is strongly advised to inherit from `DefaultHttpClientFactory` and extend the client returned by the base implementation. Much of Flurl's functionality, including callbacks, enhanced exceptions, and testability hooks, are dependent on the default factory and could break if you do not use the default as a starting point.

Here is the recommended way to create a custom factory:

````c#
public class MyCustomHttpClientFactory : DefaultHttpClientFactory
{
    public virtual HttpClient CreateClient(Url url)
    {
    	var client = base.CreateClient(Url url);
        // customize the client here
        return client;
    }
}
````

Register the factory as part of your [global configuration]({{ site.baseurl }}/configuration):

````c#
FlurlHttp.Configure(c => {
    c.HttpClientFactory = new MyCustomHttpClientFactory();
});
````
