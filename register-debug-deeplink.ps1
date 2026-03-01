$Protocol = "sparktrack-debug"
$AppName = "SparkTrack-Debug"

# Получаем путь к папке, где лежит скрипт
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$appPath = Join-Path $scriptPath "Redist\Frontend\net10.0\SparkTrack.Desktop.exe"

Write-Host "Registering deeplink protocol: $Protocol" -ForegroundColor Green
Write-Host "Application path: $appPath" -ForegroundColor Gray

# Регистрируем в реестре текущего пользователя (не требует прав администратора)
$regPath = "HKCU:\SOFTWARE\Classes\$Protocol"

try {
    # Создаем ключи
    New-Item -Path $regPath -Force | Out-Null
    Set-ItemProperty -Path $regPath -Name "(Default)" -Value "URL:$AppName Protocol"
    Set-ItemProperty -Path $regPath -Name "URL Protocol" -Value ""
    
    # Создаем команду запуска
    $commandPath = "$regPath\shell\open\command"
    New-Item -Path $commandPath -Force | Out-Null
    Set-ItemProperty -Path $commandPath -Name "(Default)" -Value "`"$appPath`" `"%1`""
    
    # Дополнительно регистрируем для браузеров (Chrome, Edge)
    $browserRegPath = "HKCU:\SOFTWARE\Microsoft\Windows\Shell\Associations\UrlAssociations\$Protocol\UserChoice"
    New-Item -Path $browserRegPath -Force -ErrorAction SilentlyContinue | Out-Null
    Set-ItemProperty -Path $browserRegPath -Name "Progid" -Value $Protocol -ErrorAction SilentlyContinue
    
    Write-Host "✓ Protocol $Protocol successfully registered for current user!" -ForegroundColor Green
    
    # Проверяем регистрацию
    $registeredPath = Get-ItemProperty -Path "HKCU:\SOFTWARE\Classes\$Protocol\shell\open\command" -Name "(Default)" -ErrorAction SilentlyContinue
    if ($registeredPath) {
        Write-Host "Registration verified: $($registeredPath.'(Default)')" -ForegroundColor Gray
    }
}
catch {
    Write-Host "Error during registration: $_" -ForegroundColor Red
}