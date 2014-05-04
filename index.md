---
layout: default
title: Home
---

###Flurl is a modern, fluent, asynchronous, testable, portable, buzzword-laden URL builder and HTTP client helper.

````c#
await "https://api.mysite.com"
    .AppendPathSegment("person")
    .SetQueryParams(new { ap_key = "my-key" })
    .WithOAuthBearerToken("MyToken")
    .PostJsonAsync(new { first_name = firstName, last_name = lastName });

[Test]
public void Can_Create_Person() {
    using (var httpTest = new HttpTest()) {
        // arrange
        httpTest.RespondWith(200, "OK");

        // act
        sut.CreatePersonAsync("Frank", "Underwood").Wait();
        
        // assert
        httpTest.ShouldHaveCalled("http://api.mysite.com/*")
            .WithVerb(HttpMethod.Post)
            .WithContentType("application/json");
    }
}
````

With a discoverable API and extensibility at every turn, Flurl is intended to make building and calling URLs easy and downright fun.

##Contents

- [Installation](installation)
- [Fluent URL building](fluent-url)
- [Fluent HTTP](fluent-http)
- [Testable HTTP](testable-http)
- [Configuration](configuration)
- [Extensibility](extensibility)

