param(
    [int]$TargetProjectId = 11
)

Write-Host "Target project = $TargetProjectId"

$projects = Get-ChildItem -Path . -Recurse -Filter "LIB_*.csproj"

if ($projects.Count -eq 0) {
    Write-Host "No LIB_*.csproj found."
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

        # 這裡可以：
        # 1) 用 CI 變數：$apiKey = $env:NUGET_API_KEY
        # 2) 或暫時寫死：
        $apiKey = $env:NUGET_API_KEY   # 如果你想寫死，就改成 'glpat-xxxxxxx'

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
