[CmdletBinding(PositionalBinding=$false)]

Param(
    [switch] $noConfigure = $false,
    [switch] $noBuild = $false,
    [switch] $install = $false,
    [switch] $pack = $false,
    [switch] $clean = $false,
    [string] $vsDir = $null,
    [string] $version = $null
)

$sourceDir = $(Get-Location)
$buildDir = "$sourceDir/build"
$artifactsDir = "$sourceDir/artifacts"

if ($vsDir -eq $null -or $vsDir -eq "") {
    if (-not (Get-Command "vswhere" -ErrorAction SilentlyContinue)) {
        Write-Host "Either install vswhere or set the -vsDir option"
    }
    $vsDir = & vswhere -property installationPath
}

if (-not (Test-Path "$vsDir\Common7\Tools\Launch-VsDevShell.ps1")) {
    Write-Host "Launch-VsDevShell does not exist"
    exit 1
}

. "$vsDir\Common7\Tools\Launch-VsDevShell.ps1"
Set-Location $sourceDir

# Urgh.
# https://stackoverflow.com/questions/2124753/how-can-i-use-powershell-with-the-visual-studio-command-prompt
cmd /c """$vsDir\VC\Auxiliary\Build\vcvars64.bat"" & set" | foreach {
    if ($_ -match "=") {
        $v = $_.split("="); set-item -force -path "ENV:\$($v[0])" -value "$($v[1])"
    }
}

if ($clean) {
    if (Test-Path $buildDir) {
        Write-Host "Cleaning build directory"
        Remove-Item -Recurse -Force -Path $buildDir
    }
    New-Item $buildDir -ItemType "directory"

    if (Test-Path $artifactsDir) {
        Write-Host "Cleaning artifacts directory"
        Remove-Item -Recurse -Force -Path $artifactsDir
    }
    New-Item $artifactsDir -ItemType "directory"
}

if (-not $noConfigure) {
    Write-Host "Configuring vku"
    cmake -S $sourceDir -B $buildDir -DCMAKE_INSTALL_PREFIX=$artifactsDir -G Ninja
    if (-not $?) {
        exit 1
    }
}

if (-not $noBuild) {
    Write-Host "Building vku"
    cmake --build $buildDir --config Release
    if (-not $?) {
        exit 1
    }
}

if ($install) {
    Write-Host "Installing vku"
    cmake --build $buildDir --target install --config Release
    if (-not $?) {
        exit 1
    }
}

if ($pack) {
    $packArgs = "-property:PackageOutputPath=""$artifactsDir"""
    $packArgs += " -property:Configuration=Release"
    if ("$version" -ne $null) {
        $packArgs += " -property:Version=""$(version.Trim('v'))"""
    }
    Write-Host "Packing helveg"
    dotnet msbuild "$sourceDir" `
        -target:Restore,Build,Pack `
        $packArgs
    if (-not $?) {
        exit 1
    }
}

