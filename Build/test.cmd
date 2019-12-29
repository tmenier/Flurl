@call dotnet test -c Release /p:CollectCoverage=true /p:Threshold=80 /p:Include=\"[Flurl]*,[Flurl.Http]*\" /p:Exclude="[*]*.GeneratedExtensions" ..\test\Flurl.Test\
@if ERRORLEVEL 1 (
echo Error! Tests for Flurl failed.
exit /b 1
)