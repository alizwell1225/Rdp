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
            this.components = new System.ComponentModel.Container();
            
            // Initialize form properties
            this.Text = "Log Viewer";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize main panel
            this.mainPanel = new TableLayoutPanel();
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.RowCount = 3;
            this.mainPanel.ColumnCount = 1;
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            this.mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            // Initialize top panel
            this.topPanel = new GroupBox();
            this.topPanel.Text = "Log Directory";
            this.topPanel.Dock = DockStyle.Fill;
            this.topPanel.Padding = new Padding(10);

            // Initialize top layout
            this.topLayout = new TableLayoutPanel();
            this.topLayout.Dock = DockStyle.Fill;
            this.topLayout.RowCount = 2;
            this.topLayout.ColumnCount = 3;
            this.topLayout.Padding = new Padding(5);
            this.topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            this.topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

            // Initialize directory label
            this.lblDirectory = new Label();
            this.lblDirectory.Text = "Directory:";
            this.lblDirectory.Dock = DockStyle.Fill;

            // Initialize directory textbox
            this.txtLogDirectory = new TextBox();
            this.txtLogDirectory.Dock = DockStyle.Fill;
            this.txtLogDirectory.Text = Path.Combine(AppContext.BaseDirectory, "logs");

            // Initialize browse button
            this.btnBrowse = new Button();
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.Dock = DockStyle.Fill;
            this.btnBrowse.Click += new EventHandler(this.BtnBrowse_Click);

            // Initialize load logs button
            this.btnLoadLogs = new Button();
            this.btnLoadLogs.Text = "Load Logs";
            this.btnLoadLogs.Dock = DockStyle.Fill;
            this.btnLoadLogs.Click += new EventHandler(this.BtnLoadLogs_Click);

            // Initialize filter panel
            this.filterPanel = new GroupBox();
            this.filterPanel.Text = "Filters";
            this.filterPanel.Dock = DockStyle.Top;
            this.filterPanel.Height = 80;
            this.filterPanel.Padding = new Padding(10);

            // Initialize filter layout
            this.filterLayout = new FlowLayoutPanel();
            this.filterLayout.Dock = DockStyle.Fill;
            this.filterLayout.FlowDirection = FlowDirection.LeftToRight;
            this.filterLayout.WrapContents = true;

            // Initialize enable date filter checkbox
            this.chkEnableDateFilter = new CheckBox();
            this.chkEnableDateFilter.Text = "Enable Date Filter";
            this.chkEnableDateFilter.AutoSize = true;
            this.chkEnableDateFilter.Checked = false;
            this.chkEnableDateFilter.CheckedChanged += new EventHandler(this.ChkEnableDateFilter_CheckedChanged);

            // Initialize "From:" label
            this.lblFrom = new Label();
            this.lblFrom.Text = "From:";
            this.lblFrom.AutoSize = true;
            this.lblFrom.Padding = new Padding(5, 5, 0, 0);

            // Initialize start date picker
            this.dtpStartDate = new DateTimePicker();
            this.dtpStartDate.Format = DateTimePickerFormat.Short;
            this.dtpStartDate.Width = 120;
            this.dtpStartDate.Enabled = false;

            // Initialize "To:" label
            this.lblTo = new Label();
            this.lblTo.Text = "To:";
            this.lblTo.AutoSize = true;
            this.lblTo.Padding = new Padding(5, 5, 0, 0);

            // Initialize end date picker
            this.dtpEndDate = new DateTimePicker();
            this.dtpEndDate.Format = DateTimePickerFormat.Short;
            this.dtpEndDate.Width = 120;
            this.dtpEndDate.Value = DateTime.Now;
            this.dtpEndDate.Enabled = false;

            // Initialize "Keyword:" label
            this.lblKeyword = new Label();
            this.lblKeyword.Text = "Keyword:";
            this.lblKeyword.AutoSize = true;
            this.lblKeyword.Padding = new Padding(5, 5, 0, 0);

            // Initialize keyword textbox
            this.txtKeyword = new TextBox();
            this.txtKeyword.PlaceholderText = "Keyword...";
            this.txtKeyword.Width = 200;

            // Initialize "Level:" label
            this.lblLevel = new Label();
            this.lblLevel.Text = "Level:";
            this.lblLevel.AutoSize = true;
            this.lblLevel.Padding = new Padding(5, 5, 0, 0);

            // Initialize log level combobox
            this.cmbLogLevel = new ComboBox();
            this.cmbLogLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbLogLevel.Width = 100;
            this.cmbLogLevel.Items.AddRange(new object[] { "All", "Debug", "Info", "Warn", "Error" });
            this.cmbLogLevel.SelectedIndex = 0;

            // Initialize filter button
            this.btnFilter = new Button();
            this.btnFilter.Text = "Apply Filter";
            this.btnFilter.Width = 100;
            this.btnFilter.Click += new EventHandler(this.BtnFilter_Click);

            // Initialize clear filter button
            this.btnClearFilter = new Button();
            this.btnClearFilter.Text = "Clear Filter";
            this.btnClearFilter.Width = 100;
            this.btnClearFilter.Click += new EventHandler(this.BtnClearFilter_Click);

            // Initialize data grid view
            this.dataGridView = new DataGridView();
            this.dataGridView.Dock = DockStyle.Fill;
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ReadOnly = true;
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.MultiSelect = false;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Initialize grid panel
            this.gridPanel = new Panel();
            this.gridPanel.Dock = DockStyle.Fill;

            // Initialize total records label
            this.lblTotalRecords = new Label();
            this.lblTotalRecords.Text = "Total Records: 0";
            this.lblTotalRecords.Dock = DockStyle.Fill;
            this.lblTotalRecords.AutoSize = false;
            this.lblTotalRecords.TextAlign = ContentAlignment.MiddleLeft;

            // Add controls to layouts
            this.topLayout.Controls.Add(this.lblDirectory, 0, 0);
            this.topLayout.SetColumnSpan(this.lblDirectory, 3);
            this.topLayout.Controls.Add(this.txtLogDirectory, 0, 1);
            this.topLayout.Controls.Add(this.btnBrowse, 1, 1);
            this.topLayout.Controls.Add(this.btnLoadLogs, 2, 1);

            this.topPanel.Controls.Add(this.topLayout);

            this.filterLayout.Controls.Add(this.chkEnableDateFilter);
            this.filterLayout.Controls.Add(this.lblFrom);
            this.filterLayout.Controls.Add(this.dtpStartDate);
            this.filterLayout.Controls.Add(this.lblTo);
            this.filterLayout.Controls.Add(this.dtpEndDate);
            this.filterLayout.Controls.Add(this.lblKeyword);
            this.filterLayout.Controls.Add(this.txtKeyword);
            this.filterLayout.Controls.Add(this.lblLevel);
            this.filterLayout.Controls.Add(this.cmbLogLevel);
            this.filterLayout.Controls.Add(this.btnFilter);
            this.filterLayout.Controls.Add(this.btnClearFilter);

            this.filterPanel.Controls.Add(this.filterLayout);

            this.gridPanel.Controls.Add(this.dataGridView);
            this.gridPanel.Controls.Add(this.filterPanel);

            this.mainPanel.Controls.Add(this.topPanel, 0, 0);
            this.mainPanel.Controls.Add(this.gridPanel, 0, 1);
            this.mainPanel.Controls.Add(this.lblTotalRecords, 0, 2);

            this.Controls.Add(this.mainPanel);
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
