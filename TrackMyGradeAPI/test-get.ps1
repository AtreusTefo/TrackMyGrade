$req = [System.Net.WebRequest]::Create("http://localhost:5000/api/students")
$req.Method = "GET"
$req.Headers.Add("X-TeacherId", "1")
try {
    $resp = $req.GetResponse()
    $body = (New-Object System.IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
    Write-Host "200 OK: $body"
} catch [System.Net.WebException] {
    $errResp = $_.Exception.Response
    if ($errResp) {
        $body = (New-Object System.IO.StreamReader($errResp.GetResponseStream())).ReadToEnd()
        Write-Host "HTTP $([int]$errResp.StatusCode): $body"
    } else {
        Write-Host "No response: $($_.Exception.Message)"
    }
}
