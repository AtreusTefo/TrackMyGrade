# Start the TrackMyGrade API using IIS Express on port 5000
$iisExpressPath = "C:\Program Files\IIS Express\iisexpress.exe"
$projectPath = $PSScriptRoot

if (-not (Test-Path $iisExpressPath)) {
    Write-Error "IIS Express not found at: $iisExpressPath"
    exit 1
}

Write-Host "Starting TrackMyGrade API on http://localhost:5000 ..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop." -ForegroundColor Yellow

& $iisExpressPath /path:"$projectPath" /port:5000 /clr:v4.0
