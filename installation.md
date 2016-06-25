---
layout: default
---

## Installation

Flurl is [available on NuGet](https://www.nuget.org/packages?q=flurl) in two flavors. For all the fluent URL and testable HTTP goodness described on this site:

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

Both Flurl and Flurl.Http are Portable Class Libraries supporting the following platforms:

- .NET Framework 4 and above for Flurl, 4.5 and above for Flurl.Http
- Windows 8
- Windows Phone 8.1
- Windows Phone Silverlight 8
- Xamarin.Android
- Xamerin.iOS

Special thanks to [@carolynvs](https://github.com/carolynvs) for maintaining the signed packages!
