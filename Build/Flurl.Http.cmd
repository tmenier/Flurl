mkdir ..\publish
call NuGet.exe pack Flurl.Http.nuspec -OutputDirectory ../publish/ >> pack.log
