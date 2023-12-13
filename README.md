# Flurl

[![build](https://github.com/tmenier/Flurl/actions/workflows/ci.yml/badge.svg)](https://github.com/tmenier/Flurl/actions/workflows/ci.yml)
[![NuGet Version](http://img.shields.io/nuget/v/Flurl.Http.svg?style=flat)](https://www.nuget.org/packages/Flurl.Http/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flurl.Http.svg)](https://www.nuget.org/packages/Flurl.Http/)

Flurl is a modern, fluent, asynchronous, testable, portable, buzzword-laden URL builder and HTTP client library.

```cs
var result = await "https://api.mysite.com"
    .AppendPathSegment("person")
    .SetQueryParams(new { api_key = "xyz" })
    .WithOAuthBearerToken("my_oauth_token")
    .PostJsonAsync(new { first_name = firstName, last_name = lastName })
    .ReceiveJson<T>();

[Test]
public void Can_Create_Person() {
    // fake & record all http calls in the test subject
    using var httpTest = new HttpTest();

    // arrange
    httpTest.RespondWith("OK", 200);

    // act
    await sut.CreatePersonAsync("Frank", "Reynolds");
        
    // assert
    httpTest.ShouldHaveCalled("http://api.mysite.com/*")
        .WithVerb(HttpMethod.Post)
        .WithContentType("application/json");
}
```

Get it on NuGet:

`PM> Install-Package Flurl.Http`

Or get just the stand-alone URL builder without the HTTP features:

`PM> Install-Package Flurl`

For updates and announcements, [follow @FlurlHttp on Twitter](https://twitter.com/intent/user?screen_name=FlurlHttp).

For detailed documentation, please visit the [main site](https://flurl.dev). 
