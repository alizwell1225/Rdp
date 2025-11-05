namespace LogViewer
{
    partial class LogViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainPanel = new TableLayoutPanel();
            topPanel = new GroupBox();
            topLayout = new TableLayoutPanel();
            lblDirectory = new Label();
            txtLogDirectory = new TextBox();
            btnBrowse = new Button();
            btnLoadLogs = new Button();
            gridPanel = new Panel();
            dataGridView = new DataGridView();
            filterPanel = new GroupBox();
            filterLayout = new FlowLayoutPanel();
            chkEnableDateFilter = new CheckBox();
            lblFrom = new Label();
            dtpStartDate = new DateTimePicker();
            lblTo = new Label();
            dtpEndDate = new DateTimePicker();
            lblKeyword = new Label();
            txtKeyword = new TextBox();
            lblLevel = new Label();
            cmbLogLevel = new ComboBox();
            btnFilter = new Button();
            btnClearFilter = new Button();
            lblTotalRecords = new Label();
            mainPanel.SuspendLayout();
            topPanel.SuspendLayout();
            topLayout.SuspendLayout();
            gridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            filterPanel.SuspendLayout();
            filterLayout.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            mainPanel.Controls.Add(topPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            mainPanel.Controls.Add(lblTotalRecords, 0, 2);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(10);
            mainPanel.RowCount = 3;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainPanel.Size = new Size(1184, 761);
            mainPanel.TabIndex = 0;
            // 
            // topPanel
            // 
            topPanel.Controls.Add(topLayout);
            topPanel.Dock = DockStyle.Fill;
            topPanel.Location = new Point(13, 13);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(10);
            topPanel.Size = new Size(1158, 114);
            topPanel.TabIndex = 0;
            topPanel.TabStop = false;
            topPanel.Text = "Log Directory";
            // 
            // topLayout
            // 
            topLayout.ColumnCount = 3;
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            topLayout.Controls.Add(lblDirectory, 0, 0);
            topLayout.Controls.Add(txtLogDirectory, 0, 1);
            topLayout.Controls.Add(btnBrowse, 1, 1);
            topLayout.Controls.Add(btnLoadLogs, 2, 1);
            topLayout.Dock = DockStyle.Fill;
            topLayout.Location = new Point(10, 26);
            topLayout.Name = "topLayout";
            topLayout.Padding = new Padding(5);
            topLayout.RowCount = 2;
            topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            topLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            topLayout.Size = new Size(1138, 78);
            topLayout.TabIndex = 0;
            // 
            // lblDirectory
            // 
            topLayout.SetColumnSpan(lblDirectory, 3);
            lblDirectory.Dock = DockStyle.Fill;
            lblDirectory.Location = new Point(8, 5);
            lblDirectory.Name = "lblDirectory";
            lblDirectory.Size = new Size(1122, 20);
            lblDirectory.TabIndex = 0;
            lblDirectory.Text = "Directory:";
            // 
            // txtLogDirectory
            // 
            txtLogDirectory.Dock = DockStyle.Fill;
            txtLogDirectory.Location = new Point(8, 28);
            txtLogDirectory.Name = "txtLogDirectory";
            txtLogDirectory.Size = new Size(922, 23);
            txtLogDirectory.TabIndex = 1;
            txtLogDirectory.Text = "C:\\Users\\Gavin\\AppData\\Local\\Microsoft\\VisualStudio\\17.0_3c87b0e4\\WinFormsDesigner\\bijdg1dg.1cr\\logs";
            // 
            // btnBrowse
            // 
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Location = new Point(936, 28);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 42);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // btnLoadLogs
            // 
            btnLoadLogs.Dock = DockStyle.Fill;
            btnLoadLogs.Location = new Point(1036, 28);
            btnLoadLogs.Name = "btnLoadLogs";
            btnLoadLogs.Size = new Size(94, 42);
            btnLoadLogs.TabIndex = 3;
            btnLoadLogs.Text = "Load Logs";
            btnLoadLogs.Click += BtnLoadLogs_Click;
            // 
            // gridPanel
            // 
            gridPanel.Controls.Add(dataGridView);
            gridPanel.Controls.Add(filterPanel);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Location = new Point(13, 133);
            gridPanel.Name = "gridPanel";
            gridPanel.Size = new Size(1158, 585);
            gridPanel.TabIndex = 1;
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.Location = new Point(0, 80);
            dataGridView.MultiSelect = false;
            dataGridView.Name = "dataGridView";
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.Size = new Size(1158, 505);
            dataGridView.TabIndex = 0;
            // 
            // filterPanel
            // 
            filterPanel.Controls.Add(filterLayout);
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Location = new Point(0, 0);
            filterPanel.Name = "filterPanel";
            filterPanel.Padding = new Padding(10);
            filterPanel.Size = new Size(1158, 80);
            filterPanel.TabIndex = 1;
            filterPanel.TabStop = false;
            filterPanel.Text = "Filters";
            // 
            // filterLayout
            // 
            filterLayout.Controls.Add(chkEnableDateFilter);
            filterLayout.Controls.Add(lblFrom);
            filterLayout.Controls.Add(dtpStartDate);
            filterLayout.Controls.Add(lblTo);
            filterLayout.Controls.Add(dtpEndDate);
            filterLayout.Controls.Add(lblKeyword);
            filterLayout.Controls.Add(txtKeyword);
            filterLayout.Controls.Add(lblLevel);
            filterLayout.Controls.Add(cmbLogLevel);
            filterLayout.Controls.Add(btnFilter);
            filterLayout.Controls.Add(btnClearFilter);
            filterLayout.Dock = DockStyle.Fill;
            filterLayout.Location = new Point(10, 26);
            filterLayout.Name = "filterLayout";
            filterLayout.Size = new Size(1138, 44);
            filterLayout.TabIndex = 0;
            // 
            // chkEnableDateFilter
            // 
            chkEnableDateFilter.AutoSize = true;
            chkEnableDateFilter.Location = new Point(3, 3);
            chkEnableDateFilter.Name = "chkEnableDateFilter";
            chkEnableDateFilter.Size = new Size(125, 19);
            chkEnableDateFilter.TabIndex = 0;
            chkEnableDateFilter.Text = "Enable Date Filter";
            chkEnableDateFilter.CheckedChanged += ChkEnableDateFilter_CheckedChanged;
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.Location = new Point(134, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.Padding = new Padding(5, 5, 0, 0);
            lblFrom.Size = new Size(44, 20);
            lblFrom.TabIndex = 1;
            lblFrom.Text = "From:";
            // 
            // dtpStartDate
            // 
            dtpStartDate.Enabled = false;
            dtpStartDate.Format = DateTimePickerFormat.Short;
            dtpStartDate.Location = new Point(184, 3);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(120, 23);
            dtpStartDate.TabIndex = 2;
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Location = new Point(310, 0);
            lblTo.Name = "lblTo";
            lblTo.Padding = new Padding(5, 5, 0, 0);
            lblTo.Size = new Size(30, 20);
            lblTo.TabIndex = 3;
            lblTo.Text = "To:";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Enabled = false;
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(346, 3);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(120, 23);
            dtpEndDate.TabIndex = 4;
            dtpEndDate.Value = new DateTime(2025, 11, 5, 13, 25, 25, 725);
            // 
            // lblKeyword
            // 
            lblKeyword.AutoSize = true;
            lblKeyword.Location = new Point(472, 0);
            lblKeyword.Name = "lblKeyword";
            lblKeyword.Padding = new Padding(5, 5, 0, 0);
            lblKeyword.Size = new Size(64, 20);
            lblKeyword.TabIndex = 5;
            lblKeyword.Text = "Keyword:";
            // 
            // txtKeyword
            // 
            txtKeyword.Location = new Point(542, 3);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.PlaceholderText = "Keyword...";
            txtKeyword.Size = new Size(200, 23);
            txtKeyword.TabIndex = 6;
            // 
            // lblLevel
            // 
            lblLevel.AutoSize = true;
            lblLevel.Location = new Point(748, 0);
            lblLevel.Name = "lblLevel";
            lblLevel.Padding = new Padding(5, 5, 0, 0);
            lblLevel.Size = new Size(44, 20);
            lblLevel.TabIndex = 7;
            lblLevel.Text = "Level:";
            // 
            // cmbLogLevel
            // 
            cmbLogLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLogLevel.Items.AddRange(new object[] { "All", "Debug", "Info", "Warn", "Error" });
            cmbLogLevel.Location = new Point(798, 3);
            cmbLogLevel.Name = "cmbLogLevel";
            cmbLogLevel.Size = new Size(100, 23);
            cmbLogLevel.TabIndex = 8;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(904, 3);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(100, 23);
            btnFilter.TabIndex = 9;
            btnFilter.Text = "Apply Filter";
            btnFilter.Click += BtnFilter_Click;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Location = new Point(1010, 3);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(100, 23);
            btnClearFilter.TabIndex = 10;
            btnClearFilter.Text = "Clear Filter";
            btnClearFilter.Click += BtnClearFilter_Click;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.Dock = DockStyle.Fill;
            lblTotalRecords.Location = new Point(13, 721);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(1158, 30);
            lblTotalRecords.TabIndex = 2;
            lblTotalRecords.Text = "Total Records: 0";
            lblTotalRecords.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LogViewerForm
            // 
            ClientSize = new Size(1184, 761);
            Controls.Add(mainPanel);
            Name = "LogViewerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Log Viewer";
            mainPanel.ResumeLayout(false);
            topPanel.ResumeLayout(false);
            topLayout.ResumeLayout(false);
            topLayout.PerformLayout();
            gridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            filterPanel.ResumeLayout(false);
            filterLayout.ResumeLayout(false);
            filterLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel mainPanel;
        private GroupBox topPanel;
        private TableLayoutPanel topLayout;
        private Label lblDirectory;
        private TextBox txtLogDirectory;
        private Button btnBrowse;
        private Button btnLoadLogs;
        private GroupBox filterPanel;
        private FlowLayoutPanel filterLayout;
        private CheckBox chkEnableDateFilter;
        private Label lblFrom;
        private DateTimePicker dtpStartDate;
        private Label lblTo;
        private DateTimePicker dtpEndDate;
        private Label lblKeyword;
        private TextBox txtKeyword;
        private Label lblLevel;
        private ComboBox cmbLogLevel;
        private Button btnFilter;
        private Button btnClearFilter;
        private DataGridView dataGridView;
        private Panel gridPanel;
        private Label lblTotalRecords;
    }
}
