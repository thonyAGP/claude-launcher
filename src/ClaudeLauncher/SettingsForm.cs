using ClaudeLauncher.Models;

namespace ClaudeLauncher;

public class SettingsForm : Form
{
    private readonly LauncherConfig _config;

    private TextBox textBoxProjectsRoot = null!;
    private Button buttonBrowse = null!;
    private CheckBox checkBoxScanAtStartup = null!;
    private TextBox textBoxExcludedFolders = null!;
    private ComboBox comboBoxDefaultProfile = null!;
    private Button buttonOk = null!;
    private Button buttonCancel = null!;

    public SettingsForm(LauncherConfig config)
    {
        _config = config;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        this.Text = "Parametres";
        this.Size = new Size(500, 320);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            RowCount = 6,
            ColumnCount = 2
        };
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Projects root
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Scan at startup
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // Excluded folders
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Default profile
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Spacer
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // Buttons

        // Projects Root
        var labelProjectsRoot = new Label
        {
            Text = "Dossier Projects:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 5, 0, 0)
        };
        tableLayout.Controls.Add(labelProjectsRoot, 0, 0);

        var panelProjectsRoot = new Panel { Dock = DockStyle.Fill };
        textBoxProjectsRoot = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F)
        };
        buttonBrowse = new Button
        {
            Text = "...",
            Width = 30,
            Dock = DockStyle.Right
        };
        buttonBrowse.Click += ButtonBrowse_Click;
        panelProjectsRoot.Controls.Add(textBoxProjectsRoot);
        panelProjectsRoot.Controls.Add(buttonBrowse);
        tableLayout.Controls.Add(panelProjectsRoot, 1, 0);

        // Scan at startup
        var labelScan = new Label
        {
            Text = "Au demarrage:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 5, 0, 0)
        };
        tableLayout.Controls.Add(labelScan, 0, 1);

        checkBoxScanAtStartup = new CheckBox
        {
            Text = "Scanner automatiquement le dossier",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        tableLayout.Controls.Add(checkBoxScanAtStartup, 1, 1);

        // Excluded folders
        var labelExcluded = new Label
        {
            Text = "Dossiers exclus:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Padding = new Padding(0, 5, 0, 0)
        };
        tableLayout.Controls.Add(labelExcluded, 0, 2);

        textBoxExcludedFolders = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            Font = new Font("Consolas", 9F),
            ScrollBars = ScrollBars.Vertical
        };
        tableLayout.Controls.Add(textBoxExcludedFolders, 1, 2);

        // Default profile
        var labelProfile = new Label
        {
            Text = "Profil par defaut:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 5, 0, 0)
        };
        tableLayout.Controls.Add(labelProfile, 0, 3);

        comboBoxDefaultProfile = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9F)
        };
        comboBoxDefaultProfile.Items.AddRange(new[] { "dev", "web", "magic-analysis", "business" });
        tableLayout.Controls.Add(comboBoxDefaultProfile, 1, 3);

        // Buttons
        var panelButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 5, 0, 0)
        };

        buttonCancel = new Button
        {
            Text = "Annuler",
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };

        buttonOk = new Button
        {
            Text = "OK",
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        buttonOk.Click += ButtonOk_Click;

        panelButtons.Controls.Add(buttonCancel);
        panelButtons.Controls.Add(buttonOk);
        tableLayout.SetColumnSpan(panelButtons, 2);
        tableLayout.Controls.Add(panelButtons, 0, 5);

        this.Controls.Add(tableLayout);
        this.AcceptButton = buttonOk;
        this.CancelButton = buttonCancel;
    }

    private void LoadSettings()
    {
        textBoxProjectsRoot.Text = _config.ProjectsRootPath;
        checkBoxScanAtStartup.Checked = _config.ScanAtStartup;
        textBoxExcludedFolders.Text = string.Join(Environment.NewLine, _config.ExcludedFolders);
        comboBoxDefaultProfile.SelectedItem = _config.DefaultProfile;
    }

    private void ButtonBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Selectionnez le dossier racine de vos projets",
            UseDescriptionForTitle = true,
            InitialDirectory = textBoxProjectsRoot.Text
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            textBoxProjectsRoot.Text = dialog.SelectedPath;
        }
    }

    private void ButtonOk_Click(object? sender, EventArgs e)
    {
        _config.ProjectsRootPath = textBoxProjectsRoot.Text.Trim();
        _config.ScanAtStartup = checkBoxScanAtStartup.Checked;
        _config.ExcludedFolders = textBoxExcludedFolders.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
        _config.DefaultProfile = comboBoxDefaultProfile.SelectedItem?.ToString() ?? "dev";
    }
}
