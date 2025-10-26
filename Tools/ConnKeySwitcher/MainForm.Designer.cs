using System.Windows.Forms;

namespace ConnKeySwitcher;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private TabControl tabs = null!;
    private TabPage tabSwitch = null!;
    private TabPage tabRun = null!;

    // Switch tab controls
    private Panel panelSwitch = null!;
    private TableLayoutPanel layoutSwitch = null!;
    private Label lblRoot = null!;
    private TextBox txtRoot = null!;
    private Button btnBrowse = null!;
    private Label lblKey = null!;
    private ComboBox cboKey = null!;
    private FlowLayoutPanel switchActions = null!;
    private Button btnScan = null!;
    private Button btnApply = null!;
    private DataGridView grid = null!;
    private DataGridViewTextBoxColumn colFile = null!;
    private DataGridViewTextBoxColumn colKeys = null!;
    private DataGridViewTextBoxColumn colMatches = null!;
    private DataGridViewCheckBoxColumn colChange = null!;
    private DataGridViewTextBoxColumn colStatus = null!;

    // Run tab controls
    private Panel panelRun = null!;
    private TableLayoutPanel runTopLayout = null!;
    private Label lblRunRoot = null!;
    private TextBox txtRunRoot = null!;
    private Button btnRunBrowse = null!;
    private FlowLayoutPanel runActions = null!;
    private Button btnDiscover = null!;
    private Button btnRunSelected = null!;
    private Button btnStopSelected = null!;
    private Button btnStopAll = null!;
    private Button btnSelectAll = null!;
    private Button btnDeselectAll = null!;
    private CheckBox chkRunTerminal = null!;
    private CheckBox chkRunBrowser = null!;
    private SplitContainer runSplit = null!;
    private CheckedListBox lstRunProjects = null!;
    private TextBox txtRunLog = null!;

    private StatusStrip status = null!;
    private ToolStripStatusLabel statusText = null!;
    private ToolStripProgressBar statusProgress = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tabs = new TabControl();
        tabSwitch = new TabPage();
        tabRun = new TabPage();
        panelSwitch = new Panel();
        layoutSwitch = new TableLayoutPanel();
        lblRoot = new Label();
        txtRoot = new TextBox();
        btnBrowse = new Button();
        lblKey = new Label();
        cboKey = new ComboBox();
        switchActions = new FlowLayoutPanel();
        btnScan = new Button();
        btnApply = new Button();
        grid = new DataGridView();
        colFile = new DataGridViewTextBoxColumn();
        colKeys = new DataGridViewTextBoxColumn();
        colMatches = new DataGridViewTextBoxColumn();
        colChange = new DataGridViewCheckBoxColumn();
        colStatus = new DataGridViewTextBoxColumn();
        panelRun = new Panel();
        runTopLayout = new TableLayoutPanel();
        lblRunRoot = new Label();
        txtRunRoot = new TextBox();
        btnRunBrowse = new Button();
        runActions = new FlowLayoutPanel();
        btnDiscover = new Button();
        btnRunSelected = new Button();
        btnStopSelected = new Button();
        btnStopAll = new Button();
        chkRunTerminal = new CheckBox();
        chkRunBrowser = new CheckBox();
        runSplit = new SplitContainer();
        lstRunProjects = new CheckedListBox();
        txtRunLog = new TextBox();
        status = new StatusStrip();
        statusText = new ToolStripStatusLabel();
        statusProgress = new ToolStripProgressBar();

        tabs.SuspendLayout();
        tabSwitch.SuspendLayout();
        panelSwitch.SuspendLayout();
        layoutSwitch.SuspendLayout();
        switchActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
        tabRun.SuspendLayout();
        panelRun.SuspendLayout();
        runTopLayout.SuspendLayout();
        runActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)runSplit).BeginInit();
        runSplit.Panel1.SuspendLayout();
        runSplit.Panel2.SuspendLayout();
        runSplit.SuspendLayout();
        status.SuspendLayout();
        SuspendLayout();

        // tabs
        tabs.Controls.Add(tabSwitch);
        tabs.Controls.Add(tabRun);
        tabs.Dock = DockStyle.Fill;
        tabs.Location = new System.Drawing.Point(0, 0);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new System.Drawing.Size(980, 528);

        // tabSwitch
        tabSwitch.Text = "Switch Connection";
        tabSwitch.Controls.Add(panelSwitch);

        // panelSwitch
        panelSwitch.Dock = DockStyle.Fill;
        panelSwitch.Controls.Add(grid);
        panelSwitch.Controls.Add(layoutSwitch);

        // layoutSwitch
        layoutSwitch.ColumnCount = 3;
        layoutSwitch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        layoutSwitch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutSwitch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
        layoutSwitch.Dock = DockStyle.Top;
        layoutSwitch.Padding = new Padding(10, 10, 10, 6);
        layoutSwitch.RowCount = 2;
        layoutSwitch.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layoutSwitch.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layoutSwitch.Height = 96;

        // lblRoot
        lblRoot.AutoSize = true;
        lblRoot.Text = "Repository root:";
        lblRoot.Anchor = AnchorStyles.Left;

        // txtRoot
        txtRoot.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        // btnBrowse
        btnBrowse.Text = "Browse";
        btnBrowse.AutoSize = true;
        btnBrowse.Margin = new Padding(6, 4, 0, 4);
        btnBrowse.Click += BtnBrowse_Click;

        // lblKey
        lblKey.AutoSize = true;
        lblKey.Text = "Connection key:";
        lblKey.Anchor = AnchorStyles.Left;

        // cboKey
        cboKey.DropDownStyle = ComboBoxStyle.DropDownList;
        cboKey.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        // switchActions
        switchActions.Dock = DockStyle.Fill;
        switchActions.FlowDirection = FlowDirection.RightToLeft;
        switchActions.WrapContents = false;

        btnApply.Text = "Apply";
        btnApply.AutoSize = true;
        btnApply.Margin = new Padding(6, 4, 0, 4);
        btnApply.Click += BtnApply_Click;

        btnScan.Text = "Scan";
        btnScan.AutoSize = true;
        btnScan.Margin = new Padding(6, 4, 0, 4);
        btnScan.Click += BtnScan_Click;

        switchActions.Controls.Add(btnApply);
        switchActions.Controls.Add(btnScan);

        layoutSwitch.Controls.Add(lblRoot, 0, 0);
        layoutSwitch.Controls.Add(txtRoot, 1, 0);
        layoutSwitch.Controls.Add(btnBrowse, 2, 0);
        layoutSwitch.Controls.Add(lblKey, 0, 1);
        layoutSwitch.Controls.Add(cboKey, 1, 1);
        layoutSwitch.Controls.Add(switchActions, 2, 1);

        // grid
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ColumnHeadersHeight = 32;
        grid.Columns.AddRange(new DataGridViewColumn[] { colFile, colKeys, colMatches, colChange, colStatus });
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        colFile.HeaderText = "File";
        colFile.FillWeight = 62F;
        colFile.MinimumWidth = 8;
        colFile.ReadOnly = true;

        colKeys.HeaderText = "Keys Found";
        colKeys.FillWeight = 18F;
        colKeys.MinimumWidth = 8;
        colKeys.ReadOnly = true;

        colMatches.HeaderText = "Matches";
        colMatches.FillWeight = 10F;
        colMatches.MinimumWidth = 8;
        colMatches.ReadOnly = true;

        colChange.HeaderText = "Will Change";
        colChange.FillWeight = 10F;
        colChange.MinimumWidth = 8;
        colChange.ReadOnly = true;

        colStatus.HeaderText = "Status";
        colStatus.FillWeight = 10F;
        colStatus.MinimumWidth = 8;
        colStatus.ReadOnly = true;

        // tabRun
        tabRun.Text = "Run Projects";
        tabRun.Controls.Add(panelRun);

        // panelRun
        panelRun.Dock = DockStyle.Fill;
        panelRun.Controls.Add(runSplit);
        panelRun.Controls.Add(runActions);
        panelRun.Controls.Add(runTopLayout);

        // runTopLayout
        runTopLayout.ColumnCount = 3;
        runTopLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        runTopLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        runTopLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        runTopLayout.Dock = DockStyle.Top;
        runTopLayout.Padding = new Padding(10, 10, 10, 6);
        runTopLayout.RowCount = 1;
        runTopLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

        lblRunRoot.AutoSize = true;
        lblRunRoot.Text = "Repository root:";
        lblRunRoot.Anchor = AnchorStyles.Left;

        txtRunRoot.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        btnRunBrowse.Text = "Browse";
        btnRunBrowse.AutoSize = true;
        btnRunBrowse.Margin = new Padding(6, 4, 0, 4);
        btnRunBrowse.Click += BtnRunBrowse_Click;

        runTopLayout.Controls.Add(lblRunRoot, 0, 0);
        runTopLayout.Controls.Add(txtRunRoot, 1, 0);
        runTopLayout.Controls.Add(btnRunBrowse, 2, 0);

        // runActions
        runActions.Dock = DockStyle.Top;
        runActions.Padding = new Padding(10, 0, 10, 6);
        runActions.Height = 40;
        runActions.FlowDirection = FlowDirection.LeftToRight;
        runActions.WrapContents = false;

        btnDiscover.Text = "Discover";
        btnDiscover.AutoSize = true;
        btnDiscover.Margin = new Padding(0, 6, 6, 6);
        btnDiscover.Click += BtnDiscover_Click;

        btnSelectAll = new Button { Text = "Select All", AutoSize = true, Margin = new Padding(0, 6, 6, 6) };
        btnSelectAll.Click += BtnSelectAll_Click;

        btnDeselectAll = new Button { Text = "Deselect All", AutoSize = true, Margin = new Padding(0, 6, 6, 6) };
        btnDeselectAll.Click += BtnDeselectAll_Click;

        btnRunSelected.Text = "Run Selected";
        btnRunSelected.AutoSize = true;
        btnRunSelected.Margin = new Padding(0, 6, 6, 6);
        btnRunSelected.Click += BtnRunSelected_Click;

        btnStopSelected.Text = "Stop Selected";
        btnStopSelected.AutoSize = true;
        btnStopSelected.Margin = new Padding(0, 6, 6, 6);
        btnStopSelected.Click += BtnStopSelected_Click;

        btnStopAll.Text = "Stop All";
        btnStopAll.AutoSize = true;
        btnStopAll.Margin = new Padding(0, 6, 6, 6);
        btnStopAll.Click += BtnStopAll_Click;

        chkRunTerminal.Text = "Terminal";
        chkRunTerminal.AutoSize = true;
        chkRunTerminal.Checked = true;
        chkRunTerminal.Margin = new Padding(12, 9, 6, 6);

        chkRunBrowser.Text = "Open Browser";
        chkRunBrowser.AutoSize = true;
        chkRunBrowser.Checked = true;
        chkRunBrowser.Margin = new Padding(6, 9, 6, 6);

        runActions.Controls.Add(btnDiscover);
        runActions.Controls.Add(btnSelectAll);
        runActions.Controls.Add(btnDeselectAll);
        runActions.Controls.Add(btnRunSelected);
        runActions.Controls.Add(btnStopSelected);
        runActions.Controls.Add(btnStopAll);
        runActions.Controls.Add(chkRunTerminal);
        runActions.Controls.Add(chkRunBrowser);

        // runSplit
        runSplit.Dock = DockStyle.Fill;
        runSplit.SplitterDistance = 350;

        lstRunProjects.CheckOnClick = true;
        lstRunProjects.Dock = DockStyle.Fill;

        txtRunLog.Dock = DockStyle.Fill;
        txtRunLog.Multiline = true;
        txtRunLog.ReadOnly = true;
        txtRunLog.ScrollBars = ScrollBars.Both;
        txtRunLog.Font = new System.Drawing.Font("Consolas", 9F);

        runSplit.Panel1.Controls.Add(lstRunProjects);
        runSplit.Panel2.Controls.Add(txtRunLog);

        // status
        status.Items.AddRange(new ToolStripItem[] { statusText, statusProgress });
        status.Dock = DockStyle.Bottom;
        status.SizingGrip = false;

        statusText.Spring = true;
        statusText.Text = "Ready";

        statusProgress.Alignment = ToolStripItemAlignment.Right;
        statusProgress.Visible = false;

        // MainForm
        AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(980, 560);
        Controls.Add(tabs);
        Controls.Add(status);
        Font = new System.Drawing.Font("Segoe UI", 9F);
        MinimumSize = new System.Drawing.Size(900, 540);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Connection Key Switcher";
        Load += MainForm_Load;

        runSplit.Panel1.ResumeLayout(false);
        runSplit.Panel2.ResumeLayout(false);
        runSplit.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)runSplit).EndInit();
        runSplit.ResumeLayout(false);
        runActions.ResumeLayout(false);
        runActions.PerformLayout();
        runTopLayout.ResumeLayout(false);
        runTopLayout.PerformLayout();
        panelRun.ResumeLayout(false);
        panelSwitch.ResumeLayout(false);
        layoutSwitch.ResumeLayout(false);
        layoutSwitch.PerformLayout();
        switchActions.ResumeLayout(false);
        switchActions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        tabRun.ResumeLayout(false);
        tabSwitch.ResumeLayout(false);
        tabs.ResumeLayout(false);
        status.ResumeLayout(false);
        status.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
