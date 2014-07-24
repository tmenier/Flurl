#Flurl

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

For detailed documentation, Please visit the [main site](http://tmenier.github.io/Flurl/).
