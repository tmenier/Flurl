@call dotnet --info

@call dotnet restore -v m ../

@if ERRORLEVEL 1 (
echo Error! Restoring dependicies failed.
exit /b 1
) else (
echo Restoring dependicies was successful.
)

@set project=..\src\Flurl.Http.CodeGen\Flurl.Http.CodeGen.csproj

@call dotnet run -c Release -p %project% ..\src\Flurl.Http\GeneratedExtensions.cs
@if ERRORLEVEL 1 (
echo Error! Generation cs file failed.
exit /b 1
)

@set project=..\src\Flurl\

@call dotnet build -c Release %project%

@if ERRORLEVEL 1 (
echo Error! Build Flurl failed.
exit /b 1
)

@set project=..\src\Flurl.Http\

@call dotnet build -c Release %project%

@if ERRORLEVEL 1 (
echo Error! Build Flurl.Http failed.
exit /b 1
)