---
layout: default
---

## Installation

Flurl is [available on NuGet](https://www.nuget.org/packages?q=flurl) in several flavors. For all the fluent URL and testable HTTP goodness described on this site:

````
PM> Install-Package Flurl.Http
````

For *just* the [URL builder functionality]({{ site.baseurl }}/fluent-url) without the HTTP features:

````
PM> Install-Package Flurl
````

Working with an XML based API? This package will make your life easier:

````
PM> Install-Package Flurl.Http.Xml
````

(Flurl.Http.Xml is maintained and documented [here](https://github.com/lvermeulen/Flurl.Http.Xml).)

## Supported Platforms

Flurl targets .NET Standard 1.0 and Flurl.Http targets .NET Standard 1.1, meaning both will run on just about every platform that .NET runs on, including .NET Framework, .NET Core, Xamarin (iOS and Android), Mono, Windows Phone, and more. For specific platform versions, see the [.NET Standard compatibility matrix](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Acknowledgements

Flurl is truly a community effort. Special thanks to the following contributors:

- [@kroniak](https://github.com/kroniak) for incredible work on cross-platform support, [automating the build](https://ci.appveyor.com/project/kroniak/flurl/branch/master) and making improvements to the project structure and processes.
- [@lvermeulen](https://github.com/lvermeulen) for creating and maintaining Flurl.Http.Xml.
