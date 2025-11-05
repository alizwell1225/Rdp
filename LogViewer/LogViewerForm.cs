namespace LogViewer
{
    public partial class LogViewerForm : Form
    {
        private List<LogRecord> _allRecords = new();
        private List<LogRecord> _filteredRecords = new();

        public LogViewerForm()
        {
            InitializeComponent();
            SetupDataGridViewColumns();
        }

        private void SetupDataGridViewColumns()
        {
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

        private void ChkEnableDateFilter_CheckedChanged(object? sender, EventArgs e)
        {
            dtpStartDate.Enabled = chkEnableDateFilter.Checked;
            dtpEndDate.Enabled = chkEnableDateFilter.Checked;
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

                // Load logs from main directory and all date subdirectories
                var allLogFiles = new List<string>();
                
                // Get log files from main directory
                allLogFiles.AddRange(Directory.GetFiles(directory, "*.log", SearchOption.TopDirectoryOnly));
                
                // Get log files from date subdirectories (yyyy-MM-dd format)
                var subdirectories = Directory.GetDirectories(directory);
                foreach (var subdir in subdirectories)
                {
                    var dirName = Path.GetFileName(subdir);
                    // Check if directory name matches date format (yyyy-MM-dd)
                    if (DateTime.TryParseExact(dirName, "yyyy-MM-dd", 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out _))
                    {
                        allLogFiles.AddRange(Directory.GetFiles(subdir, "*.log", SearchOption.TopDirectoryOnly));
                    }
                }

                await Task.Run(() =>
                {
                    foreach (var file in allLogFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var parentDir = Path.GetFileName(Path.GetDirectoryName(file));
                        
                        // If the parent directory is a date folder, include it in the display name
                        var displayFileName = DateTime.TryParseExact(parentDir, "yyyy-MM-dd", 
                            System.Globalization.CultureInfo.InvariantCulture, 
                            System.Globalization.DateTimeStyles.None, out _)
                            ? $"{parentDir}/{fileName}"
                            : fileName;

                        var lines = File.ReadAllLines(file);

                        foreach (var line in lines)
                        {
                            var record = ParseLogLine(line, displayFileName);
                            if (record != null)
                            {
                                _allRecords.Add(record);
                            }
                        }
                    }
                });

                _filteredRecords = new List<LogRecord>(_allRecords);
                UpdateGrid();

                MessageBox.Show($"Loaded {_allRecords.Count} log records from {allLogFiles.Count} files.",
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
                if (chkEnableDateFilter.Checked)
                {
                    if (r.Timestamp.Date < dtpStartDate.Value.Date ||
                        r.Timestamp.Date > dtpEndDate.Value.Date)
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtKeyword.Text))
                {
                    if (!r.Message.Contains(txtKeyword.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

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
