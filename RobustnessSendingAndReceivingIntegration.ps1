Start-Sleep -Seconds 45

Write-Host "Disable"

Disable-NetAdapter "Ethernet 4" -Confirm:$false

Start-Sleep -Seconds 10

Write-Host "Enable"

Enable-NetAdapter "Ethernet 4" -Confirm:$false
