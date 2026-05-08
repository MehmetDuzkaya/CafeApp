param(
  [string]$Runtime = "win-x64",
  [string]$Configuration = "Release",
  [string]$OutputRoot = "dist"
)

$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "CafeApp.csproj"
if (!(Test-Path $projectPath)) {
  Write-Error "CafeApp.csproj not found in $PSScriptRoot"
  exit 1
}

$publishDir = Join-Path $PSScriptRoot "bin\$Configuration\net8.0-windows\$Runtime\publish"
$distDir = Join-Path $PSScriptRoot "$OutputRoot\CafeApp-$Runtime"

Write-Host "Publishing..."

dotnet publish $projectPath -c $Configuration -r $Runtime --self-contained true `
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if (!(Test-Path $publishDir)) {
  Write-Error "Publish output not found: $publishDir"
  exit 1
}

New-Item -ItemType Directory -Force -Path $distDir | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $distDir -Recurse -Force

Write-Host "Ready: $distDir"
