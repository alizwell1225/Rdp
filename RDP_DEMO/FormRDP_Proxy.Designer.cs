namespace RDP_DEMO
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
            pnlTop = new System.Windows.Forms.Panel();
            btnSetDemoIP = new System.Windows.Forms.Button();
            lblConnections = new System.Windows.Forms.Label();
            numConnections = new System.Windows.Forms.NumericUpDown();
            dgvConnections = new System.Windows.Forms.DataGridView();
            pnlPreviews = new System.Windows.Forms.Panel();
            _autoHideTimer = new System.Windows.Forms.Timer(components);
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numConnections).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvConnections).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.AutoSize = true;
            pnlTop.Controls.Add(lblConnections);
            pnlTop.Controls.Add(numConnections);
            pnlTop.Controls.Add(dgvConnections);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Margin = new System.Windows.Forms.Padding(4);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new System.Drawing.Size(1475, 197);
            pnlTop.TabIndex = 1;
            pnlTop.MouseEnter += pnlTop_MouseEnter;
            pnlTop.MouseLeave += pnlTop_MouseLeave;
            // 
            // lblConnections
            // 
            lblConnections.AutoSize = true;
            lblConnections.Location = new System.Drawing.Point(1193, 15);
            lblConnections.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblConnections.Name = "lblConnections";
            lblConnections.Size = new System.Drawing.Size(67, 15);
            lblConnections.TabIndex = 0;
            lblConnections.Text = "連線台數：";
            // 
            // numConnections
            // 
            numConnections.Location = new System.Drawing.Point(1275, 13);
            numConnections.Margin = new System.Windows.Forms.Padding(4);
            numConnections.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            numConnections.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numConnections.Name = "numConnections";
            numConnections.Size = new System.Drawing.Size(70, 23);
            numConnections.TabIndex = 1;
            numConnections.Value = new decimal(new int[] { 12, 0, 0, 0 });
            // 
            // dgvConnections
            // 
            dgvConnections.AllowUserToAddRows = false;
            dgvConnections.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvConnections.Location = new System.Drawing.Point(13, 5);
            dgvConnections.Margin = new System.Windows.Forms.Padding(4);
            dgvConnections.Name = "dgvConnections";
            dgvConnections.RowHeadersVisible = false;
            dgvConnections.RowTemplate.Height = 50;
            dgvConnections.Size = new System.Drawing.Size(1157, 188);
            dgvConnections.TabIndex = 2;
            // 
            // pnlPreviews
            // 
            pnlPreviews.AutoScroll = true;
            pnlPreviews.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlPreviews.Location = new System.Drawing.Point(0, 197);
            pnlPreviews.Margin = new System.Windows.Forms.Padding(4);
            pnlPreviews.Name = "pnlPreviews";
            pnlPreviews.Size = new System.Drawing.Size(1475, 714);
            pnlPreviews.TabIndex = 0;
            // 
            // FormRDP
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1475, 911);
            Controls.Add(pnlPreviews);
            Controls.Add(pnlTop);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "FormRDP_Proxy";
            Text = "RDP Proxy 多重連線管理";
            Load += FormRDP_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numConnections).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvConnections).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblConnections;
        private System.Windows.Forms.NumericUpDown numConnections;
        private System.Windows.Forms.DataGridView dgvConnections;
        private System.Windows.Forms.Panel pnlPreviews;
        private System.Windows.Forms.Button btnSetDemoIP ;
    }
}