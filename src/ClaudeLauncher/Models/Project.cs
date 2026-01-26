namespace ClaudeLauncher.Models;

public class Project
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Profile { get; set; } = "dev";
    public DateTime LastOpened { get; set; } = DateTime.Now;
    public List<string> EnabledMcpServers { get; set; } = new();
    public bool DangerouslySkipPermissions { get; set; } = true;
    public bool LoadTickets { get; set; } = false;

    public string DisplayName => $"{Name} [{Profile}] {LastOpened:dd/MM/yyyy}";
}
