namespace TestGrpcServerApp
{
    partial class JsonInputForm
    {
        private System.ComponentModel.IContainer components = null;
        
        private Label lblMessageType;
        private Label lblJsonContent;
        private TextBox txtMessageType;
        private TextBox txtJsonContent;
        private Button btnOK;
        private Button btnCancel;
        private Button btnValidate;
        private Button btnFormat;

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
            this.lblMessageType = new Label();
            this.lblJsonContent = new Label();
            this.txtMessageType = new TextBox();
            this.txtJsonContent = new TextBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.btnValidate = new Button();
            this.btnFormat = new Button();
            this.SuspendLayout();
            
            // lblMessageType
            this.lblMessageType.AutoSize = true;
            this.lblMessageType.Location = new Point(20, 20);
            this.lblMessageType.Name = "lblMessageType";
            this.lblMessageType.Size = new Size(85, 15);
            this.lblMessageType.TabIndex = 0;
            this.lblMessageType.Text = "Message Type:";
            
            // txtMessageType
            this.txtMessageType.Location = new Point(120, 17);
            this.txtMessageType.Name = "txtMessageType";
            this.txtMessageType.Size = new Size(400, 23);
            this.txtMessageType.TabIndex = 1;
            
            // lblJsonContent
            this.lblJsonContent.AutoSize = true;
            this.lblJsonContent.Location = new Point(20, 55);
            this.lblJsonContent.Name = "lblJsonContent";
            this.lblJsonContent.Size = new Size(82, 15);
            this.lblJsonContent.TabIndex = 2;
            this.lblJsonContent.Text = "JSON Content:";
            
            // txtJsonContent
            this.txtJsonContent.Location = new Point(20, 75);
            this.txtJsonContent.Multiline = true;
            this.txtJsonContent.Name = "txtJsonContent";
            this.txtJsonContent.ScrollBars = ScrollBars.Vertical;
            this.txtJsonContent.Size = new Size(500, 300);
            this.txtJsonContent.TabIndex = 3;
            this.txtJsonContent.Font = new Font("Consolas", 9F);
            
            // btnValidate
            this.btnValidate.Location = new Point(20, 385);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new Size(90, 30);
            this.btnValidate.TabIndex = 4;
            this.btnValidate.Text = "Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new EventHandler(this.btnValidate_Click);
            
            // btnFormat
            this.btnFormat.Location = new Point(130, 385);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new Size(90, 30);
            this.btnFormat.TabIndex = 5;
            this.btnFormat.Text = "Format";
            this.btnFormat.UseVisualStyleBackColor = true;
            this.btnFormat.Click += new EventHandler(this.btnFormat_Click);
            
            // btnOK
            this.btnOK.Location = new Point(320, 385);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(90, 30);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "Send";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            
            // btnCancel
            this.btnCancel.Location = new Point(430, 385);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(90, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            // JsonInputForm
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(544, 431);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnFormat);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.txtJsonContent);
            this.Controls.Add(this.lblJsonContent);
            this.Controls.Add(this.txtMessageType);
            this.Controls.Add(this.lblMessageType);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JsonInputForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Send JSON Message";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
