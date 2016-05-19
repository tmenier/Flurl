call build.flurl.http.cmd
mkdir publish
nuget.exe pack nuspec\Flurl.http.nuspec -OutputDirectory publish >> pack.log