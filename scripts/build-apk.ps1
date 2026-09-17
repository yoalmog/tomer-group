# Tomer Group - Local Android APK Build Script
# Prerequisites: .NET 8 SDK, maui-android workload, Java JDK 17, and Android SDK.

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  TOMER GROUP - ANDROID APK PACKAGING TOOL" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

$projectPath = Join-Path $PSScriptRoot "..\src\TomerGroup.Mobile\TomerGroup.Mobile.csproj"
$outputDir = Join-Path $PSScriptRoot "..\artifacts\apk"

if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

Write-Host "`n[1/3] Checking .NET MAUI Workloads..." -ForegroundColor Yellow
$workloads = dotnet workload list
if ($workloads -notmatch "maui-android") {
    Write-Warning "maui-android workload not detected. Attempting installation (requires Admin/Elevation)..."
    dotnet workload install maui-android
}

Write-Host "`n[2/3] Publishing Release APK for net8.0-android..." -ForegroundColor Yellow
dotnet publish "$projectPath" `
    -f net8.0-android `
    -c Release `
    /p:EnableMaui=true `
    /p:AndroidPackageFormats=apk `
    -o "$outputDir"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[3/3] BUILD SUCCEEDED!" -ForegroundColor Green
    Write-Host "APK output location: $outputDir" -ForegroundColor Green
    Get-ChildItem -Path $outputDir -Filter "*.apk" | ForEach-Object {
        Write-Host "  -> $($_.Name) ($([math]::Round($_.Length / 1MB, 2)) MB)" -ForegroundColor Green
    }
} else {
    Write-Host "`n[!] Build failed. If running outside Visual Studio, ensure Android SDK & JDK 17 are installed and configured in PATH/ANDROID_HOME." -ForegroundColor Red
}

