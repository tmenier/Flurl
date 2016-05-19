SET dir=%cd%

call build.flurl.cmd

cd %dir%
mkdir publish
nuget.exe pack nuspec\Flurl.nuspec -OutputDirectory publish >> pack.log
