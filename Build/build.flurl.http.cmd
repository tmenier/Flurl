@set project=..\src\Flurl.Http.CodeGen\

call dotnet restore --verbosity Error %project%
@if ERRORLEVEL 1 (
echo Error! Restoring dependicies failed.
exit /b 1
) else (
echo Restoring dependicies was successful.
)

call dotnet run -c Release -p %project% ..\src\Flurl.Http.Shared\HttpExtensions.cs
@if ERRORLEVEL 1 (
echo Error! Generation cs file failed.
exit /b 1
)

@set project=..\src\Flurl.Http\

call dotnet restore --verbosity Error %project%
@if ERRORLEVEL 1 (
echo Error! Restoring dependicies failed.
exit /b 1
) else (
echo Restoring dependicies was successful.
)

call dotnet build -c Release %project%