param(
    [int]$TargetProjectId = 11
)

Write-Host "Target project = $TargetProjectId"

# 根目錄 = 目前路徑
$rootPath = Get-Location
Write-Host "Root path = $rootPath"

$projects = Get-ChildItem -Path $rootPath -Recurse -Filter "*.csproj"
if ($projects.Count -eq 0) {
    Write-Host "*.csproj found under $rootPath."
    exit 0
}

$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Write-Host "nupkgs path = $nupkgsPath"

foreach ($p in $projects) {
    $projPath = $p.FullName
    $name     = [System.IO.Path]::GetFileNameWithoutExtension($p.Name)

    Write-Host "== Process $name ($projPath) =="

    dotnet restore "$projPath"
    dotnet build  -c Release --no-restore "$projPath"
    dotnet pack   -c Release --no-build "$projPath" -o "$nupkgsPath"

    $pattern = Join-Path $nupkgsPath "$name.*.nupkg"
    $pkg = Get-ChildItem $pattern | Select-Object -First 1
    if ($pkg -ne $null) {
        Write-Host "Push package $($pkg.FullName)"

        $base   = $env:CI_API_V4_URL
        $source = "$base/projects/$TargetProjectId/packages/nuget/index.json"
        $apiKey = $env:NUGET_API_KEY

        Write-Host ("HAS_API_KEY=" + ([string]::IsNullOrEmpty($apiKey) -eq $false))

        dotnet nuget push "$($pkg.FullName)" `
            --source "$source" `
            --api-key "$apiKey" `
            --skip-duplicate
    }
    else {
        Write-Host "No package found for $name"
    }
}
