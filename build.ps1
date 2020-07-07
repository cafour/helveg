[CmdletBinding(PositionalBinding=$false)]

Param(
    [switch] $noConfigure = $false,
    [switch] $noBuild = $false,
    [switch] $install = $false,
    [switch] $pack = $false,
    [switch] $tagVersion = $false,
    [switch] $clean = $false,
    [string] $vsDir = $null
)

$sourceDir = $(Get-Location)
$buildDir = "$sourceDir/build"
$artifactsDir = "$sourceDir/artifacts"

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

if ($vsDir -eq $null -or $vsDir -eq "") {
    if (-not (Get-Command "vswhere" -ErrorAction SilentlyContinue)) {
        Write-Host "Either install vswhere or set the -vsDir option"
        exit 1
    }
    $vsDir = & vswhere -property installationPath
}

# Urgh.
# https://stackoverflow.com/questions/2124753/how-can-i-use-powershell-with-the-visual-studio-command-prompt
cmd /c """$vsDir\VC\Auxiliary\Build\vcvars64.bat"" & set" | foreach {
    if ($_ -match "=") {
        $v = $_.split("="); set-item -force -path "ENV:\$($v[0])" -value "$($v[1])"
    }
}

if (-not $noConfigure) {
    Write-Host "Configuring vku"
    cmake -S $sourceDir -B $buildDir -DCMAKE_INSTALL_PREFIX=$artifactsDir -DCMAKE_BUILD_TYPE=Release -G Ninja
    if (-not $?) {
        exit 1
    }
}

if (-not $noBuild) {
    Write-Host "Building vku"
    cmake --build $buildDir
    if (-not $?) {
        exit 1
    }
}

if ($install) {
    Write-Host "Installing vku"
    cmake --build $buildDir --target install
    if (-not $?) {
        exit 1
    }
}

if ($pack) {
    [string[]] $packArgs = "-property:PackageOutputPath=""$artifactsDir"""
    $packArgs += "-property:Configuration=Release"
    if ($tagVersion) {
        $version = & git describe --tags --abbrev=0
        $version = $version.Trim('v')
    }
    if ($version -ne $null) {
        $packArgs += "-property:Version=""$version"""
    }
    Write-Host "Packing helveg"
    dotnet msbuild "$sourceDir" `
        -target:Restore,Build,Pack `
        $packArgs
    if (-not $?) {
        exit 1
    }
}

