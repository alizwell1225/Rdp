param(
    [int]$TargetProjectId = 11
)

Write-Host "Target project = $TargetProjectId"

# 專案根目錄 = 目前位置 (ci) 的上一層
$rootPath = (Resolve-Path "..").Path
Write-Host "Root path = $rootPath"

# 從根目錄往下找所有 LIB_*.csproj
$projects = Get-ChildItem -Path $rootPath -Recurse -Filter "LIB_*.csproj"

if ($projects.Count -eq 0) {
    Write-Host "No LIB_*.csproj found under $rootPath."
    exit 0
}

# 在根目錄底下建立 nupkgs 資料夾
$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null

foreach ($p in $projects) {
    $projPath = $p.FullName
    $name     = [System.IO.Path]::GetFileNameWithoutExtension($p.Name)

    Write-Host "== Process $name ($projPath) =="

    dotnet restore "$projPath"
    dotnet build  -c Release --no-restore "$projPath"
    dotnet pack   -c Release --no-build "$projPath" -o "$nupkgsPath"

    $pkg = Get-ChildItem (Join-Path $nupkgsPath "$name.*.nupkg") | Select-Object -First 1
    if ($pkg -ne $null) {
        Write-Host "Push package $($pkg.FullName)"

        $base   = $env:CI_API_V4_URL
        $source = "$base/projects/$TargetProjectId/packages/nuget/index.json"

        # 用 CI 變數（建議）
        $apiKey = $env:NUGET_API_KEY   # 如要寫死就改成 'glpat-xxxxxxx'

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
