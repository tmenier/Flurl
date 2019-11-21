# Flurl

[![Build status](https://ci.appveyor.com/api/projects/status/hec8ioqg0j07ttg5/branch/master?svg=true)](https://ci.appveyor.com/project/kroniak/flurl/branch/master)
[![Flurl](https://img.shields.io/nuget/v/Flurl.svg?maxAge=3600)](https://www.nuget.org/packages/Flurl/)
[![Flurl.Http](https://img.shields.io/nuget/v/Flurl.Http.svg?maxAge=3600)](https://www.nuget.org/packages/Flurl.Http/)

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
        httpTest.RespondWith("OK",200);

        // act
        await sut.CreatePersonAsync("Claire", "Underwood");
        
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

For detailed documentation, please visit the [main site](https://flurl.dev). 
