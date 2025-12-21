param(
    [int]$TargetProjectId = 11,
    [string[]]$ProjectNames = @()   # 直接從 yml 傳 LIB_RPC,LIB_RDP...
)

Write-Host "Target project = $TargetProjectId"
Write-Host "Raw ProjectNames = $($ProjectNames -join ', ')"

# ===== 1. 專案根目錄 = 目前路徑 =====
$rootPath = Get-Location
Write-Host "Root path = $rootPath"

# ===== 2. 找出所有 LIB_*.csproj =====
$allProjects = Get-ChildItem -Path $rootPath -Recurse -Filter "LIB_*.csproj"
if ($allProjects.Count -eq 0) {
    Write-Host "No LIB_*.csproj found under $rootPath."
    exit 0
}

Write-Host "Found projects:"
$allProjects | ForEach-Object {
    Write-Host "  - $($_.FullName)"
}

# ===== 3. 依名稱過濾（用 -in，比較直覺）=====
if ($ProjectNames.Count -gt 0) {
    # 轉成純字串陣列（防止前後有引號）
    $normalized = $ProjectNames | ForEach-Object { $_.ToString().Trim('"') }
    Write-Host "Normalized names = $($normalized -join ', ')"

    $projects = $allProjects | Where-Object {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
        $name -in $normalized
    }
}
else {
    $projects = $allProjects
}

if ($projects.Count -eq 0) {
    Write-Host "No matched project for given names."
    exit 0
}

Write-Host "Matched projects:"
$projects | ForEach-Object {
    Write-Host "  * $($_.FullName)"
}

# ===== 4. nupkgs 放在根目錄 =====
$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Write-Host "nupkgs path = $nupkgsPath"

# ===== 5. 還原 / 建置 / 打包 / Push =====
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
