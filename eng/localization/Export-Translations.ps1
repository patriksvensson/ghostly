Push-Location "$PSScriptRoot\src"
&"dotnet" run -- export "..\..\..\src\Ghostly.Uwp\Strings" "$PSScriptRoot"
Pop-Location