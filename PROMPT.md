# Claude Launcher - Specification Prompt

> Prompt complet pour creer un lanceur interactif Claude Code avec gestion de profils.

---

## Contexte

L'utilisateur travaille avec Claude Code sur plusieurs projets de natures differentes:
- **Projets Magic** : Analyse/migration d'applications Magic Unipaas (XML parsing, MCP servers)
- **Projets Dev** : Developpement d'outils (MCP servers C#, scripts PowerShell)
- **Projets Web** : Applications Next.js, APIs TypeScript
- **Projets Business** : Analyses et business plans

Chaque type de projet necessite un contexte different:
- Differents MCP servers actifs
- Differentes regles chargees
- Differents niveaux de detail dans OpenSpec

Actuellement, TOUT le contexte est charge a chaque session (~104k tokens), reduisant la capacite de traitement.

---

## Objectif

Creer un **Lanceur Claude** qui:
1. Affiche une interface interactive (TUI ou CLI menu)
2. Permet de selectionner/creer un projet
3. Configure le contexte selon le type de projet
4. Lance Claude Code avec les bons parametres

---

## Fonctionnalites Requises

### 1. Selection de Projet

```
╔══════════════════════════════════════════╗
║           CLAUDE LAUNCHER v1.0           ║
╠══════════════════════════════════════════╣
║  PROJETS RECENTS:                        ║
║  [1] Lecteur_Magic (Magic Analysis)      ║
║  [2] MagicMcp (Dev Tools)                ║
║  [3] MonSite (Next.js)                   ║
║                                          ║
║  [N] Nouveau projet                      ║
║  [S] Settings                            ║
║  [Q] Quitter                             ║
╚══════════════════════════════════════════╝
```

### 2. Profils de Contexte

| Profil | MCP Servers | Rules Chargees | OpenSpec |
|--------|-------------|----------------|----------|
| **magic-analysis** | magic-interpreter, context7 | dotnet, sql-server | Complet + tickets |
| **dev** | context7 seulement | typescript, testing | Minimal |
| **web** | context7, eslint | react, typescript, testing | Standard |
| **business** | aucun | business.md | Minimal |

### 3. Gestion MCP Servers

```
╔══════════════════════════════════════════╗
║          MCP SERVERS CONFIG              ║
╠══════════════════════════════════════════╣
║  [x] context7        (documentation)     ║
║  [ ] magic-interpreter (Magic XML)       ║
║  [ ] memory-keeper   (persistence)       ║
║  [ ] postgres-mcp    (database)          ║
║  [ ] eslint          (linting)           ║
║                                          ║
║  [ENTER] Confirmer  [A] Toggle All       ║
╚══════════════════════════════════════════╝
```

### 4. Options de Lancement

```
╔══════════════════════════════════════════╗
║          OPTIONS DE LANCEMENT            ║
╠══════════════════════════════════════════╣
║  Mode:                                   ║
║  ( ) Normal                              ║
║  (x) Dangerously Skip Permissions        ║
║                                          ║
║  Contexte:                               ║
║  [x] Charger OpenSpec                    ║
║  [ ] Charger tickets Jira                ║
║  [x] Charger memoire globale             ║
║                                          ║
║  [ENTER] Lancer Claude                   ║
╚══════════════════════════════════════════╝
```

### 5. Configuration Projet

Chaque projet a un fichier `.claude-launcher.json`:

```json
{
  "profile": "dev",
  "mcpServers": ["context7"],
  "rules": ["typescript", "testing"],
  "openspec": "minimal",
  "skipTickets": true,
  "dangerousMode": true,
  "lastOpened": "2026-01-24T10:30:00Z"
}
```

---

## Architecture Technique

### Stack Recommandee

| Composant | Choix | Raison |
|-----------|-------|--------|
| Runtime | PowerShell 7+ | Natif Windows, cross-platform possible |
| TUI | PSMenu ou custom | Simple, pas de deps externes |
| Config | JSON | Facile a editer manuellement |
| Storage | `~/.claude-launcher/` | Central, portable |

### Structure Fichiers

```
~/.claude-launcher/
├── config.json           # Config globale (profils, defaults)
├── projects.json         # Liste projets avec metadata
├── profiles/
│   ├── magic-analysis.json
│   ├── dev.json
│   ├── web.json
│   └── business.json
└── cache/
    └── mcp-status.json   # Etat actuel des MCP
```

### Fichiers Projet (dans chaque projet)

```
{project}/
├── .claude-launcher.json  # Config specifique projet
└── .openspec/
    ├── spec.md            # Version complete
    └── spec.minimal.md    # Version reduite (auto-generee)
```

---

## Flux de Lancement

```
┌─────────────────────────────────────────────────────────────┐
│                      LANCEUR CLAUDE                         │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────────┐
              │   1. Selection Projet       │
              │   (recent ou nouveau)       │
              └─────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────────┐
              │   2. Charger Profil         │
              │   (depuis .claude-launcher) │
              └─────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────────┐
              │   3. Configurer MCP         │
              │   (activer/desactiver)      │
              └─────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────────┐
              │   4. Generer Contexte       │
              │   (OpenSpec minimal si dev) │
              └─────────────────────────────┘
                            │
                            ▼
              ┌─────────────────────────────┐
              │   5. Lancer Claude          │
              │   claude --dangerously-...  │
              └─────────────────────────────┘
```

---

## Implementation Details

### Desactivation MCP Servers

Pour desactiver un MCP server AVANT le lancement:

