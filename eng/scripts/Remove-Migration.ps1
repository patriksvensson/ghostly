Push-Location $PSScriptRoot\..\..\src\Ghostly
dotnet ef migrations remove -s ..\Ghostly.Data
Pop-Location