call build.flurl.http.cmd
mkdir publish
call nuget.exe pack nuspec\Flurl.http.nuspec -OutputDirectory publish