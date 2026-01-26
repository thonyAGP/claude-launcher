using ClaudeLauncher.Models;
using ClaudeLauncher.Services;

namespace ClaudeLauncher;

public partial class MainForm : Form
{
    private readonly ConfigService _configService;
    private readonly SettingsService _settingsService;
    private readonly LaunchService _launchService;

    private LauncherConfig _config = null!;
    private List<Profile> _profiles = new();
    private List<Project> _projects = new();
    private List<string> _availableMcpServers = new();
    private readonly Dictionary<string, CheckBox> _mcpCheckboxes = new();

    private int _sortColumn = 2; // Default sort by LastOpened
    private bool _sortAscending = false; // Descending by default

    private Project? SelectedProject
    {
        get
        {
            if (listViewProjects.SelectedItems.Count == 0) return null;
            return listViewProjects.SelectedItems[0].Tag as Project;
        }
    }

    private Profile? SelectedProfile => comboBoxProfile.SelectedItem as Profile;

    public MainForm()
    {
        InitializeComponent();

        _configService = new ConfigService();
        _settingsService = new SettingsService();
        _launchService = new LaunchService(_settingsService, _configService);
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        // Check if Claude is installed
        if (!LaunchService.IsClaudeInstalled())
        {
            MessageBox.Show(
                "Claude Code n'est pas installe ou n'est pas dans le PATH.\n\n" +
                "Installez Claude Code avec: npm install -g @anthropic/claude-code",
                "Claude non trouve",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        LoadConfiguration();
        LoadProfiles();
        LoadMcpServers();

        // Scan projects directory at startup if enabled
        if (_config.ScanAtStartup)
        {
            ScanAndRefreshProjects();
        }
        else
        {
            RefreshProjectList();
        }

        UpdateProjectsLabel();
        PositionFooterButtons();
    }

    private void PositionFooterButtons()
    {
        // Position buttons from the right edge of the footer panel
        var panel = buttonCancel.Parent;
        if (panel == null) return;

        int rightMargin = 15;
        int buttonY = 12;
        int gap = 15;

        // Annuler button at far right
        buttonCancel.Location = new Point(
            panel.ClientSize.Width - rightMargin - buttonCancel.Width,
            buttonY
        );

        // Lancer Claude button to the left of Annuler
        buttonLaunch.Location = new Point(
            buttonCancel.Left - gap - buttonLaunch.Width,
            buttonY
        );
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        PositionFooterButtons();
    }

    private void LoadConfiguration()
    {
        _config = _configService.LoadConfig();
    }

    private void LoadProfiles()
    {
        _profiles = _configService.LoadProfiles();
        comboBoxProfile.Items.Clear();
        foreach (var profile in _profiles)
        {
            comboBoxProfile.Items.Add(profile);
        }
        comboBoxProfile.DisplayMember = "Name";

        // Select default profile
        var defaultProfile = _profiles.FirstOrDefault(p => p.Name == _config.DefaultProfile)
                            ?? _profiles.FirstOrDefault();
        if (defaultProfile != null)
        {
            comboBoxProfile.SelectedItem = defaultProfile;
        }
    }

    private void LoadMcpServers()
    {
        _availableMcpServers = _configService.GetAvailableMcpServers();
        RefreshMcpCheckboxes();
    }

    private void RefreshMcpCheckboxes()
    {
        flowLayoutMcp.Controls.Clear();
        _mcpCheckboxes.Clear();

        foreach (var server in _availableMcpServers)
        {
            var checkbox = new CheckBox
            {
                Text = server,
                AutoSize = true,
                Margin = new Padding(5, 3, 15, 3),
                Tag = server
            };
            checkbox.CheckedChanged += McpCheckbox_CheckedChanged;
            flowLayoutMcp.Controls.Add(checkbox);
            _mcpCheckboxes[server] = checkbox;
        }

        UpdateMcpCheckboxesFromProfile();
    }

    private void RefreshProjectList()
    {
        _projects = _config.RecentProjects.ToList();
        PopulateListView();
    }

    private void PopulateListView()
    {
        listViewProjects.Items.Clear();

        // Sort projects
        var sortedProjects = SortProjects(_projects);

        foreach (var project in sortedProjects)
        {
            var item = new ListViewItem(project.Name)
            {
                Tag = project
            };
            item.SubItems.Add(project.Profile);
            item.SubItems.Add(project.LastOpened.ToString("dd/MM/yyyy HH:mm"));
            listViewProjects.Items.Add(item);
        }
    }

    private List<Project> SortProjects(List<Project> projects)
    {
        return _sortColumn switch
        {
            0 => _sortAscending
                ? projects.OrderBy(p => p.Name).ToList()
                : projects.OrderByDescending(p => p.Name).ToList(),
            1 => _sortAscending
                ? projects.OrderBy(p => p.Profile).ToList()
                : projects.OrderByDescending(p => p.Profile).ToList(),
            2 => _sortAscending
                ? projects.OrderBy(p => p.LastOpened).ToList()
                : projects.OrderByDescending(p => p.LastOpened).ToList(),
            _ => projects
        };
    }

    private void ListViewProjects_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        if (e.Column == _sortColumn)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = e.Column;
            _sortAscending = e.Column != 2; // Descending for date, ascending for others
        }

        PopulateListView();
    }

