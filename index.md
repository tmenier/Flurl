---
layout: default
title: Home
---

### Flurl is a modern, fluent, asynchronous, testable, portable, buzzword-laden URL builder and HTTP client library.

````c#
var result = await "https://api.mysite.com"
    .AppendPathSegment("person")
    .SetQueryParams(new { a = 1, b = 2 })
    .WithOAuthBearerToken("my_oauth_token")
    .PostJsonAsync(new { first_name = "Frank", last_name = "Underwood" })
    .ReceiveJson<T>();
````

With a discoverable API, [extensibility](extensibility) at every turn, and a nifty set of [testing features](testable-http), Flurl is intended to make building and calling URLs easy and downright fun.

## Documentation

- [Installation](installation)
- [Fluent URL building](fluent-url)
- [Fluent HTTP](fluent-http)
- [Testable HTTP](testable-http)
- [Configuration](configuration)
- [Error Handling](error-handling)
- [HttpClient Lifetime Management](client-lifetime)
- [Extensibility](extensibility)
