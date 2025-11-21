namespace LIB_Define.RPC.Server
{
    partial class ImageSendForm
    {
        private System.ComponentModel.IContainer components = null;
        
        private Label lblPictureType;
        private Label lblImagePath;
        private ComboBox cmbPictureType;
        private TextBox txtImagePath;
        private Button btnBrowse;
        private Button btnOK;
        private Button btnCancel;

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
            this.lblPictureType = new Label();
            this.lblImagePath = new Label();
            this.cmbPictureType = new ComboBox();
            this.txtImagePath = new TextBox();
            this.btnBrowse = new Button();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            
            // lblPictureType
            this.lblPictureType.AutoSize = true;
            this.lblPictureType.Location = new Point(20, 20);
            this.lblPictureType.Name = "lblPictureType";
            this.lblPictureType.Size = new Size(75, 15);
            this.lblPictureType.TabIndex = 0;
            this.lblPictureType.Text = "Picture Type:";
            
            // cmbPictureType
            this.cmbPictureType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPictureType.FormattingEnabled = true;
            this.cmbPictureType.Location = new Point(120, 17);
            this.cmbPictureType.Name = "cmbPictureType";
            this.cmbPictureType.Size = new Size(300, 23);
            this.cmbPictureType.TabIndex = 1;
            
            // lblImagePath
            this.lblImagePath.AutoSize = true;
            this.lblImagePath.Location = new Point(20, 60);
            this.lblImagePath.Name = "lblImagePath";
            this.lblImagePath.Size = new Size(73, 15);
            this.lblImagePath.TabIndex = 2;
            this.lblImagePath.Text = "Image Path:";
            
            // txtImagePath
            this.txtImagePath.Location = new Point(120, 57);
            this.txtImagePath.Name = "txtImagePath";
            this.txtImagePath.ReadOnly = true;
            this.txtImagePath.Size = new Size(250, 23);
            this.txtImagePath.TabIndex = 3;
            
            // btnBrowse
            this.btnBrowse.Location = new Point(380, 56);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(40, 25);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
            
            // btnOK
            this.btnOK.Location = new Point(220, 110);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(90, 30);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "Send";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            
            // btnCancel
            this.btnCancel.Location = new Point(330, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(90, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            // ImageSendForm
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(444, 161);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtImagePath);
            this.Controls.Add(this.lblImagePath);
            this.Controls.Add(this.cmbPictureType);
            this.Controls.Add(this.lblPictureType);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageSendForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Send Image to Clients";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
