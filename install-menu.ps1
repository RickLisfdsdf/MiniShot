# 把「粘贴截图」装进 Win11 一级右键菜单（需要开启开发者模式）
# 用法：右键本文件 → 使用 PowerShell 运行；加 -Uninstall 卸载
param([switch]$Uninstall)
$ErrorActionPreference = 'Stop'
$dir = $PSScriptRoot

Get-AppxPackage RickT.MiniShot | Remove-AppxPackage
if ($Uninstall) { Write-Host '已卸载 Win11 右键菜单'; return }

$dev = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock' -ErrorAction SilentlyContinue
if (-not $dev -or $dev.AllowDevelopmentWithoutDevLicense -ne 1) {
    Write-Host '需要先开启开发者模式：设置 → 系统 → 开发者选项 → 开发人员模式' -ForegroundColor Yellow
    Start-Process 'ms-settings:developers'
    return
}
Add-AppxPackage -Register (Join-Path $dir 'AppxManifest.xml') -ExternalLocation $dir
Write-Host '已安装。在桌面或文件夹空白处右键即可看到「粘贴截图」（没出现就重启一下资源管理器）' -ForegroundColor Green
