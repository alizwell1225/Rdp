namespace LIB_RDP.UI
{
    partial class FormShowRdp
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
            uc = new Uc_Viewer();
            SuspendLayout();
            // 
            // uc
            // 
            uc.Dock = DockStyle.Fill;
            uc.Location = new Point(0, 0);
            uc.Margin = new Padding(0, 0, 0, 0);
            uc.Name = "uc";
            uc.Size = new Size(800, 450);
            uc.TabIndex = 0;
            // 
            // FormShowRdp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(uc);
            Name = "FormShowRdp";
            Text = "FormShowRdp";
            Load += FormShowRdp_Load;
            ResumeLayout(false);
        }

        #endregion

        private Uc_Viewer uc;
    }
}