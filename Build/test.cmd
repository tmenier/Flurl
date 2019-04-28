@call dotnet test -c Release /p:CollectCoverage=true /p:Threshold=75 /p:Exclude="[NUnit3.*]*" ..\test\Flurl.Test\
@if ERRORLEVEL 1 (
echo Error! Tests for Flurl failed.
exit /b 1
)