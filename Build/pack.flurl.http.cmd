@call build.flurl.http.cmd
@if ERRORLEVEL 1 (
echo Error! Build Flurl.Http failed.
exit /b 1
)

@md publish > nul 2>&1

@call nuget.exe pack nuspec\Flurl.http.nuspec -OutputDirectory publish
@if ERRORLEVEL 1 (
echo Error! Packing Flurl.Http failed.
exit /b 1
)