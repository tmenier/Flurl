---
layout: default
---

## Testable HTTP

Flurl.Http provides a set of testing features that make isolated arrange-act-assert style testing dead simple. At its core is `HttpTest`, the creation of which kicks Flurl into test mode, where all HTTP activity in the test subject is automatically faked and recorded. No need for wrapper interfaces or injected mocks.

````c#
using Flurl.Http.Testing;

[Test]
public void Test_Some_Http_Calling_Method() {
    using (var httpTest = new HttpTest()) {
        // Flurl is now in test mode
        sut.CallThingThatUsesFlurlHttp(); // HTTP calls are faked!
    }
}
````

Most unit testing frameworks have some notion of setup/teardown methods that are executed before/after each test. For classes with lots of tests against HTTP-calling code, you might prefer this approach:

````c#
private HttpTest _httpTest;

[SetUp]
public void CreateHttpTest() {
    _httpTest = new HttpTest();
}

[TearDown]
public void DisposeHttpTest() {
    _httpTest.Dispose();
}

[Test]
public void Test_Some_Http_Calling_Method() {
    // Flurl is in test mode
}
````

### Arrange

By default, fake HTTP calls return a 200 (OK) status with an empty body. Of course you'll likely want to test your code against other responses.

````c#
httpTest.RespondWith("some response body");
sut.DoThing();
````

Use objects for JSON responses:

````c#
httpTest.RespondWithJson(new { x = 1, y = 2 });
````

Test failure conditions:

````c#
httpTest.RespondWith(500, "server error");
httpTest.RespondWithJson(401, new { message = "unauthorized" });
httpTest.SimulateTimeout();
````

`RespondWith` methods are chainable. Responses will be dequeued and provided to the calling code in the order they were added.

````c#
httpTest
    .RespondWith("some response body")
    .RespondWithJson(someObject)
    .RespondWith(500, "error!");
    
sut.DoThingThatMakesSeveralHttpCalls();
````

When the fluent methods don't suit your needs, you can create and add `HttpResponseMessage`s to the queue directly:

````c#
var response = new HttpResponseMessage { ... };
httpTest.ResponseQueue.Enqueue(response);
````

### Assert

As HTTP methods are faked, they are automatically recorded, allowing you to assert that certain calls were made. Assertions are test framework-agnostic; they throw an exception at any point when a match is not found as specified, signaling a test failure in virtually all testing frameworks.

`HttpTest` provides a couple assertion methods against the call log:

````c#
sut.DoThing();
httpTest.ShouldHaveCalled("http://some-api.com/*");
httpTest.ShouldNotHaveCalled("http://other-api.com/*");
````

Notice that both support the * wildcard character, which can appear anywhere in the URL.

You can make further assertions against the call log with a fluent API:

````c#
httpTest.ShouldHaveCalled("http://some-api.com/*")
    .WithVerb(HttpMethd.Post)
    .WithContentType("application/json")
    .WithRequestBody("{\"a\":*,\"b\":*}") // supports wildcards
    .Times(1);
````

`Times(n)` allows you to assert that the call was made a specific number of times; otherwise, the assertion passes when one or more matching calls were made.

Here again, you have lower-level access when the fluent methods aren't enough. In this case, to the call log:

````c#
Assert.That(httpTest.CallLog.Any(call => /* check an HttpCall */));
````

CallLog is an instance of `List<HttpCall>`. An `HttpCall` object contains lots of useful information as sepcified [here]({{ site.baseurl }}/fluent-http/#httpcall).
