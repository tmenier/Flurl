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

Need strongly-named assemblies? These are also available:

````
PM> Install-Package Flurl.Http.Signed
PM> Install-Package Flurl.Signed
````

Working with an XML based API? This package will make your life easier:

````
PM> Install-Package Flurl.Http.Xml
````

(Flurl.Http.Xml is maintained and documented [here](https://github.com/lvermeulen/Flurl.Http.Xml).)

Both Flurl and Flurl.Http enjoy broad cross-platfrom support including the following (minimal required version where indicated):

- .NET Framework 4 and above for Flurl, 4.5 and above for Flurl.Http
- .NET Core 1.0 (via .NET Standard 1.4)
- Windows 8
- Windows Phone 8.1
- Windows Phone Silverlight 8
- Xamarin.Android
- Xamarin.iOS
- Xamarin.Mac
- UAP 1.0
- MonoTouch
- MonoAndroid

Flurl is truly a community effort. Special thanks to the following contributors:

- [@kroniak](https://github.com/kroniak) for incredible work on cross-platform support in the core packages, as well as [automating our build](https://ci.appveyor.com/project/kroniak/flurl/branch/master) and many other improvements to the project and processes.
- [@carolynvs](https://github.com/carolynvs) for creating and maintaining the signed packages.
- [@lvermeulen](https://github.com/lvermeulen) for creating and maintaining Flurl.Http.Xml.
