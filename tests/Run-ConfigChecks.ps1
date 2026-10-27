param([string]$MSBuildPath)

$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent $PSScriptRoot
if (-not $MSBuildPath) {
    $command = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($command) {
        $MSBuildPath = $command.Source
    } else {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
        if (Test-Path -LiteralPath $vswhere) {
            $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1
        }
    }
}
if (-not $MSBuildPath) { throw '找不到 Visual Studio MSBuild，请通过 -MSBuildPath 指定。' }

& $MSBuildPath (Join-Path $repository 'RtMap.sln') /t:Build /p:Configuration=Release /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'RtMap 编译失败。' }

$fixtureRoot = Join-Path $repository ('obj\ConfigChecks\' + [Guid]::NewGuid().ToString('N'))
$output = Join-Path $fixtureRoot 'child\bin'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$release = Join-Path $repository 'bin\Release'
Copy-Item -LiteralPath (Join-Path $release 'RtMap.exe'), (Join-Path $release 'Newtonsoft.Json.dll') -Destination $output
$csc = Join-Path (Split-Path -Parent $MSBuildPath) 'Roslyn\csc.exe'
$executable = Join-Path $output 'ConfigChecks.exe'
& $csc /nologo /target:exe "/out:$executable" "/reference:$(Join-Path $output 'RtMap.exe')" "/reference:$(Join-Path $output 'Newtonsoft.Json.dll')" (Join-Path $PSScriptRoot 'ConfigChecks.cs')
if ($LASTEXITCODE -ne 0) { throw '配置检查程序编译失败。' }

& $executable (Join-Path $repository 'config\AppSettings.example.json') $fixtureRoot
if ($LASTEXITCODE -ne 0) { throw '配置检查失败。' }
