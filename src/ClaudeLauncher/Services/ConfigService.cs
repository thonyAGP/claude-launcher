using System.Text.Json;
using System.Text.Json.Serialization;
using ClaudeLauncher.Models;

namespace ClaudeLauncher.Services;

public class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _configDir;
    private readonly string _configPath;
    private readonly string _profilesDir;

    public ConfigService()
    {
        _configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude-launcher"
        );
        _configPath = Path.Combine(_configDir, "config.json");
        _profilesDir = Path.Combine(_configDir, "profiles");

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_configDir);
        Directory.CreateDirectory(_profilesDir);
        Directory.CreateDirectory(Path.Combine(_configDir, "backups"));
    }

    public LauncherConfig LoadConfig()
    {
        if (!File.Exists(_configPath))
        {
            var defaultConfig = new LauncherConfig
            {
                ClaudeSettingsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".claude",
                    "settings.json"
                )
            };
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(_configPath);
        return JsonSerializer.Deserialize<LauncherConfig>(json, JsonOptions) ?? new LauncherConfig();
    }

    public void SaveConfig(LauncherConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(_configPath, json);
    }

    public void AddRecentProject(LauncherConfig config, Project project)
    {
        config.RecentProjects.RemoveAll(p => p.Path == project.Path);
        project.LastOpened = DateTime.Now;
        config.RecentProjects.Insert(0, project);

        if (config.RecentProjects.Count > config.MaxRecentProjects)
        {
            config.RecentProjects = config.RecentProjects.Take(config.MaxRecentProjects).ToList();
        }

        SaveConfig(config);
    }

    public List<Profile> LoadProfiles()
    {
        var profiles = new List<Profile>();
        var defaultProfiles = GetDefaultProfiles();

        foreach (var profile in defaultProfiles)
        {
            var profilePath = Path.Combine(_profilesDir, $"{profile.Name}.json");
            if (!File.Exists(profilePath))
            {
                var json = JsonSerializer.Serialize(profile, JsonOptions);
                File.WriteAllText(profilePath, json);
            }
        }

        foreach (var file in Directory.GetFiles(_profilesDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var profile = JsonSerializer.Deserialize<Profile>(json, JsonOptions);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }
            catch
            {
                // Skip invalid profiles
            }
        }

        return profiles.OrderBy(p => p.Name).ToList();
    }

    private static List<Profile> GetDefaultProfiles()
    {
        return new List<Profile>
        {
            new()
            {
                Name = "magic-analysis",
                Description = "Analyse et migration Magic Unipaas",
                McpServers = new List<string> { "context7" },
                Rules = new List<string> { "dotnet", "sql-server", "magic-analysis" },
                OpenSpecMode = "full",
                SkipTickets = false,
                Model = "sonnet",
                PermissionMode = "default",
                SystemPromptAddition = "Focus on Magic Unipaas analysis. Use magic-analysis rules.",
                MaxTurns = null
            },
            new()
            {
                Name = "dev",
                Description = "Developpement outils (MCP, scripts)",
                McpServers = new List<string> { "context7" },
                Rules = new List<string> { "typescript", "testing", "dotnet" },
                OpenSpecMode = "minimal",
                SkipTickets = true,
                Model = "sonnet",
                PermissionMode = "bypassPermissions",
                SystemPromptAddition = null,
                MaxTurns = null
            },
            new()
            {
                Name = "web",
                Description = "Applications web (Next.js, React)",
                McpServers = new List<string> { "context7", "eslint" },
                Rules = new List<string> { "typescript", "react", "testing" },
                OpenSpecMode = "standard",
                SkipTickets = true,
                Model = "sonnet",
                PermissionMode = "bypassPermissions",
                SystemPromptAddition = null,
                MaxTurns = null
            },
            new()
            {
                Name = "business",
                Description = "Analyses business et plans",
                McpServers = new List<string>(),
                Rules = new List<string> { "business" },
                OpenSpecMode = "minimal",
                SkipTickets = true,
                Model = "haiku",
                PermissionMode = "default",
                SystemPromptAddition = "Focus on business analysis. Follow business rules strictly.",
                MaxTurns = 50
            }
        };
    }

    public Profile? DetectProjectProfile(string projectPath)
    {
        var profiles = LoadProfiles();

        // Check for .claude-launcher.json in project
        var launcherConfigPath = Path.Combine(projectPath, ".claude-launcher.json");
        if (File.Exists(launcherConfigPath))
        {
            try
            {
                var json = File.ReadAllText(launcherConfigPath);
                var config = JsonSerializer.Deserialize<ProjectLauncherConfig>(json, JsonOptions);
                if (config != null)
                {
                    return profiles.FirstOrDefault(p => p.Name == config.Profile);
                }
            }
            catch { }
        }

        // Auto-detect based on files
        if (Directory.Exists(Path.Combine(projectPath, "skills", "magic-unipaas")) ||
            Directory.GetFiles(projectPath, "*.xml", SearchOption.TopDirectoryOnly).Any())
        {
            return profiles.FirstOrDefault(p => p.Name == "magic-analysis");
        }

        if (File.Exists(Path.Combine(projectPath, "next.config.js")) ||
            File.Exists(Path.Combine(projectPath, "next.config.mjs")) ||
            File.Exists(Path.Combine(projectPath, "next.config.ts")))
        {
            return profiles.FirstOrDefault(p => p.Name == "web");
        }

        if (Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly).Any())
        {
            return profiles.FirstOrDefault(p => p.Name == "dev");
        }

        return profiles.FirstOrDefault(p => p.Name == "dev");
    }

    public List<string> GetAvailableMcpServers()
    {
        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude",
            "settings.json"
        );

        if (!File.Exists(settingsPath)) return new List<string>();

        try
        {
            var json = File.ReadAllText(settingsPath);
            using var doc = JsonDocument.Parse(json);
            var servers = new List<string>();

            if (doc.RootElement.TryGetProperty("mcpServers", out var mcpServers))
            {
                foreach (var server in mcpServers.EnumerateObject())
                {
                    servers.Add(server.Name);
                }
            }

            // Also check enabledMcpjsonServers for servers not in mcpServers
            if (doc.RootElement.TryGetProperty("enabledMcpjsonServers", out var enabled))
            {
                foreach (var server in enabled.EnumerateArray())
                {
                    var name = server.GetString();
                    if (name != null && !servers.Contains(name))
                    {
                        servers.Add(name);
                    }
                }
            }

            return servers.OrderBy(s => s).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public List<Project> ScanProjectsDirectory(LauncherConfig config)
    {
        var projects = new List<Project>();

        if (string.IsNullOrEmpty(config.ProjectsRootPath) || !Directory.Exists(config.ProjectsRootPath))
        {
            return projects;
        }

        try
        {
            var directories = Directory.GetDirectories(config.ProjectsRootPath);

            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);

                // Skip excluded folders
                if (config.ExcludedFolders.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Skip hidden folders
                var dirInfo = new DirectoryInfo(dir);
                if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                // Check if it looks like a project (has common project files)
                if (!IsLikelyProject(dir))
                {
                    continue;
                }

                // Check if already in recent projects
                var existingProject = config.RecentProjects.FirstOrDefault(p => p.Path == dir);
                if (existingProject != null)
                {
                    projects.Add(existingProject);
                    continue;
                }

                // Detect profile for new project
                var profile = DetectProjectProfile(dir);

                var project = new Project
                {
                    Name = dirName,
                    Path = dir,
                    Profile = profile?.Name ?? "dev",
                    LastOpened = Directory.GetLastWriteTime(dir)
                };

                // Load project-specific MCP if exists
                var projectConfig = LoadProjectConfig(dir);
                if (projectConfig?.McpServers != null)
                {
                    project.EnabledMcpServers = projectConfig.McpServers;
                }
                else if (profile != null)
                {
                    project.EnabledMcpServers = profile.McpServers;
                }

                projects.Add(project);
            }
        }
        catch
        {
            // Ignore scan errors
        }

        return projects.OrderByDescending(p => p.LastOpened).ToList();
    }

    private static bool IsLikelyProject(string dir)
    {
        // Check for common project indicators
        var indicators = new[]
        {
            "package.json",
            "*.csproj",
            "*.sln",
            "Cargo.toml",
            "go.mod",
            "requirements.txt",
            "pyproject.toml",
            ".git",
            ".openspec",
            ".claude-launcher.json",
            "CLAUDE.md"
        };

        foreach (var indicator in indicators)
        {
            if (indicator.Contains('*'))
            {
                if (Directory.GetFiles(dir, indicator, SearchOption.TopDirectoryOnly).Length > 0)
                {
                    return true;
                }
            }
            else if (indicator.StartsWith('.'))
            {
                if (Directory.Exists(Path.Combine(dir, indicator)) || File.Exists(Path.Combine(dir, indicator)))
                {
                    return true;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(dir, indicator)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private ProjectLauncherConfig? LoadProjectConfig(string projectPath)
    {
        var configPath = Path.Combine(projectPath, ".claude-launcher.json");
        if (!File.Exists(configPath)) return null;

        try
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<ProjectLauncherConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
