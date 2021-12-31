Push-Location "$PSScriptRoot\src"
&"dotnet" run -- import "$PSScriptRoot/translations" "..\..\..\src\Ghostly.Uwp\Strings"
Pop-Location