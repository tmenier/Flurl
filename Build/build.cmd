@call dotnet --info

@call dotnet restore -v m ../

@if ERRORLEVEL 1 (
echo Error! Restoring dependencies failed.
exit /b 1
) else (
echo Restoring dependencies was successful.
)

@set project=..\src\Flurl.CodeGen\Flurl.CodeGen.csproj

@call dotnet run -c Release -p %project% ..
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