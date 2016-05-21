set project=..\test\Flurl.Test.NETCore\
call dotnet --info
call dotnet restore %project%
call dotnet run -c Release -p %project%