@set project=..\test\Flurl.Test.NETCore\

call dotnet restore --verbosity Error %project%
@if ERRORLEVEL 1 (
echo Error! Restoring dependicies failed.
exit /b 1
) else (
echo Restoring dependicies was successful.
)

call dotnet test -c Release %project%