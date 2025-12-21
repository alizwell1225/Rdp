param(
    [int]$TargetProjectId = 11
)

Write-Host "Target project = $TargetProjectId"

# 1. 專案根目錄（GitLab 目前工作目錄）====================================
$rootPath = Get-Location
Write-Host "Root path = $rootPath"

# 2. 找出所有 LIB_*.csproj ==============================================
$projects = Get-ChildItem -Path $rootPath -Recurse -Filter "*.csproj"

if ($projects.Count -eq 0) {
    Write-Host "No *.csproj found under $rootPath."
    exit 0
}

Write-Host "Found projects:"
$projects | ForEach-Object {
    Write-Host "  - $($_.FullName)"
}

# 3. 在根目錄底下建立 nupkgs ==========================================
$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Write-Host "nupkgs path = $nupkgsPath"

# 4. 逐一還原 / 建置 / 打包 / Push ====================================
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
        $apiKey = $env:NUGET_API_KEY   # 在 GitLab CI 變數設定

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
