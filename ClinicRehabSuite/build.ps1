$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$outDir = Join-Path $root "bin"
$exe = Join-Path $outDir "ClinicRehabSuite.exe"
$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$coreRoot = Join-Path (Split-Path -Parent $root) "ClinicCore"
$coreBuild = Join-Path $coreRoot "build.ps1"
$coreDll = Join-Path $coreRoot "bin\ClinicCore.dll"

if (-not (Test-Path $csc)) {
    throw "未找到 .NET Framework C# 编译器：$csc"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

if (Test-Path $coreBuild) {
    powershell -ExecutionPolicy Bypass -File $coreBuild
    if ($LASTEXITCODE -ne 0) {
        throw "ClinicCore build failed: $LASTEXITCODE"
    }
    if (Test-Path $coreDll) {
        Copy-Item -LiteralPath $coreDll -Destination (Join-Path $outDir "ClinicCore.dll") -Force
    }
}

& $csc `
    /nologo `
    /target:winexe `
    /platform:x64 `
    /out:$exe `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    (Join-Path $root "Program.cs")

if ($LASTEXITCODE -ne 0) {
    throw "C# 编译失败，退出码：$LASTEXITCODE"
}

Write-Host "Built $exe"
