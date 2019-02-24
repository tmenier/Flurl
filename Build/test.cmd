@cd ..\test\Flurl.Test\
@call dotnet test -c Release /p:CollectCoverage=true /p:Threshold=75
@cd ..\..\Build\