    private void ListViewProjects_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (SelectedProject == null)
        {
            buttonLaunch.Enabled = false;
            labelStatus.Text = "Selectionnez un projet";
            return;
        }

        buttonLaunch.Enabled = true;
        labelStatus.Text = SelectedProject.Path;

        // Load project-specific config from .claude-launcher.json
        var projectConfig = _settingsService.LoadProjectLauncherConfig(SelectedProject.Path);

        if (projectConfig != null)
        {
            // Use profile from .claude-launcher.json (last used)
            var lastProfile = _profiles.FirstOrDefault(p => p.Name == projectConfig.Profile);
            if (lastProfile != null)
            {
                comboBoxProfile.SelectedItem = lastProfile;
            }

            checkBoxDangerous.Checked = projectConfig.DangerousMode;
            checkBoxTickets.Checked = !projectConfig.SkipTickets;

            // Update MCP checkboxes from project config
            if (projectConfig.McpServers != null)
            {
                foreach (var checkbox in _mcpCheckboxes.Values)
                {
                    checkbox.Checked = projectConfig.McpServers.Contains(checkbox.Tag?.ToString() ?? "");
                }
            }
        }
        else
        {
            // No .claude-launcher.json - use profile from Project (stored in RecentProjects)
            var savedProfile = _profiles.FirstOrDefault(p => p.Name == SelectedProject.Profile);
            if (savedProfile != null)
            {
                comboBoxProfile.SelectedItem = savedProfile;
            }
            else
            {
                // Fallback: auto-detect profile based on project files
                var detectedProfile = _configService.DetectProjectProfile(SelectedProject.Path);
                if (detectedProfile != null)
                {
                    comboBoxProfile.SelectedItem = detectedProfile;
                }
            }
            UpdateMcpCheckboxesFromProfile();
        }
    }

    private void ListViewProjects_DoubleClick(object? sender, EventArgs e)
    {
        if (SelectedProject != null)
        {
            LaunchClaude();
        }
    }

    private void ComboBoxProfile_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (SelectedProfile == null) return;

        labelProfileDescription.Text = SelectedProfile.Description;
        checkBoxTickets.Checked = !SelectedProfile.SkipTickets;

        // Update model and permission mode from profile
        var modelIndex = comboBoxModel.Items.IndexOf(SelectedProfile.Model);
        if (modelIndex >= 0) comboBoxModel.SelectedIndex = modelIndex;

        var permIndex = comboBoxPermissionMode.Items.IndexOf(SelectedProfile.PermissionMode);
        if (permIndex >= 0) comboBoxPermissionMode.SelectedIndex = permIndex;

        // Only update MCP if no project is selected or project has no config
        if (SelectedProject == null)
        {
            UpdateMcpCheckboxesFromProfile();
        }
    }

    private void UpdateMcpCheckboxesFromProfile()
    {
        if (SelectedProfile == null) return;

        foreach (var checkbox in _mcpCheckboxes.Values)
        {
            var serverName = checkbox.Tag?.ToString() ?? "";
            checkbox.Checked = SelectedProfile.McpServers.Contains(serverName);
        }
    }

    private void McpCheckbox_CheckedChanged(object? sender, EventArgs e)
    {
        // User manually changed MCP selection - nothing special needed
    }

    private void ButtonBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Selectionnez le dossier du projet",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            AddProject(dialog.SelectedPath);
        }
    }

    private void ButtonNewProject_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Selectionnez ou creez le dossier du nouveau projet",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            AddProject(dialog.SelectedPath);
        }
    }

    private void ButtonRemoveProject_Click(object? sender, EventArgs e)
    {
        if (SelectedProject == null) return;

        var result = MessageBox.Show(
            $"Retirer '{SelectedProject.Name}' de la liste?\n\n" +
            "(Le projet ne sera pas supprime du disque)",
            "Confirmer",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            _config.RecentProjects.RemoveAll(p => p.Path == SelectedProject.Path);
            _projects.RemoveAll(p => p.Path == SelectedProject.Path);
            _configService.SaveConfig(_config);
            PopulateListView();
        }
    }

    private void AddProject(string path)
    {
        if (!Directory.Exists(path))
        {
            MessageBox.Show("Le dossier n'existe pas.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Check if already exists
        var existing = _projects.FirstOrDefault(p => p.Path == path);
        if (existing != null)
        {
            // Select it
            foreach (ListViewItem item in listViewProjects.Items)
            {
                if (item.Tag is Project p && p.Path == path)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    break;
                }
            }
            return;
        }

        var project = new Project
        {
            Name = Path.GetFileName(path),
            Path = path,
            Profile = SelectedProfile?.Name ?? "dev",
            LastOpened = DateTime.Now
        };

        _configService.AddRecentProject(_config, project);
        _projects.Insert(0, project);
        PopulateListView();

        // Select the new project
        if (listViewProjects.Items.Count > 0)
        {
            listViewProjects.Items[0].Selected = true;
        }
    }

    private void ButtonLaunch_Click(object? sender, EventArgs e)
    {
        LaunchClaude();
    }

    private async void LaunchClaude()
    {
        if (SelectedProject == null || SelectedProfile == null) return;

        // Collect enabled MCP servers
        var enabledServers = _mcpCheckboxes
            .Where(kvp => kvp.Value.Checked)
            .Select(kvp => kvp.Key)
            .ToList();

        // Get selected model and permission mode
        var selectedModel = comboBoxModel.SelectedItem?.ToString() ?? "sonnet";
        var selectedPermissionMode = checkBoxDangerous.Checked
            ? "bypassPermissions"
            : comboBoxPermissionMode.SelectedItem?.ToString() ?? "default";

        // Update project with current settings
        SelectedProject.Profile = SelectedProfile.Name;
        SelectedProject.EnabledMcpServers = enabledServers;
        SelectedProject.DangerouslySkipPermissions = checkBoxDangerous.Checked;
        SelectedProject.LoadTickets = checkBoxTickets.Checked;
        _configService.AddRecentProject(_config, SelectedProject);

        // Disable UI during launch
        buttonLaunch.Enabled = false;
        buttonBrowse.Enabled = false;
        buttonNewProject.Enabled = false;

        try
        {
            // Build launch options with all settings
            var options = new LaunchOptions
            {
                ProjectPath = SelectedProject.Path,
                Profile = SelectedProfile,
                EnabledMcpServers = enabledServers,
                Model = selectedModel,
                PermissionMode = selectedPermissionMode,
                MaxTurns = SelectedProfile.MaxTurns
            };

            // Launch Claude in detached mode (don't wait)
            _launchService.LaunchClaudeDetached(options);

            // Small delay to let Claude start
            await Task.Delay(1000);

            // Ask if user wants to keep window open (with 5s timeout)
            var keepOpen = await ShowKeepOpenDialogAsync();

            if (keepOpen)
            {
                // Re-enable UI for another launch
                buttonLaunch.Enabled = SelectedProject != null;
                buttonBrowse.Enabled = true;
                buttonNewProject.Enabled = true;
                labelStatus.Text = "Pret pour un autre lancement";
            }
            else
            {
                this.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du lancement:\n{ex.Message}",
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            buttonLaunch.Enabled = true;
            buttonBrowse.Enabled = true;
            buttonNewProject.Enabled = true;
        }
    }

    private Task<bool> ShowKeepOpenDialogAsync()
    {
        using var dialog = new Form
        {
            Text = "Claude lance",
            Size = new Size(350, 150),
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            TopMost = true
        };

        var label = new Label
        {
            Text = "Garder la fenetre ouverte pour lancer un autre projet?",
            Location = new Point(20, 20),
            AutoSize = true
        };

        var countdownLabel = new Label
        {
            Text = "Fermeture dans 5s...",
            Location = new Point(20, 50),
            AutoSize = true,
            ForeColor = Color.Gray
        };

        var btnYes = new Button
        {
            Text = "Oui",
            DialogResult = DialogResult.Yes,
            Location = new Point(80, 80),
            Size = new Size(80, 30)
        };

        var btnNo = new Button
        {
            Text = "Non",
            DialogResult = DialogResult.No,
            Location = new Point(180, 80),
            Size = new Size(80, 30)
        };

        dialog.Controls.AddRange(new Control[] { label, countdownLabel, btnYes, btnNo });
        dialog.AcceptButton = btnYes;
        dialog.CancelButton = btnNo;

        // Countdown timer
        var countdown = 5;
        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (s, e) =>
        {
            countdown--;
            if (countdown <= 0)
            {
                timer.Stop();
                dialog.DialogResult = DialogResult.No;
                dialog.Close();
            }
            else
            {
                countdownLabel.Text = $"Fermeture dans {countdown}s...";
            }
        };
        timer.Start();

        var result = dialog.ShowDialog(this);
        timer.Stop();

        return Task.FromResult(result == DialogResult.Yes);
    }

    private void ButtonCancel_Click(object? sender, EventArgs e)
    {
        this.Close();
    }

    private void ButtonRefresh_Click(object? sender, EventArgs e)
    {
        labelStatus.Text = "Scan en cours...";
        Application.DoEvents();

        ScanAndRefreshProjects();

        labelStatus.Text = $"Scan termine - {listViewProjects.Items.Count} projets";
    }

    private void ButtonSettings_Click(object? sender, EventArgs e)
    {
        using var dialog = new SettingsForm(_config);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _configService.SaveConfig(_config);
            ScanAndRefreshProjects();
            UpdateProjectsLabel();
        }
    }

    private void ScanAndRefreshProjects()
    {
        _projects = _configService.ScanProjectsDirectory(_config);
        PopulateListView();
    }

    private void UpdateProjectsLabel()
    {
        if (!string.IsNullOrEmpty(_config.ProjectsRootPath) && Directory.Exists(_config.ProjectsRootPath))
        {
            labelProjects.Text = $"Projets ({_config.ProjectsRootPath})";
        }
        else
        {
            labelProjects.Text = "Projets";
        }
    }
}
