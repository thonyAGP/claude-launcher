namespace ClaudeLauncher.Models;

public class LauncherConfig
{
    public List<Project> RecentProjects { get; set; } = new();
    public string DefaultProfile { get; set; } = "dev";
    public int MaxRecentProjects { get; set; } = 10;
    public string ClaudeSettingsPath { get; set; } = string.Empty;
    public string ProjectsRootPath { get; set; } = @"D:\Projects";
    public bool ScanAtStartup { get; set; } = true;
    public List<string> ExcludedFolders { get; set; } = new() { "node_modules", ".git", "bin", "obj", ".vs" };
}

public class ClaudeSettings
{
    public List<string> EnabledMcpjsonServers { get; set; } = new();
    public Dictionary<string, object>? McpServers { get; set; }
    public object? Permissions { get; set; }
    public object? Hooks { get; set; }
    public object? StatusLine { get; set; }
    public object? EnabledPlugins { get; set; }
    public bool EnableAllProjectMcpServers { get; set; }
    public List<string>? AlwaysApproveTools { get; set; }
    public bool AutoApproveReadsInDir { get; set; }
    public string? ThinkingBudget { get; set; }
}

public class ProjectLauncherConfig
{
    public string Profile { get; set; } = "dev";
    public List<string>? McpServers { get; set; }
    public List<string>? Rules { get; set; }
    public string OpenSpec { get; set; } = "standard";
    public bool SkipTickets { get; set; } = true;
    public bool DangerousMode { get; set; } = true;
    public DateTime LastOpened { get; set; } = DateTime.Now;
}
