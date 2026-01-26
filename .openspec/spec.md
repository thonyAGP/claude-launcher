# Lanceur_Claude - OpenSpec

> Documentation generee le 2026-01-25

## Vue d'ensemble

Application Windows WinForms pour lancer Claude Code avec gestion de profils et contexte optimise. Permet de reduire le contexte charge selon le type de projet (Magic, Dev, Web, Business).

## Architecture

| Composant | Technologie | Role |
|-----------|-------------|------|
| Frontend | C# WinForms .NET 8 | Interface graphique |
| Config | JSON | Stockage profils et projets |
| Integration | PowerShell hooks | Session-start existant |

### Structure

```
src/ClaudeLauncher/
├── Program.cs           # Point d'entree
├── MainForm.cs          # Interface principale
├── Models/
│   ├── Project.cs       # Projet recent
│   ├── Profile.cs       # Profil de contexte
│   └── LauncherConfig.cs # Config launcher
└── Services/
    ├── ConfigService.cs    # Gestion config/profils
    ├── SettingsService.cs  # Modif settings.json
    └── LaunchService.cs    # Lancement Claude
```

## Fonctionnalites

### Implementees (v1.0)
- [x] Interface WinForms avec liste projets recents
- [x] Selection/detection automatique de profil
- [x] Checkboxes MCP servers
- [x] Option dangerous skip permissions
- [x] Backup/restore settings.json
- [x] Creation .claude-launcher.json par projet
- [x] Script d'installation avec raccourci bureau

### A faire (v1.1)
- [ ] Icone personnalisee
- [ ] Theme sombre
- [ ] Raccourci clavier global
- [ ] Historique des sessions

## Taches

### A traiter

### En cours

### Terminees
- [x] Structure projet C# WinForms
- [x] Modeles (Project, Profile, LauncherConfig)
- [x] Services (Config, Settings, Launch)
- [x] Interface MainForm
- [x] Script installation
- [x] Documentation README

## Plans

### Plan actuel
[Projet MVP termine - en attente de tests utilisateur]

### Historique des plans

## Decisions

| Date | Decision | Contexte | Alternatives rejetees |
|------|----------|----------|----------------------|
| 2026-01-25 | C# WinForms | Interface native Windows demandee | PowerShell TUI, WPF, Electron |
| 2026-01-25 | Integration hook existant | session-start.ps1 detecte deja le mode | Systeme independant |
| 2026-01-25 | Backup/restore settings.json | Isolation par projet demandee | Modification globale persistante |

---

## Preferences Projet

| Preference | Valeur | Raison |
|------------|--------|--------|
| Framework | .NET 8 WinForms | LTS, simple, natif Windows |
| Config | JSON | Compatible avec settings.json Claude |

## A Retenir

- Le hook session-start.ps1 lit deja .claude-launcher.json pour le mode DEV
- Les MCP servers sont lus au demarrage de Claude, donc settings.json doit etre modifie AVANT le lancement
- Restauration settings.json apres 5 secondes (Claude a deja lu la config)

## Contexte Important

- Installation: `powershell -ExecutionPolicy Bypass -File install.ps1`
- Executable: `%LOCALAPPDATA%\ClaudeLauncher\ClaudeLauncher.exe`
- Config: `%USERPROFILE%\.claude-launcher\`

---

## Changelog
- 2026-01-25 : MVP complete - WinForms, profils, installation
