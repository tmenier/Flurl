call build.flurl.cmd
mkdir publish
call nuget.exe pack nuspec\Flurl.nuspec -OutputDirectory publish