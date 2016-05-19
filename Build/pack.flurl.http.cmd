SET dir=%cd%

call build.flurl.http.cmd

cd %dir%
mkdir publish
nuget.exe pack nuspec\Flurl.http.nuspec -OutputDirectory publish >> pack.log
