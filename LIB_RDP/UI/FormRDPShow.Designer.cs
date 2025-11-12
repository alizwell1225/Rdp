namespace LIB_RDP.UI
{
    partial class FormRDPShow
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
            splitContainer1 = new SplitContainer();
            bntClose = new Button();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 4, 4, 4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(bntClose);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Size = new Size(1010, 778);
            splitContainer1.SplitterDistance = 676;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // bntClose
            // 
            bntClose.Location = new Point(887, 15);
            bntClose.Margin = new Padding(4, 4, 4, 4);
            bntClose.Name = "bntClose";
            bntClose.Size = new Size(110, 71);
            bntClose.TabIndex = 0;
            bntClose.Text = "關閉 (ESC)";
            bntClose.UseVisualStyleBackColor = true;
            bntClose.Click += bntClose_Click;
            // 
            // button1
            // 
            button1.Location = new Point(757, 15);
            button1.Margin = new Padding(4, 4, 4, 4);
            button1.Name = "button1";
            button1.Size = new Size(110, 71);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // FormRDPShow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1010, 778);
            ControlBox = false;
            Controls.Add(splitContainer1);
            Margin = new Padding(4, 4, 4, 4);
            Name = "FormRDPShow";
            StartPosition = FormStartPosition.Manual;
            Text = "IP:     ";
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button bntClose;
        private System.Windows.Forms.Button button1;
    }
}