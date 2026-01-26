$exePath = "C:\Users\thony\AppData\Local\ClaudeLauncher\ClaudeLauncher.exe"
$shortcutPath = [Environment]::GetFolderPath("Desktop") + "\Claude Launcher.lnk"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = Split-Path $exePath
$shortcut.IconLocation = "$exePath,0"
$shortcut.Description = "Lanceur interactif pour Claude Code"
$shortcut.Save()

Write-Host "Shortcut updated with icon"
