call build.flurl.cmd
mkdir publish
nuget.exe pack nuspec\Flurl.nuspec -OutputDirectory publish >> pack.log