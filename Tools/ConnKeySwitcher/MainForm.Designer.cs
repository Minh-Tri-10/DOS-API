using System.Windows.Forms;

namespace ConnKeySwitcher;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private ComboBox cboKey = null!;
    private TextBox txtRoot = null!;
    private Button btnBrowse = null!;
    private Button btnScan = null!;
    private Button btnApply = null!;
    private DataGridView grid = null!;
    private DataGridViewTextBoxColumn colFile = null!;
    private DataGridViewTextBoxColumn colKeys = null!;
    private DataGridViewTextBoxColumn colMatches = null!;
    private DataGridViewCheckBoxColumn colChange = null!;
    private DataGridViewTextBoxColumn colStatus = null!;
    private Label lblKey = null!;
    private Label lblRoot = null!;
    private FolderBrowserDialog folderDialog = null!;
    private StatusStrip status = null!;
    private ToolStripStatusLabel statusText = null!;
    private ToolStripProgressBar statusProgress = null!;
    private TableLayoutPanel layout = null!;
    private FlowLayoutPanel actions = null!;

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
        cboKey = new ComboBox();
        txtRoot = new TextBox();
        btnBrowse = new Button();
        btnScan = new Button();
        btnApply = new Button();
        grid = new DataGridView();
        colFile = new DataGridViewTextBoxColumn();
        colKeys = new DataGridViewTextBoxColumn();
        colMatches = new DataGridViewTextBoxColumn();
        colChange = new DataGridViewCheckBoxColumn();
        colStatus = new DataGridViewTextBoxColumn();
        lblKey = new Label();
        lblRoot = new Label();
        folderDialog = new FolderBrowserDialog();
        status = new StatusStrip();
        statusText = new ToolStripStatusLabel();
        statusProgress = new ToolStripProgressBar();
        layout = new TableLayoutPanel();
        actions = new FlowLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
        status.SuspendLayout();
        layout.SuspendLayout();
        actions.SuspendLayout();
        SuspendLayout();
        // 
        // cboKey
        // 
        cboKey.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cboKey.DropDownStyle = ComboBoxStyle.DropDownList;
        cboKey.Location = new Point(143, 56);
        cboKey.Margin = new Padding(3, 6, 3, 6);
        cboKey.Name = "cboKey";
        cboKey.Size = new Size(564, 33);
        cboKey.TabIndex = 4;
        // 
        // txtRoot
        // 
        txtRoot.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtRoot.Location = new Point(143, 16);
        txtRoot.Margin = new Padding(3, 6, 3, 6);
        txtRoot.Name = "txtRoot";
        txtRoot.Size = new Size(564, 31);
        txtRoot.TabIndex = 1;
        // 
        // btnBrowse
        // 
        btnBrowse.AutoSize = true;
        btnBrowse.Location = new Point(716, 14);
        btnBrowse.Margin = new Padding(6, 4, 0, 4);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(254, 32);
        btnBrowse.TabIndex = 2;
        btnBrowse.Text = "Browse";
        btnBrowse.Click += BtnBrowse_Click;
        // 
        // btnScan
        // 
        btnScan.AutoSize = true;
        btnScan.Location = new Point(6, 4);
        btnScan.Margin = new Padding(6, 4, 0, 4);
        btnScan.Name = "btnScan";
        btnScan.Size = new Size(173, 35);
        btnScan.TabIndex = 2;
        btnScan.Text = "Scan";
        btnScan.Click += BtnScan_Click;
        // 
        // btnApply
        // 
        btnApply.AutoSize = true;
        btnApply.Location = new Point(185, 4);
        btnApply.Margin = new Padding(6, 4, 0, 4);
        btnApply.Name = "btnApply";
        btnApply.Size = new Size(75, 35);
        btnApply.TabIndex = 1;
        btnApply.Text = "Apply";
        btnApply.Click += BtnApply_Click;
        // 
        // grid
        // 
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ColumnHeadersHeight = 34;
        grid.Columns.AddRange(new DataGridViewColumn[] { colFile, colKeys, colMatches, colChange, colStatus });
        grid.Dock = DockStyle.Fill;
        grid.Location = new Point(0, 85);
        grid.Name = "grid";
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.RowHeadersWidth = 62;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.Size = new Size(980, 443);
        grid.TabIndex = 0;
        grid.CellContentClick += grid_CellContentClick;
        // 
        // colFile
        // 
        colFile.FillWeight = 62F;
        colFile.HeaderText = "File";
        colFile.MinimumWidth = 8;
        colFile.Name = "colFile";
        colFile.ReadOnly = true;
        // 
        // colKeys
        // 
        colKeys.FillWeight = 18F;
        colKeys.HeaderText = "Keys Found";
        colKeys.MinimumWidth = 8;
        colKeys.Name = "colKeys";
        colKeys.ReadOnly = true;
        // 
        // colMatches
        // 
        colMatches.FillWeight = 10F;
        colMatches.HeaderText = "Matches";
        colMatches.MinimumWidth = 8;
        colMatches.Name = "colMatches";
        colMatches.ReadOnly = true;
        // 
        // colChange
        // 
        colChange.FillWeight = 10F;
        colChange.HeaderText = "Will Change";
        colChange.MinimumWidth = 8;
        colChange.Name = "colChange";
        colChange.ReadOnly = true;
        // 
        // colStatus
        // 
        colStatus.FillWeight = 10F;
        colStatus.HeaderText = "Status";
        colStatus.MinimumWidth = 8;
        colStatus.Name = "colStatus";
        colStatus.ReadOnly = true;
        // 
        // lblKey
        // 
        lblKey.Anchor = AnchorStyles.Left;
        lblKey.AutoSize = true;
        lblKey.Location = new Point(13, 50);
        lblKey.Name = "lblKey";
        lblKey.Size = new Size(107, 40);
        lblKey.TabIndex = 3;
        lblKey.Text = "Connection key:";
        // 
        // lblRoot
        // 
        lblRoot.Anchor = AnchorStyles.Left;
        lblRoot.AutoSize = true;
        lblRoot.Location = new Point(13, 10);
        lblRoot.Name = "lblRoot";
        lblRoot.Size = new Size(102, 40);
        lblRoot.TabIndex = 0;
        lblRoot.Text = "Repository root:";
        // 
        // status
        // 
        status.ImageScalingSize = new Size(24, 24);
        status.Items.AddRange(new ToolStripItem[] { statusText, statusProgress });
        status.Location = new Point(0, 528);
        status.Name = "status";
        status.Size = new Size(980, 32);
        status.SizingGrip = false;
        status.TabIndex = 2;
        // 
        // statusText
        // 
        statusText.Name = "statusText";
        statusText.Size = new Size(965, 25);
        statusText.Spring = true;
        statusText.Text = "Ready";
        // 
        // statusProgress
        // 
        statusProgress.Alignment = ToolStripItemAlignment.Right;
        statusProgress.Name = "statusProgress";
        statusProgress.Size = new Size(100, 24);
        statusProgress.Visible = false;
        // 
        // layout
        // 
        layout.ColumnCount = 3;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        layout.Controls.Add(lblRoot, 0, 0);
        layout.Controls.Add(txtRoot, 1, 0);
        layout.Controls.Add(lblKey, 0, 1);
        layout.Controls.Add(cboKey, 1, 1);
        layout.Controls.Add(actions, 2, 1);
        layout.Controls.Add(btnBrowse, 2, 0);
        layout.Dock = DockStyle.Top;
        layout.Location = new Point(0, 0);
        layout.Name = "layout";
        layout.Padding = new Padding(10, 10, 10, 8);
        layout.RowCount = 2;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        layout.Size = new Size(980, 85);
        layout.TabIndex = 1;
        // 
        // actions
        // 
        actions.Controls.Add(btnApply);
        actions.Controls.Add(btnScan);
        actions.Dock = DockStyle.Fill;
        actions.FlowDirection = FlowDirection.RightToLeft;
        actions.Location = new Point(710, 52);
        actions.Margin = new Padding(0, 2, 0, 2);
        actions.Name = "actions";
        actions.Size = new Size(260, 36);
        actions.TabIndex = 5;
        actions.WrapContents = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 560);
        Controls.Add(grid);
        Controls.Add(layout);
        Controls.Add(status);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(900, 540);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Connection Key Switcher";
        Load += MainForm_Load;
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        status.ResumeLayout(false);
        status.PerformLayout();
        layout.ResumeLayout(false);
        layout.PerformLayout();
        actions.ResumeLayout(false);
        actions.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
}
