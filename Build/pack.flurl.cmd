@call build.flurl.cmd
@if ERRORLEVEL 1 (
echo Error! Build Flurl failed.
exit /b 1
)

@md publish > nul 2>&1

@call nuget.exe pack nuspec\Flurl.nuspec -OutputDirectory publish
@if ERRORLEVEL 1 (
echo Error! Packing Flurl failed.
exit /b 1
)