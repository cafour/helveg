param(
    [string]$source=$null,
    [string]$framework="net6.0"
)

$appDir = "$PSScriptRoot"
$cliDir = Resolve-Path "$PSScriptRoot\..\..\CommandLine"
$repoDir = Resolve-Path "$PSScriptRoot\..\..\.."
if ([string]::IsNullOrEmpty($source)) {
    $source = "$repoDir\Helveg.sln"
}

# A: Kolik regenerací je potřeba na napsání jednoho software visualization toolu?
# N: 14.

Remove-Item -Recurse -Force "$appDir\template" -ErrorAction 'SilentlyContinue'
dotnet build "$cliDir"
echo "Running Helveg.CommandLine on '$source'."
dotnet run --no-build --project "$cliDir" `
    -- `
    "$source" `
    --outdir "$appDir/template" `
    -pa Explicit `
    -ea WithoutSymbols `
    -m StaticApp `
    --no-restore `
    --verbose `
    --icondir "." `
    --styledir "." `
    --scriptdir "." `
    -p "TargetFramework=$framework"
