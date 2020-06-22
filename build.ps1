$sourceDir = $(Get-Location)
$buildDir = "$sourceDir/build"
$artifactsDir = "$sourceDir/artifacts"

if (Test-Path $buildDir) {
    Remove-Item -Recurse -Force -Path $buildDir
}
New-Item $buildDir -ItemType "directory"

if (Test-Path $artifactsDir) {
    Remove-Item -Recurse -Force -Path $artifactsDir
}
New-Item $artifactsDir -ItemType "directory"

cmake -S $sourceDir -B $buildDir -DCMAKE_INSTALL_PREFIX=$artifactsDir
cmake --build $buildDir --config Release
cmake --build $buildDir --target install --config Release
