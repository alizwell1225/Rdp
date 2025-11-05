namespace LIB_Log
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
            filterLayout = new Panel();
            chkEnableDateFilter = new CheckBox();
            lblFrom = new Label();
            dtpStartDate = new DateTimePicker();
            dtpStartTime = new DateTimePicker();
            lblTo = new Label();
            dtpEndDate = new DateTimePicker();
            dtpEndTime = new DateTimePicker();
            btnSetToday = new Button();
            lblKeyword = new Label();
            txtKeyword = new TextBox();
            lblLevel = new Label();
            cmbLogLevel = new ComboBox();
            label1 = new Label();
            cmbLogSubTitle = new ComboBox();
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
            mainPanel.Size = new Size(1076, 761);
            mainPanel.TabIndex = 0;
            // 
            // topPanel
            // 
            topPanel.Controls.Add(topLayout);
            topPanel.Dock = DockStyle.Fill;
            topPanel.Location = new Point(13, 13);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(10);
            topPanel.Size = new Size(1050, 114);
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
            topLayout.Size = new Size(1030, 78);
            topLayout.TabIndex = 0;
            // 
            // lblDirectory
            // 
            topLayout.SetColumnSpan(lblDirectory, 3);
            lblDirectory.Dock = DockStyle.Fill;
            lblDirectory.Location = new Point(8, 5);
            lblDirectory.Name = "lblDirectory";
            lblDirectory.Size = new Size(1014, 20);
            lblDirectory.TabIndex = 0;
            lblDirectory.Text = "Directory:";
            // 
            // txtLogDirectory
            // 
            txtLogDirectory.Dock = DockStyle.Fill;
            txtLogDirectory.Location = new Point(8, 28);
            txtLogDirectory.Name = "txtLogDirectory";
            txtLogDirectory.Size = new Size(814, 23);
            txtLogDirectory.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Location = new Point(828, 28);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 42);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // btnLoadLogs
            // 
            btnLoadLogs.Dock = DockStyle.Fill;
            btnLoadLogs.Location = new Point(928, 28);
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
            gridPanel.Size = new Size(1050, 585);
            gridPanel.TabIndex = 1;
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.Location = new Point(0, 104);
            dataGridView.MultiSelect = false;
            dataGridView.Name = "dataGridView";
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.Size = new Size(1050, 481);
            dataGridView.TabIndex = 0;
            // 
            // filterPanel
            // 
            filterPanel.Controls.Add(filterLayout);
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Location = new Point(0, 0);
            filterPanel.Name = "filterPanel";
            filterPanel.Padding = new Padding(10);
            filterPanel.Size = new Size(1050, 104);
            filterPanel.TabIndex = 1;
            filterPanel.TabStop = false;
            filterPanel.Text = "Filters";
            // 
            // filterLayout
            // 
            filterLayout.Controls.Add(chkEnableDateFilter);
            filterLayout.Controls.Add(lblFrom);
            filterLayout.Controls.Add(dtpStartDate);
            filterLayout.Controls.Add(dtpStartTime);
            filterLayout.Controls.Add(lblTo);
            filterLayout.Controls.Add(dtpEndDate);
            filterLayout.Controls.Add(dtpEndTime);
            filterLayout.Controls.Add(btnSetToday);
            filterLayout.Controls.Add(lblKeyword);
            filterLayout.Controls.Add(txtKeyword);
            filterLayout.Controls.Add(lblLevel);
            filterLayout.Controls.Add(cmbLogLevel);
            filterLayout.Controls.Add(label1);
            filterLayout.Controls.Add(cmbLogSubTitle);
            filterLayout.Controls.Add(btnFilter);
            filterLayout.Controls.Add(btnClearFilter);
            filterLayout.Dock = DockStyle.Fill;
            filterLayout.Location = new Point(10, 26);
            filterLayout.Name = "filterLayout";
            filterLayout.Size = new Size(1030, 68);
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
            dtpStartDate.Size = new Size(100, 23);
            dtpStartDate.TabIndex = 2;
            // 
            // dtpStartTime
            // 
            dtpStartTime.Enabled = false;
            dtpStartTime.Format = DateTimePickerFormat.Time;
            dtpStartTime.Location = new Point(290, 3);
            dtpStartTime.Name = "dtpStartTime";
            dtpStartTime.ShowUpDown = true;
            dtpStartTime.Size = new Size(90, 23);
            dtpStartTime.TabIndex = 3;
            dtpStartTime.Value = new DateTime(2025, 1, 1, 0, 0, 0, 0);
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Location = new Point(148, 32);
            lblTo.Name = "lblTo";
            lblTo.Padding = new Padding(5, 5, 0, 0);
            lblTo.Size = new Size(30, 20);
            lblTo.TabIndex = 4;
            lblTo.Text = "To:";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Enabled = false;
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(184, 35);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(100, 23);
            dtpEndDate.TabIndex = 5;
            dtpEndDate.Value = new DateTime(2025, 11, 5, 13, 25, 25, 725);
            // 
            // dtpEndTime
            // 
            dtpEndTime.Enabled = false;
            dtpEndTime.Format = DateTimePickerFormat.Time;
            dtpEndTime.Location = new Point(290, 35);
            dtpEndTime.Name = "dtpEndTime";
            dtpEndTime.ShowUpDown = true;
            dtpEndTime.Size = new Size(90, 23);
            dtpEndTime.TabIndex = 6;
            dtpEndTime.Value = new DateTime(2025, 1, 1, 23, 59, 59, 0);
            // 
            // btnSetToday
            // 
            btnSetToday.Enabled = false;
            btnSetToday.Location = new Point(31, 28);
            btnSetToday.Name = "btnSetToday";
            btnSetToday.Size = new Size(70, 23);
            btnSetToday.TabIndex = 7;
            btnSetToday.Text = "Today";
            btnSetToday.Click += BtnSetToday_Click;
            // 
            // lblKeyword
            // 
            lblKeyword.AutoSize = true;
            lblKeyword.Location = new Point(394, 3);
            lblKeyword.Name = "lblKeyword";
            lblKeyword.Padding = new Padding(5, 5, 0, 0);
            lblKeyword.Size = new Size(64, 20);
            lblKeyword.TabIndex = 8;
            lblKeyword.Text = "Keyword:";
            // 
            // txtKeyword
            // 
            txtKeyword.Location = new Point(464, 6);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.PlaceholderText = "Keyword...";
            txtKeyword.Size = new Size(150, 23);
            txtKeyword.TabIndex = 9;
            // 
            // lblLevel
            // 
            lblLevel.AutoSize = true;
            lblLevel.Location = new Point(631, 4);
            lblLevel.Name = "lblLevel";
            lblLevel.Padding = new Padding(5, 5, 0, 0);
            lblLevel.Size = new Size(44, 20);
            lblLevel.TabIndex = 10;
            lblLevel.Text = "Level:";
            // 
            // cmbLogLevel
            // 
            cmbLogLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLogLevel.Items.AddRange(new object[] { "All", "Debug", "Info", "Warn", "Error" });
            cmbLogLevel.Location = new Point(681, 7);
            cmbLogLevel.Name = "cmbLogLevel";
            cmbLogLevel.Size = new Size(100, 23);
            cmbLogLevel.TabIndex = 11;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(611, 38);
            label1.Name = "label1";
            label1.Padding = new Padding(5, 5, 0, 0);
            label1.Size = new Size(64, 20);
            label1.TabIndex = 10;
            label1.Text = "Sub Title:";
            // 
            // cmbLogSubTitle
            // 
            cmbLogSubTitle.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLogSubTitle.Location = new Point(681, 38);
            cmbLogSubTitle.Name = "cmbLogSubTitle";
            cmbLogSubTitle.Size = new Size(100, 23);
            cmbLogSubTitle.TabIndex = 11;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(828, 15);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(94, 37);
            btnFilter.TabIndex = 12;
            btnFilter.Text = "Apply Filter";
            btnFilter.Click += BtnFilter_Click;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Location = new Point(928, 15);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(94, 37);
            btnClearFilter.TabIndex = 13;
            btnClearFilter.Text = "Clear Filter";
            btnClearFilter.Click += BtnClearFilter_Click;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.Dock = DockStyle.Fill;
            lblTotalRecords.Location = new Point(13, 721);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(1050, 30);
            lblTotalRecords.TabIndex = 2;
            lblTotalRecords.Text = "Total Records: 0";
            lblTotalRecords.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // LogViewerForm
            // 
            ClientSize = new Size(1076, 761);
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
        private Panel filterLayout;
        private CheckBox chkEnableDateFilter;
        private Label lblFrom;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpStartTime;
        private Label lblTo;
        private DateTimePicker dtpEndDate;
        private DateTimePicker dtpEndTime;
        private Button btnSetToday;
        private Label lblKeyword;
        private TextBox txtKeyword;
        private Label lblLevel;
        private ComboBox cmbLogLevel;
        private Button btnFilter;
        private Button btnClearFilter;
        private DataGridView dataGridView;
        private Panel gridPanel;
        private Label lblTotalRecords;
        private Label label1;
        private ComboBox cmbLogSubTitle;
    }
}
