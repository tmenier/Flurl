mkdir ..\publish
call NuGet.exe pack Flurl.nuspec -OutputDirectory ../publish/ >> pack.log
