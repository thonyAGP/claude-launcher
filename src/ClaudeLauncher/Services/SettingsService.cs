using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ClaudeLauncher.Models;

namespace ClaudeLauncher.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _settingsPath;
    private readonly string _backupDir;

    public SettingsService()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude",
            "settings.json"
        );
        _backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude-launcher",
            "backups"
        );
        Directory.CreateDirectory(_backupDir);
    }

    public string BackupSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            throw new FileNotFoundException("Claude settings.json not found", _settingsPath);
        }

        var backupPath = Path.Combine(_backupDir, $"settings.{DateTime.Now:yyyyMMdd_HHmmss}.json");
        File.Copy(_settingsPath, backupPath, true);

        // Also keep a "latest" backup for easy restore
        var latestBackup = Path.Combine(_backupDir, "settings.latest.json");
        File.Copy(_settingsPath, latestBackup, true);

        return backupPath;
    }

    public void RestoreSettings()
    {
        var latestBackup = Path.Combine(_backupDir, "settings.latest.json");
        if (File.Exists(latestBackup))
        {
            File.Copy(latestBackup, _settingsPath, true);
        }
    }

    public List<string> GetEnabledMcpServers()
    {
        if (!File.Exists(_settingsPath)) return new List<string>();

        try
        {
            var json = File.ReadAllText(_settingsPath);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("enabledMcpjsonServers", out var servers))
            {
                return servers.EnumerateArray()
                    .Select(s => s.GetString())
                    .Where(s => s != null)
                    .Cast<string>()
                    .ToList();
            }
        }
        catch { }

        return new List<string>();
    }

    public void SetEnabledMcpServers(List<string> servers)
    {
        if (!File.Exists(_settingsPath)) return;

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var node = JsonNode.Parse(json);
            if (node == null) return;

            var serversArray = new JsonArray();
            foreach (var server in servers)
            {
                serversArray.Add(server);
            }

            node["enabledMcpjsonServers"] = serversArray;

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_settingsPath, node.ToJsonString(options));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update settings.json: {ex.Message}", ex);
        }
    }

    public void CreateProjectLauncherConfig(string projectPath, Profile profile, List<string> mcpServers, bool dangerousMode)
    {
        var config = new ProjectLauncherConfig
        {
            Profile = profile.Name,
            McpServers = mcpServers,
            Rules = profile.Rules,
            OpenSpec = profile.OpenSpecMode,
            SkipTickets = profile.SkipTickets,
            DangerousMode = dangerousMode,
            LastOpened = DateTime.Now
        };

        var configPath = Path.Combine(projectPath, ".claude-launcher.json");
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(configPath, json);
    }

    public ProjectLauncherConfig? LoadProjectLauncherConfig(string projectPath)
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
