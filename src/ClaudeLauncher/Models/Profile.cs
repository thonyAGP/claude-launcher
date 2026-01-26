namespace ClaudeLauncher.Models;

public class Profile
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> McpServers { get; set; } = new();
    public List<string> Rules { get; set; } = new();
    public string OpenSpecMode { get; set; } = "standard"; // minimal, standard, full
    public bool SkipTickets { get; set; } = true;

    // Model selection: haiku (fast/cheap), sonnet (balanced), opus (powerful)
    public string Model { get; set; } = "sonnet";

    // Permission mode: default, acceptEdits, plan, bypassPermissions
    public string PermissionMode { get; set; } = "default";

    // Additional system prompt injected via --append-system-prompt
    public string? SystemPromptAddition { get; set; }

    // Max agentic turns for safety (null = unlimited)
    public int? MaxTurns { get; set; }

    public override string ToString() => Name;
}
