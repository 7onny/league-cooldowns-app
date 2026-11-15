$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $f = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319"
    New-Item -ItemType Directory -Force build | Out-Null
    & "$f\csc.exe" /nologo /target:exe /main:Checks /out:build\Checks.exe /reference:"$f\WPF\PresentationFramework.dll" /reference:"$f\WPF\PresentationCore.dll" /reference:"$f\WPF\WindowsBase.dll" /reference:"$f\System.Xaml.dll" /reference:"$f\System.Net.Http.dll" src\WikiParser.cs src\App.cs tests\Checks.cs
    if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
    ./build/Checks.exe
    if ($LASTEXITCODE -ne 0) { throw 'Checks failed' }
} finally { Pop-Location }
