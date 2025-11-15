using System.Text.Json;

namespace LIB_Define.RPC.Client
{
    public partial class FormJsonInput : Form
    {
        public string MessageType { get; private set; } = "test_message";
        public string JsonContent { get; private set; } = "{}";
        
        public FormJsonInput()
        {
            InitializeComponent();
            LoadDefaultValues();
        }

        private void LoadDefaultValues()
        {
            txtMessageType.Text = "test_message";
            txtJsonContent.Text = @"{
  ""message"": ""Hello from test application"",
  ""timestamp"": """ + DateTime.Now.ToString("o") + @""",
  ""data"": {
    ""value"": 123
  }
}";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate message type
                if (string.IsNullOrWhiteSpace(txtMessageType.Text))
                {
                    MessageBox.Show("Message type cannot be empty.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate JSON
                if (string.IsNullOrWhiteSpace(txtJsonContent.Text))
                {
                    MessageBox.Show("JSON content cannot be empty.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Try to parse JSON to validate
                    JsonDocument.Parse(txtJsonContent.Text);
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Invalid JSON format: {ex.Message}", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageType = txtMessageType.Text.Trim();
                JsonContent = txtJsonContent.Text.Trim();
                
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            try
            {
                JsonDocument.Parse(txtJsonContent.Text);
                MessageBox.Show("JSON is valid!", "Validation Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Invalid JSON: {ex.Message}", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(txtJsonContent.Text);
                var formatted = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                txtJsonContent.Text = formatted;
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Cannot format invalid JSON: {ex.Message}", "Format Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
