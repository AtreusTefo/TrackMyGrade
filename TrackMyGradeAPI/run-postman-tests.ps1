# TrackMyGrade API - Postman Collection Test Runner
# This script helps you run the Postman collection using Newman (Postman's CLI)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  TrackMyGrade API - Postman Test Runner" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Newman is installed
$newmanInstalled = Get-Command newman -ErrorAction SilentlyContinue

if (-not $newmanInstalled) {
    Write-Host "Newman (Postman CLI) is not installed." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To install Newman globally:" -ForegroundColor White
    Write-Host "  npm install -g newman" -ForegroundColor Green
    Write-Host ""
    Write-Host "Alternative: Use Postman Desktop Application" -ForegroundColor White
    Write-Host "  1. Import: TrackMyGradeAPI.postman_collection.json" -ForegroundColor Gray
    Write-Host "  2. Import: TrackMyGradeAPI.postman_environment.json" -ForegroundColor Gray
    Write-Host "  3. Run the collection manually" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "✓ Newman is installed" -ForegroundColor Green

# Check if API is running
Write-Host "Checking if API is running on http://localhost:5000..." -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
    Write-Host "✓ API is running" -ForegroundColor Green
} catch {
    Write-Host "✗ API is not responding on http://localhost:5000" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please start the API first:" -ForegroundColor Yellow
    Write-Host "  .\start-api.ps1" -ForegroundColor Green
    Write-Host "  or" -ForegroundColor Gray
    Write-Host "  .\TrackMyGradeAPI.exe" -ForegroundColor Green
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "Running Postman Collection Tests..." -ForegroundColor Cyan
Write-Host "-----------------------------------" -ForegroundColor Gray
Write-Host ""

# Run Newman with the collection and environment
newman run TrackMyGradeAPI.postman_collection.json `
    -e TrackMyGradeAPI.postman_environment.json `
    --reporters cli,json `
    --reporter-json-export newman-results.json `
    --color on

Write-Host ""
Write-Host "-----------------------------------" -ForegroundColor Gray

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "✗ Some tests failed. Check the output above." -ForegroundColor Red
}

Write-Host ""
Write-Host "Test results saved to: newman-results.json" -ForegroundColor Cyan
Write-Host ""
