# Flurl.Http.Newtonsoft

[![NuGet version (Flurl.Http.Newtonsoft)](https://img.shields.io/nuget/v/Flurl.Http.Newtonsoft.svg?style=flat-square)](https://www.nuget.org/packages/Flurl.Http.Newtonsoft/)

Flurl.Http 4.0 [removed](https://github.com/tmenier/Flurl/issues/517) the `Newtonsoft.Json` dependency in favor of `System.Text.Json` for its default JSON serializer implementation. This has several advantages, most notably that it dramatically reduces Flurl's footprint, which is especially important to developers using it in mobile apps and browser platforms.

But it's also a breaking change. Upgrading from 3.x may cause some pains, such as:

- Newtonsoft's [serialization attributes](https://www.newtonsoft.com/json/help/html/serializationattributes.htm) no longer have any effect, and need be replaced by their STJ equivalents.
- Newtonsoft's [configuration settings](https://www.newtonsoft.com/json/help/html/serializationsettings.htm) also no longer have any effect in Flurl.
- Support for `dynamic`s has also been [removed](https://github.com/tmenier/Flurl/issues/699) in Flurl, specifically due to the lack of support in STJ.

This package aims to solve these problems, making upgrading easier for those who still want or need Newtonsoft-based serialization. Included in this package:

- `NewtonsoftJsonSearializer`, an instance of which can be assigned to a Flurl client or request via `Settings.JsonSerializer`.
- Shortcuts for enabling the serializer more "globally":
    - When using the clientless pattern: `FlurlHttp.Clients.UseNewtonsoft()`
    - When using DI / [named clients](https://github.com/tmenier/Flurl/issues/770): `new FlurlClientCache().UseNewtonsoft()`
- Non-generic `dynamic`-returning `GetJson`, `GetJsonList`, `ReceiveJson`, and `ReceiveJsonList` extension methods.

Both `NewtonsoftJsonSerializer` and the `UseNewtonsoft` shortcuts take an optional `JsonSerializerSettings` parameter. In fact, the serializer was lifted directly from 3.x, so you can trust that it's battle-tested and highly performant.