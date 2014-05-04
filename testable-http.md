---
layout: default
---

##Testable HTTP

Flurl.Http provides a set of testing features that make isolated arrange-act-assert style testing dead simple. At its core is `HttpTest`, the creation of which kicks Flurl.Http into test mode, where all HTTP activity is automatically faked.

````c#
using Flurl.Http.Testing;

[Test]
public void Test_Some_Http_Calling_Method() {
    using (var httpTest = new HttpTest()) {
        sut.CallThingThatusesFlurlHttp(); // HTTP calls will be faked!
    }
}
````

Most unit testing frameworks have some notion of setup/teardown code that is executed before and after each test. For classes with lots of tests against HTTP-calling code, you might prefer this approach:

````c#
private HttpTest _httpTest;

[SetUp]
public void CreateHttpTest() {
    _httpTest = new HttpTest();
}

[TearDown]
public void CreateHttpTest() {
    _httpTest.Dispose();
}

[Test]
public void Test_Some_Http_Calling_Method() {
    // Flurl.Http is in test mode
}
````

###Arrange

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

`RespondWith` methods are chainable and will be dequeued and provided in fake responses in the order specified.

````c#
httpTest
    .RespondWith("some response body")
    .RespondWithJson(someObject)
    .RespondWith(500, "error!");
    
sut.DoThingThatMakesSeveralHttpCalls();
````

Like most things in Flurl, you can get lower-level access where the fluent methods don't suit your needs. In this case, add `HttpResponseMessage` objects to the response queue directly:

````c#
httpTest.ResponseQueue.Enqueue(new HttpResponseMessage {...});
````

###Assert

As HTTP methods are faked, they are automatically recorded, allowing you to assert that certain calls were made. `HttpTest` provides a couple assertion methods against the call log:

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
    .WithRequestBody("{\"a\":*,\"b\":*}")
    .Times(1);
````

`Times(n)` allows you to assert that the call was made a specific number of times; otherwise the assertion just tests there are more than zero matches in the call log. `WithRequestBody` supports * wildcards.

Assertions are test framework-agnostic; they throw an exception at any point when a match is not found as specified, signaling a test failure to virtually any testing framework.

You can also access the call log directly if the built-in assertions don't meet your needs:

````c#
Assert.That(httpTest.CallLog.Any(call => /* check an HttpCall */));
````

As implied, `HttpTest.CallLog` is an instance of `List<HttpCall>`. An HttpCall object contains lots of useful information as sepcified [here](/fluent-http/#httpcall).
