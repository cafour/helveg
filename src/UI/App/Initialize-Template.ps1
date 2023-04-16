$appDir = "$PSScriptRoot"
$cliDir = Resolve-Path "$PSScriptRoot\..\..\CommandLine"
$repoDir = Resolve-Path "$PSScriptRoot\..\..\.."

# A: Kolik regenerací je potřeba na napsání jednoho software visualization toolu?
# N: 14.

Remove-Item -Recurse -Force "$appDir\template" -ErrorAction 'SilentlyContinue'
dotnet build "$cliDir"
echo "$cliDir $repoDir"
dotnet run "$repoDir\Helveg.sln" `
    --no-build `
    --project "$cliDir" `
    -- `
    --outdir "$appDir/template" `
    -pa PublicApi `
    -ea AssembliesOnly `
    -m StaticApp `
    --verbose `
    --icondir "" `
    --styledir "" `
    --scriptdir "" `
