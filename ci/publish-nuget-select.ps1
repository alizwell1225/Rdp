param(
    [int]$TargetProjectId = 11,
    [string]$ProjectNames = ""      # 注意：先當成一條字串處理
)

Write-Host "Target project = $TargetProjectId"
Write-Host "Raw ProjectNames = $ProjectNames"

# 1. 專案根目錄（GitLab 目前工作目錄）
$rootPath = Get-Location
Write-Host "Root path = $rootPath"

# 2. 把逗號分隔字串切成名稱陣列
$names = @()

if (![string]::IsNullOrWhiteSpace($ProjectNames)) {
    $names = $ProjectNames.Split(",") |
             ForEach-Object { $_.Trim() } |
             Where-Object { $_ -ne "" }
}

if ($names.Count -eq 0) {
    Write-Host "No valid project names after split."
    exit 0
}

Write-Host "Parsed names:"
$names | ForEach-Object { Write-Host "  - $_" }

# 3. 依名稱組出每個 .csproj 路徑
$projects = @()

foreach ($name in $names) {
    # 假設結構是 <root>\LIB_RPC\LIB_RPC.csproj
    $projPath = Join-Path $rootPath "$name\$name.csproj"

    if (Test-Path $projPath) {
        $item = Get-Item $projPath
        $projects += $item
        Write-Host "Add project: $($item.FullName)"
    }
    else {
        Write-Host "Skip $name (project not found: $projPath)"
    }
}

if ($projects.Count -eq 0) {
    Write-Host "No valid project for given names."
    exit 0
}

# 4. 在根目錄底下建立 nupkgs
$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Write-Host "nupkgs path = $nupkgsPath"

# 5. 還原 / 建置 / 打包 / Push
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
