$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
New-Item -ItemType Directory -Force "$PSScriptRoot/dist\data" | Out-Null
& $compiler /nologo /target:winexe /out:dist\LeagueCooldowns.exe /reference:"$framework\WPF\PresentationFramework.dll" /reference:"$framework\WPF\PresentationCore.dll" /reference:"$framework\WPF\WindowsBase.dll" /reference:"$framework\System.Xaml.dll" /reference:"$framework\System.Net.Http.dll" "src\WikiParser.cs" "src\App.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed' }
Copy-Item "$PSScriptRoot/data/champions.tsv" "$PSScriptRoot/dist\data/champions.tsv" -Force



} finally { Pop-Location }
