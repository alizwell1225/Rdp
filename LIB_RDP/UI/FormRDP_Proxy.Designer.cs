namespace LIB_RDP.UI
{
    partial class FormRDP_Proxy
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
            components = new System.ComponentModel.Container();
            pnlPreviews = new Panel();
            SuspendLayout();
            // 
            // pnlPreviews
            // 
            pnlPreviews.AutoScroll = true;
            pnlPreviews.Dock = DockStyle.Fill;
            pnlPreviews.Location = new Point(0, 0);
            pnlPreviews.Margin = new Padding(4);
            pnlPreviews.Name = "pnlPreviews";
            pnlPreviews.Size = new Size(1475, 911);
            pnlPreviews.TabIndex = 0;
            // 
            // FormRDP_Proxy
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1475, 911);
            Controls.Add(pnlPreviews);
            Margin = new Padding(4);
            Name = "FormRDP_Proxy";
            Text = "RDP Proxy 控制器";
            Load += FormRDP_Proxy_Load;
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlPreviews;
    }
}
