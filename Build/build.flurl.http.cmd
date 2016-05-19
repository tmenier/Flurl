set project=..\src\Flurl.Http.CodeGen\
call dotnet restore %project%
call dotnet run -c Release -p %project% ..\src\Flurl.Http.Shared\HttpExtensions.cs

set project=..\src\Flurl.Http.Library\
call dotnet restore %project%
call dotnet build -c Release %project%