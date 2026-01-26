# Installation script for Claude Launcher
# Run: powershell -ExecutionPolicy Bypass -File install.ps1

param(
    [switch]$NoShortcut,
    [string]$InstallPath = "$env:LOCALAPPDATA\ClaudeLauncher"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Claude Launcher Installation ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build the application
Write-Host "[1/4] Building application..." -ForegroundColor Yellow
$projectPath = Join-Path $PSScriptRoot "src\ClaudeLauncher\ClaudeLauncher.csproj"

if (-not (Test-Path $projectPath)) {
    Write-Host "Error: Project file not found at $projectPath" -ForegroundColor Red
    exit 1
}

# Publish as self-contained for portability
dotnet publish $projectPath -c Release -r win-x64 --self-contained false -o "$InstallPath" | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "  Built to: $InstallPath" -ForegroundColor Green

# Step 2: Create config directories
Write-Host "[2/4] Creating config directories..." -ForegroundColor Yellow
$configDir = "$env:USERPROFILE\.claude-launcher"
$profilesDir = "$configDir\profiles"
$backupsDir = "$configDir\backups"

New-Item -ItemType Directory -Path $profilesDir -Force | Out-Null
New-Item -ItemType Directory -Path $backupsDir -Force | Out-Null

Write-Host "  Config directory: $configDir" -ForegroundColor Green

# Step 3: Create default profiles
Write-Host "[3/4] Creating default profiles..." -ForegroundColor Yellow

$profiles = @{
    "magic-analysis" = @{
        name = "magic-analysis"
        description = "Analyse et migration Magic Unipaas"
        mcpServers = @("context7")
        rules = @("dotnet", "sql-server", "magic-analysis")
        openSpecMode = "full"
        skipTickets = $false
    }
    "dev" = @{
        name = "dev"
        description = "Developpement outils (MCP, scripts)"
        mcpServers = @("context7")
        rules = @("typescript", "testing", "dotnet")
        openSpecMode = "minimal"
        skipTickets = $true
    }
    "web" = @{
        name = "web"
        description = "Applications web (Next.js, React)"
        mcpServers = @("context7", "eslint")
        rules = @("typescript", "react", "testing")
        openSpecMode = "standard"
        skipTickets = $true
    }
    "business" = @{
        name = "business"
        description = "Analyses business et plans"
        mcpServers = @()
        rules = @("business")
        openSpecMode = "minimal"
        skipTickets = $true
    }
}

foreach ($profile in $profiles.GetEnumerator()) {
    $profilePath = Join-Path $profilesDir "$($profile.Key).json"
    $profile.Value | ConvertTo-Json -Depth 5 | Out-File -FilePath $profilePath -Encoding UTF8
    Write-Host "  Created: $($profile.Key).json" -ForegroundColor Gray
}

# Step 4: Create desktop shortcut
if (-not $NoShortcut) {
    Write-Host "[4/4] Creating desktop shortcut..." -ForegroundColor Yellow

    $exePath = Join-Path $InstallPath "ClaudeLauncher.exe"
    $shortcutPath = Join-Path ([Environment]::GetFolderPath("Desktop")) "Claude Launcher.lnk"

    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $exePath
    $shortcut.WorkingDirectory = $InstallPath
    $shortcut.Description = "Lanceur interactif pour Claude Code"
    $shortcut.Save()

    Write-Host "  Shortcut created on Desktop" -ForegroundColor Green
} else {
    Write-Host "[4/4] Skipping desktop shortcut (--NoShortcut)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Installation Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Executable: $InstallPath\ClaudeLauncher.exe"
Write-Host "Config: $configDir"
Write-Host ""
Write-Host "You can now:" -ForegroundColor Green
Write-Host "  1. Double-click 'Claude Launcher' on your Desktop"
Write-Host "  2. Run ClaudeLauncher.exe directly"
Write-Host "  3. Pin it to your taskbar"
Write-Host ""
