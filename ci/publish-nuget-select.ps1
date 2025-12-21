param(
    [int]$TargetProjectId = 11,
    [string[]]$ProjectNames = @()   # 要輸出的專案名稱（不含副檔名），例如 @('LIB_HTool','LIB_HWin32')
)

Write-Host "Target project = $TargetProjectId"
Write-Host "Selected projects = $($ProjectNames -join ', ')"

# 1. 找出所有符合條件的 .csproj
$allProjects = Get-ChildItem -Path . -Recurse -Filter "LIB_*.csproj"

if ($allProjects.Count -eq 0) {
    Write-Host "No LIB_*.csproj found."
    exit 0
}

# 如果有指定 ProjectNames，就只挑那幾個；沒指定就全部 LIB_*
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

New-Item -ItemType Directory -Force -Path "./nupkgs" | Out-Null

foreach ($p in $projects) {
    $projPath = $p.FullName
    $name     = [System.IO.Path]::GetFileNameWithoutExtension($p.Name)

    Write-Host "== Process $name ($projPath) =="

    dotnet restore "$projPath"
    dotnet build -c Release --no-restore "$projPath"
    dotnet pack  -c Release --no-build "$projPath" -o "./nupkgs"

    $pkg = Get-ChildItem "./nupkgs/$name.*.nupkg" | Select-Object -First 1
    if ($pkg -ne $null) {
        Write-Host "Push package $($pkg.FullName)"

        $base   = $env:CI_API_V4_URL
        $source = "$base/projects/$TargetProjectId/packages/nuget/index.json"
        $apiKey = $env:NUGET_API_KEY   # 或直接寫死 'glpat-xxxx'

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
