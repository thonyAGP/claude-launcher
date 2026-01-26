using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ClaudeLauncher.Models;

namespace ClaudeLauncher.Services;

public class LaunchOptions
{
    public required string ProjectPath { get; set; }
    public required Profile Profile { get; set; }
    public required List<string> EnabledMcpServers { get; set; }
    public string PermissionMode { get; set; } = "default";
    public string Model { get; set; } = "sonnet";
    public string? InitialPrompt { get; set; }
    public int? MaxTurns { get; set; }
}

public class LaunchService
{
    private readonly SettingsService _settingsService;
    private readonly ConfigService _configService;

    public LaunchService(SettingsService settingsService, ConfigService configService)
    {
        _settingsService = settingsService;
        _configService = configService;
    }

    public void LaunchClaudeDetached(LaunchOptions options)
    {
        // Create project launcher config for hooks to read
        _settingsService.CreateProjectLauncherConfig(
            options.ProjectPath,
            options.Profile,
            options.EnabledMcpServers,
            options.PermissionMode == "bypassPermissions"
        );

        // Build command line arguments
        var args = BuildCommandLineArgs(options);

        // Use Windows Terminal if available, otherwise cmd
        var terminalPath = FindWindowsTerminal();
        ProcessStartInfo startInfo;

        if (terminalPath != null)
        {
            startInfo = new ProcessStartInfo
            {
                FileName = terminalPath,
                Arguments = $"-d \"{options.ProjectPath}\" -- claude {args}",
                UseShellExecute = true
            };
        }
        else
        {
            startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k cd /d \"{options.ProjectPath}\" && claude {args}",
                UseShellExecute = true
            };
        }

        Process.Start(startInfo);
    }

    // Legacy method for backward compatibility
    public void LaunchClaudeDetached(
        string projectPath,
        Profile profile,
        List<string> enabledMcpServers,
        bool dangerouslySkipPermissions)
    {
        var options = new LaunchOptions
        {
            ProjectPath = projectPath,
            Profile = profile,
            EnabledMcpServers = enabledMcpServers,
            PermissionMode = dangerouslySkipPermissions ? "bypassPermissions" : profile.PermissionMode,
            Model = profile.Model,
            MaxTurns = profile.MaxTurns
        };

        LaunchClaudeDetached(options);
    }

    private string BuildCommandLineArgs(LaunchOptions options)
    {
        var args = new List<string>();

        // Model selection
        if (!string.IsNullOrEmpty(options.Model) && options.Model != "sonnet")
        {
            args.Add($"--model {options.Model}");
        }

        // Permission mode
        switch (options.PermissionMode)
        {
            case "bypassPermissions":
                args.Add("--dangerously-skip-permissions");
                break;
            case "acceptEdits":
                args.Add("--permission-mode acceptEdits");
                break;
            case "plan":
                args.Add("--permission-mode plan");
                break;
            // "default" doesn't need any flag
        }

        // MCP configuration - use --mcp-config with strict mode
        if (options.EnabledMcpServers.Count > 0)
        {
            var mcpConfig = BuildMcpConfig(options.EnabledMcpServers);
            if (!string.IsNullOrEmpty(mcpConfig))
            {
                // Write to temp file (command line has length limits)
                var tempFile = Path.Combine(Path.GetTempPath(), $"claude-mcp-{Guid.NewGuid():N}.json");
                File.WriteAllText(tempFile, mcpConfig);
                args.Add($"--mcp-config \"{tempFile}\"");

                // Schedule cleanup after 30 seconds (Claude has read the config by then)
                Task.Run(async () =>
                {
                    await Task.Delay(30000);
                    try { File.Delete(tempFile); } catch { }
                });
            }
        }

        // System prompt addition from profile
        if (!string.IsNullOrEmpty(options.Profile.SystemPromptAddition))
        {
            var escaped = options.Profile.SystemPromptAddition.Replace("\"", "\\\"");
            args.Add($"--append-system-prompt \"{escaped}\"");
        }

        // Max turns for safety
        var maxTurns = options.MaxTurns ?? options.Profile.MaxTurns;
        if (maxTurns.HasValue)
        {
            args.Add($"--max-turns {maxTurns.Value}");
        }

        // Initial prompt (if provided)
        if (!string.IsNullOrEmpty(options.InitialPrompt))
        {
            var escaped = options.InitialPrompt.Replace("\"", "\\\"");
            args.Add($"\"{escaped}\"");
        }

        return string.Join(" ", args);
    }

    private string? BuildMcpConfig(List<string> enabledServers)
    {
        if (enabledServers.Count == 0) return null;

        // Read existing MCP server configurations from settings.json
        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude",
            "settings.json"
        );

        if (!File.Exists(settingsPath)) return null;

        try
        {
            var json = File.ReadAllText(settingsPath);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("mcpServers", out var mcpServersElement))
                return null;

            var filteredServers = new Dictionary<string, JsonElement>();

            foreach (var server in mcpServersElement.EnumerateObject())
            {
                if (enabledServers.Contains(server.Name))
                {
                    filteredServers[server.Name] = server.Value.Clone();
                }
            }

            if (filteredServers.Count == 0) return null;

            // Build the MCP config JSON
            var config = new { mcpServers = filteredServers };
            return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return null;
        }
    }

    private static string? FindWindowsTerminal()
    {
        var wtPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "WindowsApps",
            "wt.exe"
        );

        return File.Exists(wtPath) ? wtPath : null;
    }

    public static bool IsClaudeInstalled()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c claude --version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(5000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
