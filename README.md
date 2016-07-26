#Flurl

[![Build 
status](https://ci.appveyor.com/api/projects/status/hec8ioqg0j07ttg5?svg=true)](https://ci.appveyor.com/project/kroniak/flurl) 
[![NuGet](https://img.shields.io/nuget/v/Flurl.Http.svg?maxAge=86400)](https://www.nuget.org/packages/Flurl.Http/)
[![MyGet Pre 
Release](https://img.shields.io/myget/flurl/vpre/Flurl.Http.svg?maxAge=86400)](https://www.myget.org/feed/flurl/package/nuget/Flurl.Http)

Flurl is a modern, fluent, asynchronous, testable, portable, buzzword-laden URL builder and HTTP client library.

````c#
var result = await "https://api.mysite.com"
    .AppendPathSegment("person")
    .SetQueryParams(new { api_key = "xyz" })
    .WithOAuthBearerToken("my_oauth_token")
    .PostJsonAsync(new { first_name = firstName, last_name = lastName })
    .ReceiveJson<T>();

[Test]
public void Can_Create_Person() {
	// fake & record all http calls in the test subject
    using (var httpTest = new HttpTest()) {
        // arrange
        httpTest.RespondWith(200, "OK");

        // act
        await sut.CreatePersonAsync("Frank", "Underwood");
        
        // assert
        httpTest.ShouldHaveCalled("http://api.mysite.com/*")
            .WithVerb(HttpMethod.Post)
            .WithContentType("application/json");
    }
}
````

Get it on NuGet:

`PM> Install-Package Flurl.Http`

Or get just the stand-alone URL builder without the HTTP features:

`PM> Install-Package Flurl`

For updates and announcements, [follow @FlurlHttp on Twitter](https://twitter.com/intent/user?screen_name=FlurlHttp).

For detailed documentation, please visit the [main site](http://tmenier.github.io/Flurl/). 
