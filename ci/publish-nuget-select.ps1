param(
    [int]$TargetProjectId = 11,
    [string[]]$ProjectNames = @()   # "LIB_RPC","LIB_RDP","LIB_Machine","LIB_Log"
)

Write-Host "Target project = $TargetProjectId"
Write-Host "Selected projects = $($ProjectNames -join ', ')"

# ===== 1. 找出包含解決方案的實際專案根目錄 =====
$workspace = Get-Location
Write-Host "Workspace = $workspace"

# 這裡請把 RDP_SDK.sln 改成你實際的 sln 名稱
$solution = Get-ChildItem -Path $workspace -Recurse -Filter "RDP_SDK.sln" | Select-Object -First 1
if ($null -eq $solution) {
    Write-Host "No RDP_SDK.sln found under $workspace."
    exit 0
}

$rootPath = $solution.Directory.FullName
Write-Host "Root path (solution folder) = $rootPath"

# ===== 2. 找出所有 LIB_*.csproj =====
$allProjects = Get-ChildItem -Path $rootPath -Recurse -Filter "LIB_*.csproj"
if ($allProjects.Count -eq 0) {
    Write-Host "No LIB_*.csproj found under $rootPath."
    exit 0
}

if ($ProjectNames.Count -gt 0) {
    $projects = $allProjects | Where-Object {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
        $ProjectNames -contains $name
    }
}
else {
    $projects = $allProjects
}

if ($projects.Count -eq 0) {
    Write-Host "No matched project for given names."
    exit 0
}

# ===== 3. nupkgs 放在 sln 同層 =====
$nupkgsPath = Join-Path $rootPath "nupkgs"
New-Item -ItemType Directory -Force -Path $nupkgsPath | Out-Null
Write-Host "nupkgs path = $nupkgsPath"

# ===== 4. 還原 / 建置 / 打包 / Push =====
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
