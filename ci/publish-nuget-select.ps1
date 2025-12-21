param(
    [int]$TargetProjectId = 11,
    [string[]]$ProjectNames = @()   # 例如：LIB_RPC,LIB_RDP,LIB_Machine,LIB_Log
)

Write-Host "Target project = $TargetProjectId"
Write-Host "ProjectNames   = $($ProjectNames -join ', ')"

# 1. 專案根目錄（GitLab 目前工作目錄）====================================
$rootPath = Get-Location
Write-Host "Root path = $rootPath"

if ($ProjectNames.Count -eq 0) {
    Write-Host "No ProjectNames specified. Nothing to do."
    exit 0
}

# 2. 依名稱組出每個 .csproj 路徑 ======================================
$projects = @()

foreach ($name in $ProjectNames) {
    $trimName = $name.ToString().Trim('"').Trim()

    # 組成像：<root>\LIB_RPC\LIB_RPC.csproj
    $projPath = Join-Path $rootPath "$trimName\$trimName.csproj"

    if (Test-Path $projPath) {
        $item = Get-Item $projPath
        $projects += $item
        Write-Host "Add project: $($item.FullName)"
    }
    else {
        Write-Host "Skip $trimName (project not found: $projPath)"
    }
}

if ($projects.Count -eq 0) {
    Write-Host "No valid project for given names."
    exit 0
}

# 3. 在根目錄底下建立 nupkgs ==========================================
$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Write-Host "nupkgs path = $nupkgsPath"

# 4. 還原 / 建置 / 打包 / Push =========================================
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
