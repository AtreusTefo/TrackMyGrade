$ErrorActionPreference = "Continue"
$body = '{"firstName":"John","lastName":"Doe","email":"john@example.com","phone":"12345678","subject":"Mathematics","password":"pass123"}'
try {
    $resp = Invoke-WebRequest -UseBasicParsing -Uri "http://localhost:5000/api/teachers/register" -Method POST -ContentType "application/json" -Body $body
    Write-Host "REGISTER OK ($($resp.StatusCode)): $($resp.Content)"
} catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $raw = $reader.ReadToEnd()
    Write-Host "REGISTER HTTP $([int]$_.Exception.Response.StatusCode):"
    Write-Host ($raw | Select-String -Pattern "error|exception|Error|Exception|message" -SimpleMatch | Select-Object -First 20)
    Write-Host "---RAW (first 800 chars)---"
    Write-Host $raw.Substring(0, [Math]::Min(800, $raw.Length))
}
