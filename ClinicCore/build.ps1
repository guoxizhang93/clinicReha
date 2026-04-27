$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$outDir = Join-Path $root "bin"
$dll = Join-Path $outDir "ClinicCore.dll"
$buildDir = Join-Path ([System.IO.Path]::GetTempPath()) "ClinicCoreBuild"
$lib = Join-Path $buildDir "ClinicCore.lib"
$obj = Join-Path $buildDir "clinic_core.obj"
$svmObj = Join-Path $buildDir "svm.obj"
$svmCpp = Join-Path (Split-Path -Parent $root) "libsvm-3.32\libsvm-3.32\svm.cpp"
$cl = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.42.34433\bin\Hostx64\x64\cl.exe"
$sdkLibBase = "C:\Program Files (x86)\Windows Kits\10\Lib"
$sdkIncludeBase = "C:\Program Files (x86)\Windows Kits\10\Include"

if (-not (Test-Path $cl)) {
    throw "cl.exe not found: $cl"
}

$ucrt = Get-ChildItem -Path $sdkLibBase -Directory -ErrorAction Stop |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName "ucrt\x64" } |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1
$um = Get-ChildItem -Path $sdkLibBase -Directory -ErrorAction Stop |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName "um\x64" } |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1
$vcLib = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.42.34433\lib\x64"
$vcInclude = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.42.34433\include"
$sdkIncludeRoot = Get-ChildItem -Path $sdkIncludeBase -Directory -ErrorAction Stop |
    Sort-Object Name -Descending |
    Select-Object -First 1
$ucrtInclude = if ($sdkIncludeRoot) { Join-Path $sdkIncludeRoot.FullName "ucrt" } else { $null }
$umInclude = if ($sdkIncludeRoot) { Join-Path $sdkIncludeRoot.FullName "um" } else { $null }
$sharedInclude = if ($sdkIncludeRoot) { Join-Path $sdkIncludeRoot.FullName "shared" } else { $null }

if (-not $ucrt -or -not $um -or -not (Test-Path $vcLib) -or -not (Test-Path $vcInclude) -or -not (Test-Path $ucrtInclude) -or -not (Test-Path $umInclude) -or -not (Test-Path $sharedInclude)) {
    throw "Required MSVC/Windows SDK library paths were not found."
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
if (Test-Path $buildDir) {
    Remove-Item -LiteralPath $buildDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null

if (-not (Test-Path $svmCpp)) {
    throw "libsvm source not found: $svmCpp"
}

& $cl /nologo /O2 /TP /EHsc /I$vcInclude /I$ucrtInclude /I$umInclude /I$sharedInclude /Fo:$obj /c (Join-Path $root "clinic_core.c")
if ($LASTEXITCODE -ne 0) {
    throw "ClinicCore compile failed: $LASTEXITCODE"
}

& $cl /nologo /O2 /TP /EHsc /I$vcInclude /I$ucrtInclude /I$umInclude /I$sharedInclude /Fo:$svmObj /c $svmCpp
if ($LASTEXITCODE -ne 0) {
    throw "libsvm compile failed: $LASTEXITCODE"
}

& $cl /nologo /LD /Fe:$dll $obj $svmObj /link /LIBPATH:$vcLib /LIBPATH:$ucrt /LIBPATH:$um /OUT:$dll /IMPLIB:$lib
if ($LASTEXITCODE -ne 0) {
    throw "ClinicCore native build failed: $LASTEXITCODE"
}

Write-Host "Built $dll"
