Push-Location "$PSScriptRoot\src"
&"dotnet" run -- check "..\..\..\src\Ghostly.Uwp\Strings"
Pop-Location