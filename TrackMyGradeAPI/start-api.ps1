# Start the TrackMyGrade API (OWIN Self-Hosted) on port 5000
$apiExePath = Join-Path $PSScriptRoot "bin\TrackMyGradeAPI.exe"

if (-not (Test-Path $apiExePath)) {
    Write-Host "API executable not found. Building project..." -ForegroundColor Yellow
    $csprojPath = Join-Path $PSScriptRoot "TrackMyGradeAPI.csproj"

    if (Get-Command msbuild -ErrorAction SilentlyContinue) {
        msbuild $csprojPath /p:Configuration=Debug /v:minimal
    } else {
        Write-Error "API executable not found at: $apiExePath"
        Write-Error "Please build the project in Visual Studio first."
        exit 1
    }
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  TrackMyGrade API - OWIN Self-Hosted" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Starting API on http://localhost:5000 ..." -ForegroundColor Green
Write-Host "Swagger UI: http://localhost:5000/swagger" -ForegroundColor Gray
Write-Host "Press Ctrl+C to stop." -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

& $apiExePath
