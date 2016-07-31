@set project=..\src\Flurl\

call dotnet restore --verbosity Error %project%
@if ERRORLEVEL 1 (
echo Error! Restoring dependicies failed.
exit /b 1
) else (
echo Restoring dependicies was successful.
)

call dotnet build -c Release %project%