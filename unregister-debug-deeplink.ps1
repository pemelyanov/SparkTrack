$Protocol = "sparktrack-debug"
$regPath = "HKCU:\SOFTWARE\Classes\$Protocol"

try {
    if (Test-Path $regPath) {
        Remove-Item -Path $regPath -Recurse -Force
        Write-Host "✓ Protocol $Protocol unregistered successfully!" -ForegroundColor Green
    } else {
        Write-Host "Protocol $Protocol is not registered" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Error during unregistration: $_" -ForegroundColor Red
}