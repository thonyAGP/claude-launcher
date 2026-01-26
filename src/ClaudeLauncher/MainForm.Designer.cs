namespace ClaudeLauncher;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        // Main layout
        this.tableLayoutMain = new TableLayoutPanel();
        this.panelHeader = new Panel();
        this.labelTitle = new Label();
        this.panelProjects = new Panel();
        this.labelProjects = new Label();
        this.listViewProjects = new ListView();
        this.columnName = new ColumnHeader();
        this.columnProfile = new ColumnHeader();
        this.columnLastOpened = new ColumnHeader();
        this.panelProjectButtons = new FlowLayoutPanel();
        this.buttonBrowse = new Button();
        this.buttonNewProject = new Button();
        this.buttonRemoveProject = new Button();
        this.buttonRefresh = new Button();
        this.buttonSettings = new Button();
        this.panelConfig = new Panel();
        this.labelProfile = new Label();
        this.comboBoxProfile = new ComboBox();
        this.labelProfileDescription = new Label();
        this.groupBoxMcp = new GroupBox();
        this.flowLayoutMcp = new FlowLayoutPanel();
        this.groupBoxOptions = new GroupBox();
        this.labelModel = new Label();
        this.comboBoxModel = new ComboBox();
        this.labelPermissionMode = new Label();
        this.comboBoxPermissionMode = new ComboBox();
        this.checkBoxDangerous = new CheckBox();
        this.checkBoxTickets = new CheckBox();
        this.panelFooter = new Panel();
        this.buttonLaunch = new Button();
        this.buttonCancel = new Button();
        this.labelStatus = new Label();
        this.toolTip = new ToolTip(this.components);

        this.SuspendLayout();
        this.tableLayoutMain.SuspendLayout();

        // tableLayoutMain
        this.tableLayoutMain.ColumnCount = 1;
        this.tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.tableLayoutMain.Dock = DockStyle.Fill;
        this.tableLayoutMain.RowCount = 4;
        this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Header
        this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));  // Projects
        this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));  // Config
        this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Footer
        this.tableLayoutMain.Controls.Add(this.panelHeader, 0, 0);
        this.tableLayoutMain.Controls.Add(this.panelProjects, 0, 1);
        this.tableLayoutMain.Controls.Add(this.panelConfig, 0, 2);
        this.tableLayoutMain.Controls.Add(this.panelFooter, 0, 3);

        // panelHeader
        this.panelHeader.BackColor = Color.FromArgb(45, 45, 48);
        this.panelHeader.Dock = DockStyle.Fill;
        this.panelHeader.Padding = new Padding(15, 10, 15, 10);
        this.panelHeader.Controls.Add(this.labelTitle);

        // labelTitle
        this.labelTitle.AutoSize = true;
        this.labelTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        this.labelTitle.ForeColor = Color.White;
        this.labelTitle.Location = new Point(12, 10);
        this.labelTitle.Text = "Claude Launcher";

        // panelProjects
        this.panelProjects.Dock = DockStyle.Fill;
        this.panelProjects.Padding = new Padding(15, 10, 15, 5);
        this.panelProjects.Controls.Add(this.listViewProjects);
        this.panelProjects.Controls.Add(this.panelProjectButtons);
        this.panelProjects.Controls.Add(this.labelProjects);

        // labelProjects
        this.labelProjects.AutoSize = true;
        this.labelProjects.Dock = DockStyle.Top;
        this.labelProjects.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.labelProjects.Padding = new Padding(0, 0, 0, 5);
        this.labelProjects.Text = "Projets";

        // listViewProjects
        this.listViewProjects.Dock = DockStyle.Fill;
        this.listViewProjects.Font = new Font("Segoe UI", 9F);
        this.listViewProjects.FullRowSelect = true;
        this.listViewProjects.View = View.Details;
        this.listViewProjects.GridLines = true;
        this.listViewProjects.MultiSelect = false;
        this.listViewProjects.Columns.AddRange(new ColumnHeader[] {
            this.columnName,
            this.columnProfile,
            this.columnLastOpened
        });
        this.listViewProjects.SelectedIndexChanged += ListViewProjects_SelectedIndexChanged;
        this.listViewProjects.DoubleClick += ListViewProjects_DoubleClick;
        this.listViewProjects.ColumnClick += ListViewProjects_ColumnClick;

        // columnName
        this.columnName.Text = "Projet";
        this.columnName.Width = 250;

        // columnProfile
        this.columnProfile.Text = "Profil";
        this.columnProfile.Width = 120;

        // columnLastOpened
        this.columnLastOpened.Text = "Dernière ouverture";
        this.columnLastOpened.Width = 150;

        // panelProjectButtons
        this.panelProjectButtons.AutoSize = true;
        this.panelProjectButtons.Dock = DockStyle.Bottom;
        this.panelProjectButtons.FlowDirection = FlowDirection.LeftToRight;
        this.panelProjectButtons.Padding = new Padding(0, 5, 0, 0);
        this.panelProjectButtons.Controls.Add(this.buttonBrowse);
        this.panelProjectButtons.Controls.Add(this.buttonNewProject);
        this.panelProjectButtons.Controls.Add(this.buttonRefresh);
        this.panelProjectButtons.Controls.Add(this.buttonRemoveProject);
        this.panelProjectButtons.Controls.Add(this.buttonSettings);

        // buttonBrowse
        this.buttonBrowse.AutoSize = true;
        this.buttonBrowse.Padding = new Padding(5, 2, 5, 2);
        this.buttonBrowse.Text = "Parcourir...";
        this.buttonBrowse.Click += ButtonBrowse_Click;

        // buttonNewProject
        this.buttonNewProject.AutoSize = true;
        this.buttonNewProject.Padding = new Padding(5, 2, 5, 2);
        this.buttonNewProject.Text = "Nouveau";
        this.buttonNewProject.Click += ButtonNewProject_Click;

        // buttonRemoveProject
        this.buttonRemoveProject.AutoSize = true;
        this.buttonRemoveProject.Padding = new Padding(5, 2, 5, 2);
        this.buttonRemoveProject.Text = "Retirer";
        this.buttonRemoveProject.Click += ButtonRemoveProject_Click;

        // buttonRefresh
        this.buttonRefresh.AutoSize = true;
        this.buttonRefresh.Padding = new Padding(5, 2, 5, 2);
        this.buttonRefresh.Text = "Rafraichir";
        this.buttonRefresh.Click += ButtonRefresh_Click;
        this.toolTip.SetToolTip(this.buttonRefresh, "Rescanne le dossier Projects");

        // buttonSettings
        this.buttonSettings.AutoSize = true;
        this.buttonSettings.Padding = new Padding(5, 2, 5, 2);
        this.buttonSettings.Text = "Parametres";
        this.buttonSettings.Click += ButtonSettings_Click;
        this.toolTip.SetToolTip(this.buttonSettings, "Configurer le dossier Projects et autres options");

        // panelConfig
        this.panelConfig.Dock = DockStyle.Fill;
        this.panelConfig.Padding = new Padding(15, 5, 15, 5);
        this.panelConfig.Controls.Add(this.groupBoxOptions);
        this.panelConfig.Controls.Add(this.groupBoxMcp);
        this.panelConfig.Controls.Add(this.labelProfileDescription);
        this.panelConfig.Controls.Add(this.comboBoxProfile);
        this.panelConfig.Controls.Add(this.labelProfile);

        // labelProfile
        this.labelProfile.AutoSize = true;
        this.labelProfile.Dock = DockStyle.Top;
        this.labelProfile.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.labelProfile.Text = "Profil";

        // comboBoxProfile
        this.comboBoxProfile.Dock = DockStyle.Top;
        this.comboBoxProfile.DropDownStyle = ComboBoxStyle.DropDownList;
        this.comboBoxProfile.Font = new Font("Segoe UI", 10F);
        this.comboBoxProfile.SelectedIndexChanged += ComboBoxProfile_SelectedIndexChanged;

        // labelProfileDescription
        this.labelProfileDescription.AutoSize = true;
        this.labelProfileDescription.Dock = DockStyle.Top;
        this.labelProfileDescription.ForeColor = Color.Gray;
        this.labelProfileDescription.Padding = new Padding(0, 2, 0, 8);
        this.labelProfileDescription.Text = "";

        // groupBoxMcp
        this.groupBoxMcp.Dock = DockStyle.Top;
        this.groupBoxMcp.Height = 70;
        this.groupBoxMcp.Text = "MCP Servers";
        this.groupBoxMcp.Padding = new Padding(10, 5, 10, 5);
        this.groupBoxMcp.Controls.Add(this.flowLayoutMcp);

        // flowLayoutMcp
        this.flowLayoutMcp.Dock = DockStyle.Fill;
        this.flowLayoutMcp.FlowDirection = FlowDirection.LeftToRight;
        this.flowLayoutMcp.AutoScroll = true;

        // groupBoxOptions
        this.groupBoxOptions.Dock = DockStyle.Top;
        this.groupBoxOptions.Height = 90;
        this.groupBoxOptions.Text = "Options";
        this.groupBoxOptions.Padding = new Padding(10, 5, 10, 5);
        this.groupBoxOptions.Controls.Add(this.checkBoxDangerous);
        this.groupBoxOptions.Controls.Add(this.checkBoxTickets);
        this.groupBoxOptions.Controls.Add(this.comboBoxPermissionMode);
        this.groupBoxOptions.Controls.Add(this.labelPermissionMode);
        this.groupBoxOptions.Controls.Add(this.comboBoxModel);
        this.groupBoxOptions.Controls.Add(this.labelModel);

        // labelModel
        this.labelModel.AutoSize = true;
        this.labelModel.Location = new Point(15, 25);
        this.labelModel.Text = "Modele:";

        // comboBoxModel
        this.comboBoxModel.DropDownStyle = ComboBoxStyle.DropDownList;
        this.comboBoxModel.Location = new Point(75, 22);
        this.comboBoxModel.Size = new Size(100, 23);
        this.comboBoxModel.Items.AddRange(new object[] { "haiku", "sonnet", "opus" });
        this.comboBoxModel.SelectedIndex = 1;
        this.toolTip.SetToolTip(this.comboBoxModel, "haiku=rapide, sonnet=equilibre, opus=puissant");

        // labelPermissionMode
        this.labelPermissionMode.AutoSize = true;
        this.labelPermissionMode.Location = new Point(190, 25);
        this.labelPermissionMode.Text = "Permissions:";

        // comboBoxPermissionMode
        this.comboBoxPermissionMode.DropDownStyle = ComboBoxStyle.DropDownList;
        this.comboBoxPermissionMode.Location = new Point(270, 22);
        this.comboBoxPermissionMode.Size = new Size(140, 23);
        this.comboBoxPermissionMode.Items.AddRange(new object[] { "default", "acceptEdits", "plan", "bypassPermissions" });
        this.comboBoxPermissionMode.SelectedIndex = 0;
        this.toolTip.SetToolTip(this.comboBoxPermissionMode, "default=demande, acceptEdits=auto-edits, plan=lecture seule, bypass=tout auto");

        // checkBoxTickets
        this.checkBoxTickets.AutoSize = true;
        this.checkBoxTickets.Location = new Point(15, 55);
        this.checkBoxTickets.Text = "Charger tickets";
        this.toolTip.SetToolTip(this.checkBoxTickets, "Charge les tickets depuis .openspec/tickets/");

        // checkBoxDangerous
        this.checkBoxDangerous.AutoSize = true;
        this.checkBoxDangerous.Location = new Point(150, 55);
        this.checkBoxDangerous.Text = "Dangerously skip permissions";
        this.checkBoxDangerous.ForeColor = Color.OrangeRed;
        this.toolTip.SetToolTip(this.checkBoxDangerous, "Active --dangerously-skip-permissions (bypass toutes les confirmations)");

        // panelFooter
        this.panelFooter.BackColor = Color.FromArgb(45, 45, 48);
        this.panelFooter.Dock = DockStyle.Fill;
        this.panelFooter.Padding = new Padding(15, 10, 15, 10);
        this.panelFooter.Controls.Add(this.buttonCancel);
        this.panelFooter.Controls.Add(this.buttonLaunch);
        this.panelFooter.Controls.Add(this.labelStatus);

        // labelStatus
        this.labelStatus.AutoSize = true;
        this.labelStatus.ForeColor = Color.LightGray;
        this.labelStatus.Location = new Point(15, 22);
        this.labelStatus.Text = "Selectionnez un projet";

        // buttonCancel (à droite)
        this.buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.buttonCancel.FlatStyle = FlatStyle.Flat;
        this.buttonCancel.ForeColor = Color.LightGray;
        this.buttonCancel.Size = new Size(80, 35);
        this.buttonCancel.Text = "Annuler";
        this.buttonCancel.Click += ButtonCancel_Click;

        // buttonLaunch (à gauche de Annuler)
        this.buttonLaunch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.buttonLaunch.BackColor = Color.FromArgb(0, 122, 204);
        this.buttonLaunch.FlatAppearance.BorderSize = 0;
        this.buttonLaunch.FlatStyle = FlatStyle.Flat;
        this.buttonLaunch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.buttonLaunch.ForeColor = Color.White;
        this.buttonLaunch.Size = new Size(130, 35);
        this.buttonLaunch.Text = "Lancer Claude";
        this.buttonLaunch.Enabled = false;
        this.buttonLaunch.Click += ButtonLaunch_Click;

        // MainForm
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new Size(750, 620);
        this.Controls.Add(this.tableLayoutMain);
        this.Font = new Font("Segoe UI", 9F);
        this.MinimumSize = new Size(700, 550);
        this.Name = "MainForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Claude Launcher";
        this.Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "icon.ico"));
        this.Load += MainForm_Load;

        this.tableLayoutMain.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private TableLayoutPanel tableLayoutMain;
    private Panel panelHeader;
    private Label labelTitle;
    private Panel panelProjects;
    private Label labelProjects;
    private ListView listViewProjects;
    private ColumnHeader columnName;
    private ColumnHeader columnProfile;
    private ColumnHeader columnLastOpened;
    private FlowLayoutPanel panelProjectButtons;
    private Button buttonBrowse;
    private Button buttonNewProject;
    private Button buttonRemoveProject;
    private Button buttonRefresh;
    private Button buttonSettings;
    private Panel panelConfig;
    private Label labelProfile;
    private ComboBox comboBoxProfile;
    private Label labelProfileDescription;
    private GroupBox groupBoxMcp;
    private FlowLayoutPanel flowLayoutMcp;
    private GroupBox groupBoxOptions;
    private Label labelModel;
    private ComboBox comboBoxModel;
    private Label labelPermissionMode;
    private ComboBox comboBoxPermissionMode;
    private CheckBox checkBoxTickets;
    private CheckBox checkBoxDangerous;
    private Panel panelFooter;
    private Button buttonLaunch;
    private Button buttonCancel;
    private Label labelStatus;
    private ToolTip toolTip;
}
