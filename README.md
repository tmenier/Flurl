#Flurl

Flurl is a modern, fluent, asynchronous, testable, portable, buzzword-laden URL builder and HTTP client library.

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

Get it on NuGet:

`PM> Install-Package Flurl.Http`

Or get just the stand-alone URL builder without the HTTP features:

`PM> Install-Package Flurl`

For detailed documentation, Please visit the [main site](http://tmenier.github.io/Flurl/).