```powershell
# Lire settings.json actuel
$settings = Get-Content "$env:USERPROFILE\.claude\settings.json" | ConvertFrom-Json

# Modifier enabledMcpjsonServers selon profil
$settings.enabledMcpjsonServers = @("context7")  # Seulement ceux du profil

# Sauvegarder temporairement
$settings | ConvertTo-Json -Depth 10 | Set-Content "$env:USERPROFILE\.claude\settings.json"

# Lancer Claude
claude --dangerously-skip-permissions

# Restaurer settings.json original apres fermeture Claude
```

### Generation OpenSpec Minimal

Script pour generer `spec.minimal.md` depuis `spec.md`:

```powershell
function New-MinimalSpec {
    param([string]$SpecPath)

    $content = Get-Content $SpecPath -Raw

    # Garder: Vue d'ensemble, Architecture, Taches actuelles
    # Supprimer: Changelog, Fonctionnalites terminees, Plans historiques

    $sections = @(
        "## Vue d'ensemble",
        "## Architecture",
        "## Taches",
        "### A traiter",
        "### En cours"
    )

    # Extraire seulement les sections necessaires
    # ... implementation
}
```

### Detection Auto du Profil

```powershell
function Get-ProjectProfile {
    param([string]$ProjectPath)

    # Priorite 1: .claude-launcher.json
    $launcherConfig = Join-Path $ProjectPath ".claude-launcher.json"
    if (Test-Path $launcherConfig) {
        return (Get-Content $launcherConfig | ConvertFrom-Json).profile
    }

    # Priorite 2: Detection auto
    if (Test-Path (Join-Path $ProjectPath "skills\magic-unipaas")) {
        return "magic-analysis"
    }
    if (Test-Path (Join-Path $ProjectPath "tools\MagicMcp")) {
        return "dev"
    }
    if (Test-Path (Join-Path $ProjectPath "next.config.*")) {
        return "web"
    }

    return "dev"  # Default
}
```

---

## Profils Detailles

### Profil: magic-analysis

```json
{
  "name": "magic-analysis",
  "description": "Analyse et migration Magic Unipaas",
  "mcpServers": ["magic-interpreter", "context7"],
  "rules": ["dotnet", "sql-server"],
  "openspec": "full",
  "skipTickets": false,
  "sessionStartSections": ["openspec", "git", "tickets", "memory"]
}
```

### Profil: dev

```json
{
  "name": "dev",
  "description": "Developpement outils (MCP, scripts)",
  "mcpServers": ["context7"],
  "rules": ["typescript", "testing", "dotnet"],
  "openspec": "minimal",
  "skipTickets": true,
  "sessionStartSections": ["memory", "git"]
}
```

### Profil: web

```json
{
  "name": "web",
  "description": "Applications web (Next.js, React)",
  "mcpServers": ["context7", "eslint"],
  "rules": ["typescript", "react", "testing"],
  "openspec": "standard",
  "skipTickets": true,
  "sessionStartSections": ["memory", "git", "standards"]
}
```

### Profil: business

```json
{
  "name": "business",
  "description": "Analyses business et plans",
  "mcpServers": [],
  "rules": ["business"],
  "openspec": "minimal",
  "skipTickets": true,
  "sessionStartSections": ["memory"]
}
```

---

## Gains Attendus

| Profil | Tokens Avant | Tokens Apres | Reduction |
|--------|--------------|--------------|-----------|
| magic-analysis | 104k | 104k | 0% (complet) |
| dev | 104k | 75k | ~28% |
| web | 104k | 80k | ~23% |
| business | 104k | 65k | ~38% |

---

## MVP - Version 1.0

### Features MVP

1. [ ] CLI menu basique (selection projet)
2. [ ] Lecture `.claude-launcher.json`
3. [ ] Modification `settings.json` avant lancement
4. [ ] Lancement avec `--dangerously-skip-permissions` optionnel
5. [ ] Restauration settings apres fermeture

### Features v1.1

- [ ] TUI avec ncurses/PSMenu
- [ ] Creation nouveau projet avec wizard
- [ ] Generation automatique `spec.minimal.md`
- [ ] Historique des sessions par projet

### Features v2.0

- [ ] Synchronisation settings entre machines (OneDrive)
- [ ] Presets de MCP servers par type de tache
- [ ] Integration avec `gh` pour projets GitHub
- [ ] Metriques d'utilisation par profil

---

## Commande de Lancement

Apres installation:

```powershell
# Lancer le menu
claude-launcher

# Lancer directement un projet
claude-launcher --project "Lecteur_Magic"

# Lancer avec profil specifique
claude-launcher --project "MonProjet" --profile dev

# Mode rapide (dernier projet, meme config)
claude-launcher --last
```

---

## Notes d'Implementation

1. **Backup settings.json**: Toujours faire un backup avant modification
2. **Atomic operations**: Utiliser fichiers .tmp puis rename
3. **Error handling**: Si Claude crash, restaurer settings.json
4. **Cross-platform**: PowerShell 7 fonctionne sur Linux/Mac

---

## Fichiers a Creer

| Fichier | Description |
|---------|-------------|
| `claude-launcher.ps1` | Script principal |
| `lib/menu.ps1` | Fonctions TUI |
| `lib/config.ps1` | Gestion configuration |
| `lib/mcp.ps1` | Gestion MCP servers |
| `lib/openspec.ps1` | Generation spec minimal |
| `profiles/*.json` | Definitions des profils |

---

*Prompt genere le 2026-01-24 pour le projet Lanceur_Claude*
