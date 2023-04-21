$appDir = "$PSScriptRoot"
$cliDir = Resolve-Path "$PSScriptRoot\..\..\CommandLine"
$repoDir = Resolve-Path "$PSScriptRoot\..\..\.."

# A: Kolik regenerací je potřeba na napsání jednoho software visualization toolu?
# N: 14.

Remove-Item -Recurse -Force "$appDir\template" -ErrorAction 'SilentlyContinue'
dotnet build "$cliDir"
echo "$cliDir $repoDir"
dotnet run --no-build --project "$cliDir" `
    -- `
    "$repoDir\Helveg.sln" `
    --outdir "$appDir/template" `
    -pa PublicApi `
    -ea None `
    -m StaticApp `
    --verbose `
    --icondir "." `
    --styledir "." `
    --scriptdir "." `
    -p "TargetFramework=net6.0"
