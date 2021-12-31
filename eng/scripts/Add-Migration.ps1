[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

Push-Location $PSScriptRoot\..\..\src\Ghostly
dotnet ef migrations add $Name -s ..\Ghostly.Data
Pop-Location