@call dotnet --info

@call build.flurl.cmd
@if ERRORLEVEL 1 (
echo Error! Build Flurl failed.
exit /b 1
)

@call build.flurl.http.cmd
@if ERRORLEVEL 1 (
echo Error! Build Flurl.Http failed.
exit /b 1
)