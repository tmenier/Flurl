set project=..\src\Flurl.Library\
call dotnet restore %project%
call dotnet build -c Release %project%