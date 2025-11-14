namespace LIB_Define.RPC.Client
{
    public partial class ImageSendForm : Form
    {
        public ShowPictureType SelectedPictureType { get; private set; } = ShowPictureType.Flow;
        public string ImagePath { get; private set; } = string.Empty;
        
        public ImageSendForm()
        {
            InitializeComponent();
            LoadPictureTypes();
        }

        private void LoadPictureTypes()
        {
            cmbPictureType.Items.Clear();
            cmbPictureType.Items.Add("Flow - Flow Chart");
            cmbPictureType.Items.Add("Map - Map Image");
            cmbPictureType.SelectedIndex = 0;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Title = "Select image to send";
            ofd.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*";
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtImagePath.Text = ofd.FileName;
                ImagePath = ofd.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate image path
                if (string.IsNullOrWhiteSpace(txtImagePath.Text))
                {
                    MessageBox.Show("Please select an image file.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!System.IO.File.Exists(txtImagePath.Text))
                {
                    MessageBox.Show("Selected image file does not exist.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get selected picture type
                var selectedIndex = cmbPictureType.SelectedIndex;
                SelectedPictureType = selectedIndex switch
                {
                    0 => ShowPictureType.Flow,
                    1 => ShowPictureType.Map,
                    _ => ShowPictureType.None
                };

                ImagePath = txtImagePath.Text;
                
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
    }
}
