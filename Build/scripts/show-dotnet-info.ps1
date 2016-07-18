
if (Get-Command dotnet -errorAction SilentlyContinue) {
	Write-Host "Using dotnet '$((Get-Command dotnet).Path)'"
	dotnet --version
}
else {
	Write-Host "dotnet.exe not found"
}

