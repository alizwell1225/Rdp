namespace LIB_Log
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

        public void SetDefinePath(string path)
        {
            this.txtLogDirectory.Text=Path.GetDirectoryName(path);
        }

        private void SetupDataGridViewColumns()
        {
            // Prevent auto-generation of columns
            dataGridView.AutoGenerateColumns = false;
            dataGridView.Columns.Clear();

            // Add columns with fixed widths (no resizing for non-fill columns)
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Timestamp",
                DataPropertyName = "LogTime",
                Width = 150,
                Resizable = DataGridViewTriState.False
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Level",
                HeaderText = "Level",
                DataPropertyName = "Level",
                Width = 60,
                Resizable = DataGridViewTriState.False
            });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SubTitle",
                HeaderText = "SubTitle",
                DataPropertyName = "SubTitle",
                Width = 120,
                Resizable = DataGridViewTriState.False
            });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Message",
                HeaderText = "Message",
                DataPropertyName = "Message",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 200
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FileName",
                HeaderText = "File Name",
                DataPropertyName = "FileName",
                Width = 100,
                Resizable = DataGridViewTriState.False
            });
        }

        private void ChkEnableDateFilter_CheckedChanged(object? sender, EventArgs e)
        {
            bool isEnabled = chkEnableDateFilter.Checked;
            dtpStartDate.Enabled = isEnabled;
            dtpEndDate.Enabled = isEnabled;
            dtpStartTime.Enabled = isEnabled;
            dtpEndTime.Enabled = isEnabled;
            btnSetToday.Enabled = isEnabled;
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

                var logFiles = Directory.GetFiles(directory, "*.log", SearchOption.AllDirectories);

                await Task.Run(() =>
                {
                    foreach (var file in logFiles)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(file);
                            var parentDir = Path.GetDirectoryName(file);
                            
                            // Get relative path from base directory
                            string displayFileName;
                            if (!string.IsNullOrEmpty(parentDir) && parentDir.StartsWith(directory))
                            {
                                var relativePath = parentDir.Substring(directory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                displayFileName = string.IsNullOrEmpty(relativePath) ? fileName : $"{relativePath}/{fileName}";
                            }
                            else
                            {
                                displayFileName = fileName;
                            }

                            // Open with FileShare.ReadWrite to allow reading while logger is writing
                            string[] lines;
                            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var reader = new StreamReader(fileStream))
                            {
                                var linesList = new List<string>();
                                string? line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    linesList.Add(line);
                                }
                                lines = linesList.ToArray();
                            }

                            foreach (var line in lines)
                            {
                                var record = ParseLogLine(line, displayFileName);
                                if (record != null)
                                {
                                    _allRecords.Add(record);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log file read error but continue with other files
                            System.Diagnostics.Debug.WriteLine($"Error reading file {file}: {ex.Message}");
                        }
                    }
                });

                _filteredRecords = new List<LogRecord>(_allRecords);
                DoWorkSubTitleCollect();
                UpdateGrid();

                MessageBox.Show($"Loaded {_allRecords.Count} log records from {logFiles.Length} files.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var subTitle = string.Empty;
                if (string.IsNullOrEmpty(message))
                {
                    try
                    {
                        subTitle = parts[3].Trim();
                        message = parts[4].Trim();
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                    subTitle = "Null";
                }
                if (DateTime.TryParseExact(timestampStr, LoggerBase.LogFormatPattern,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var timestamp))
                {
                    return new LogRecord
                    {
                        Timestamp = timestamp,
                        Level = level,
                        SubTitle = subTitle,
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
                    // Combine date and time for precise filtering
                    var startDateTime = dtpStartDate.Value.Date.Add(dtpStartTime.Value.TimeOfDay);
                    var endDateTime = dtpEndDate.Value.Date.Add(dtpEndTime.Value.TimeOfDay);
                    
                    if (r.Timestamp < startDateTime || r.Timestamp > endDateTime)
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
                
                if (cmbLogSubTitle.SelectedIndex >= 0)
                {
                    var selectedLogSubTitle = cmbLogSubTitle.SelectedItem?.ToString();
                    if (!string.Equals(r.SubTitle, selectedLogSubTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();

            UpdateGrid();
        }

        private List<string> SubTitleCollect;

        List<string> DoWorkSubTitleCollect()
        {
            SubTitleCollect = new List<string>();
            cmbLogSubTitle.Items.Clear();
            if (_filteredRecords!=null && _filteredRecords.Count>0)
            {
                SubTitleCollect = _filteredRecords
                    .Select(r => r.SubTitle)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (SubTitleCollect.Count>0 )
                {
                    cmbLogSubTitle.Items.AddRange(SubTitleCollect.ToArray());
                }
                return SubTitleCollect;
            }
            return null;
        }
        private void BtnClearFilter_Click(object? sender, EventArgs e)
        {
            chkEnableDateFilter.Checked = false;
            dtpStartDate.Value = DateTime.Now.AddDays(-7);
            dtpStartTime.Value = DateTime.Today; // 00:00:00
            dtpEndDate.Value = DateTime.Now;
            dtpEndTime.Value = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59); // 23:59:59
            txtKeyword.Clear();
            DoWorkSubTitleCollect();
            cmbLogSubTitle.SelectedIndex = -1;
            cmbLogLevel.SelectedIndex = 0;

            _filteredRecords = new List<LogRecord>(_allRecords);
            UpdateGrid();
        }

        private void BtnSetToday_Click(object? sender, EventArgs e)
        {
            dtpStartDate.Value = DateTime.Today;
            dtpStartTime.Value = DateTime.Today; // 00:00:00
            dtpEndDate.Value = DateTime.Today;
            dtpEndTime.Value = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59); // 23:59:59
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
        public string LogTime { get { return Timestamp.ToString("yyyyMMdd-HH:mm:ss-fff"); } }
        public string Level { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
