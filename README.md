# Claude Launcher

Application Windows pour lancer Claude Code avec gestion de profils et contexte optimise.

## Fonctionnalites

- **Selection de projet** : Liste des projets recents + navigation
- **Profils de contexte** : magic-analysis, dev, web, business
- **Gestion MCP servers** : Active/desactive selon le profil
- **Mode dangerous** : Option pour skip les permissions
- **Integration hooks** : S'integre au hook session-start existant

## Installation

```powershell
# Clone et installe
cd D:\Projects\Lanceur_Claude
powershell -ExecutionPolicy Bypass -File install.ps1
```

L'installation:
1. Compile l'application
2. Copie dans `%LOCALAPPDATA%\ClaudeLauncher`
3. Cree un raccourci sur le Bureau
4. Initialise les profils par defaut

## Utilisation

1. Double-cliquer sur "Claude Launcher" sur le Bureau
2. Selectionner un projet (ou "Parcourir...")
3. Choisir le profil
4. Ajuster les MCP servers si besoin
5. Cliquer "Lancer Claude"

## Profils

| Profil | MCP Servers | Description |
|--------|-------------|-------------|
| magic-analysis | context7 | Analyse Magic Unipaas - tickets charges |
| dev | context7 | Developpement outils - contexte minimal |
| web | context7, eslint | Applications web Next.js/React |
| business | (aucun) | Analyses business - pas de MCP |

## Configuration

### Dossiers

- `%USERPROFILE%\.claude-launcher\` - Config globale
- `%USERPROFILE%\.claude-launcher\profiles\` - Profils JSON
- `%USERPROFILE%\.claude-launcher\backups\` - Backups settings.json

### Fichier projet

Chaque projet peut avoir un `.claude-launcher.json`:

```json
{
  "profile": "dev",
  "mcpServers": ["context7"],
  "rules": ["typescript", "testing"],
  "openSpec": "minimal",
  "skipTickets": true,
  "dangerousMode": true,
  "lastOpened": "2026-01-25T10:00:00Z"
}
```

## Fonctionnement

1. **Backup** : Sauvegarde `settings.json` avant modification
2. **Configure** : Met a jour `enabledMcpjsonServers` selon profil
3. **Projet** : Cree/maj `.claude-launcher.json` dans le projet
4. **Lance** : Ouvre Claude Code dans Windows Terminal (ou cmd)
5. **Restore** : Restaure `settings.json` original apres 5 secondes

## Build

```powershell
cd src\ClaudeLauncher
dotnet build -c Release
```

Executable: `bin\Release\net8.0-windows\ClaudeLauncher.exe`

## Prerequis

- Windows 10/11
- .NET 8 Runtime
- Claude Code installe (`npm install -g @anthropic/claude-code`)

## Structure

```
src/
  ClaudeLauncher/
    Program.cs           # Point d'entree
    MainForm.cs          # Interface principale
    Models/              # Project, Profile, Config
    Services/            # Config, Settings, Launch
install.ps1              # Script d'installation
```
