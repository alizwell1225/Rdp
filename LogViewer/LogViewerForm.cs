using LIB_RPC.Logging;

namespace LogViewer
{
    public partial class LogViewerForm : Form
    {
        private DataGridView dataGridView;
        private TextBox txtLogDirectory;
        private Button btnBrowse;
        private Button btnLoadLogs;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private TextBox txtKeyword;
        private ComboBox cmbLogLevel;
        private Button btnFilter;
        private Button btnClearFilter;
        private Label lblTotalRecords;
        private CheckBox chkEnableDateFilter;
        
        private List<LogRecord> _allRecords = new();
        private List<LogRecord> _filteredRecords = new();

        public LogViewerForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            this.Text = "Log Viewer";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(10)
            };

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            // Top panel for path and load
            var topPanel = new GroupBox
            {
                Text = "Log Directory",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var topLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 3,
                Padding = new Padding(5)
            };

            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            var lblDirectory = new Label { Text = "Directory:", Dock = DockStyle.Fill };

            txtLogDirectory = new TextBox
            {
                Dock = DockStyle.Fill,
                Text = Path.Combine(AppContext.BaseDirectory, "logs")
            };

            btnBrowse = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill
            };
            btnBrowse.Click += BtnBrowse_Click;

            btnLoadLogs = new Button
            {
                Text = "Load Logs",
                Dock = DockStyle.Fill
            };
            btnLoadLogs.Click += BtnLoadLogs_Click;

            topLayout.Controls.Add(lblDirectory, 0, 0);
            topLayout.SetColumnSpan(lblDirectory, 3);
            topLayout.Controls.Add(txtLogDirectory, 0, 1);
            topLayout.Controls.Add(btnBrowse, 1, 1);
            topLayout.Controls.Add(btnLoadLogs, 2, 1);

            topPanel.Controls.Add(topLayout);

            // Filter panel
            var filterPanel = new GroupBox
            {
                Text = "Filters",
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            var filterLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            chkEnableDateFilter = new CheckBox
            {
                Text = "Enable Date Filter",
                AutoSize = true,
                Checked = false
            };
            chkEnableDateFilter.CheckedChanged += (s, e) =>
            {
                dtpStartDate.Enabled = chkEnableDateFilter.Checked;
                dtpEndDate.Enabled = chkEnableDateFilter.Checked;
            };

            dtpStartDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Enabled = false
            };

            dtpEndDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Now,
                Enabled = false
            };

            txtKeyword = new TextBox
            {
                PlaceholderText = "Keyword...",
                Width = 200
            };

            cmbLogLevel = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100
            };
            cmbLogLevel.Items.AddRange(new object[] { "All", "Debug", "Info", "Warn", "Error" });
            cmbLogLevel.SelectedIndex = 0;

            btnFilter = new Button
            {
                Text = "Apply Filter",
                Width = 100
            };
            btnFilter.Click += BtnFilter_Click;

            btnClearFilter = new Button
            {
                Text = "Clear Filter",
                Width = 100
            };
            btnClearFilter.Click += BtnClearFilter_Click;

            filterLayout.Controls.Add(chkEnableDateFilter);
            filterLayout.Controls.Add(new Label { Text = "From:", AutoSize = true, Padding = new Padding(5, 5, 0, 0) });
            filterLayout.Controls.Add(dtpStartDate);
            filterLayout.Controls.Add(new Label { Text = "To:", AutoSize = true, Padding = new Padding(5, 5, 0, 0) });
            filterLayout.Controls.Add(dtpEndDate);
            filterLayout.Controls.Add(new Label { Text = "Keyword:", AutoSize = true, Padding = new Padding(5, 5, 0, 0) });
            filterLayout.Controls.Add(txtKeyword);
            filterLayout.Controls.Add(new Label { Text = "Level:", AutoSize = true, Padding = new Padding(5, 5, 0, 0) });
            filterLayout.Controls.Add(cmbLogLevel);
            filterLayout.Controls.Add(btnFilter);
            filterLayout.Controls.Add(btnClearFilter);

            filterPanel.Controls.Add(filterLayout);

            // DataGridView for logs
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };

            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            gridPanel.Controls.Add(dataGridView);
            gridPanel.Controls.Add(filterPanel);

            // Status panel
            lblTotalRecords = new Label
            {
                Text = "Total Records: 0",
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            mainPanel.Controls.Add(topPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            mainPanel.Controls.Add(lblTotalRecords, 0, 2);

            this.Controls.Add(mainPanel);
        }

        private void InitializeCustomComponents()
        {
            // Set up DataGridView columns
            dataGridView.AutoGenerateColumns = false;
            dataGridView.Columns.Clear();

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Timestamp",
                DataPropertyName = "Timestamp",
                Width = 180
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Level",
                HeaderText = "Level",
                DataPropertyName = "Level",
                Width = 80
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Message",
                HeaderText = "Message",
                DataPropertyName = "Message",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FileName",
                HeaderText = "File Name",
                DataPropertyName = "FileName",
                Width = 200
            });
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select log directory",
                UseDescriptionForTitle = true,
                SelectedPath = txtLogDirectory.Text
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtLogDirectory.Text = folderDialog.SelectedPath;
            }
        }

        private async void BtnLoadLogs_Click(object? sender, EventArgs e)
        {
            var directory = txtLogDirectory.Text;

            if (!Directory.Exists(directory))
            {
                MessageBox.Show("Directory does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnLoadLogs.Enabled = false;
            btnLoadLogs.Text = "Loading...";

            try
            {
                _allRecords.Clear();

                var logFiles = Directory.GetFiles(directory, "*.log", SearchOption.TopDirectoryOnly);

                await Task.Run(() =>
                {
                    foreach (var file in logFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var lines = File.ReadAllLines(file);

                        foreach (var line in lines)
                        {
                            var record = ParseLogLine(line, fileName);
                            if (record != null)
                            {
                                _allRecords.Add(record);
                            }
                        }
                    }
                });

                _filteredRecords = new List<LogRecord>(_allRecords);
                UpdateGrid();

                MessageBox.Show($"Loaded {_allRecords.Count} log records from {logFiles.Length} files.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLoadLogs.Enabled = true;
                btnLoadLogs.Text = "Load Logs";
            }
        }

        private LogRecord? ParseLogLine(string line, string fileName)
        {
            try
            {
                // Expected format from LoggerBase.LogFormatPattern: yyyy-MM-dd HH:mm:ss.fff [LEVEL] Message
                var parts = line.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) return null;

                var timestampStr = parts[0].Trim();
                var level = parts[1].Trim();
                var message = parts[2].Trim();

                if (DateTime.TryParseExact(timestampStr, LoggerBase.LogFormatPattern, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out var timestamp))
                {
                    return new LogRecord
                    {
                        Timestamp = timestamp,
                        Level = level,
                        Message = message,
                        FileName = fileName
                    };
                }
            }
            catch
            {
                // Ignore malformed lines
            }

            return null;
        }

        private void BtnFilter_Click(object? sender, EventArgs e)
        {
            _filteredRecords = _allRecords.Where(r =>
            {
                // Date filter
                if (chkEnableDateFilter.Checked)
                {
                    if (r.Timestamp.Date < dtpStartDate.Value.Date ||
                        r.Timestamp.Date > dtpEndDate.Value.Date)
                    {
                        return false;
                    }
                }

                // Keyword filter
                if (!string.IsNullOrWhiteSpace(txtKeyword.Text))
                {
                    if (!r.Message.Contains(txtKeyword.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                // Level filter
                if (cmbLogLevel.SelectedIndex > 0)
                {
                    var selectedLevel = cmbLogLevel.SelectedItem?.ToString();
                    if (!string.Equals(r.Level, selectedLevel, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();

            UpdateGrid();
        }

        private void BtnClearFilter_Click(object? sender, EventArgs e)
        {
            chkEnableDateFilter.Checked = false;
            dtpStartDate.Value = DateTime.Now.AddDays(-7);
            dtpEndDate.Value = DateTime.Now;
            txtKeyword.Clear();
            cmbLogLevel.SelectedIndex = 0;

            _filteredRecords = new List<LogRecord>(_allRecords);
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            dataGridView.DataSource = null;
            dataGridView.DataSource = _filteredRecords;
            lblTotalRecords.Text = $"Total Records: {_filteredRecords.Count} (of {_allRecords.Count})";
        }
    }

    public class LogRecord
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
