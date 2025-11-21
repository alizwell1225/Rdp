namespace LIB_Define.RDP
{
    partial class Uc_Viewer
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            chkTitle = new CheckBox();
            btnHide = new Button();
            btnSmall = new Button();
            btnClose = new Button();
            btnFull = new Button();
            chkFixSize = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(chkFixSize);
            splitContainer1.Panel2.Controls.Add(chkTitle);
            splitContainer1.Panel2.Controls.Add(btnHide);
            splitContainer1.Panel2.Controls.Add(btnSmall);
            splitContainer1.Panel2.Controls.Add(btnClose);
            splitContainer1.Panel2.Controls.Add(btnFull);
            splitContainer1.Size = new Size(674, 543);
            splitContainer1.SplitterDistance = 502;
            splitContainer1.TabIndex = 0;
            // 
            // chkTitle
            // 
            chkTitle.AutoSize = true;
            chkTitle.Checked = true;
            chkTitle.CheckState = CheckState.Checked;
            chkTitle.Dock = DockStyle.Left;
            chkTitle.Location = new Point(252, 0);
            chkTitle.Name = "chkTitle";
            chkTitle.Size = new Size(84, 37);
            chkTitle.TabIndex = 3;
            chkTitle.Text = "Show Title";
            chkTitle.UseVisualStyleBackColor = true;
            // 
            // btnHide
            // 
            btnHide.Dock = DockStyle.Left;
            btnHide.Location = new Point(168, 0);
            btnHide.Margin = new Padding(30, 3, 3, 3);
            btnHide.Name = "btnHide";
            btnHide.Size = new Size(84, 37);
            btnHide.TabIndex = 2;
            btnHide.Text = "Hide";
            btnHide.UseVisualStyleBackColor = true;
            btnHide.Click += btnHide_Click;
            // 
            // btnSmall
            // 
            btnSmall.Dock = DockStyle.Left;
            btnSmall.Location = new Point(84, 0);
            btnSmall.Margin = new Padding(10, 3, 30, 3);
            btnSmall.Name = "btnSmall";
            btnSmall.Size = new Size(84, 37);
            btnSmall.TabIndex = 1;
            btnSmall.Text = "Small";
            btnSmall.UseVisualStyleBackColor = true;
            btnSmall.Click += btnSmall_Click;
            // 
            // btnClose
            // 
            btnClose.Dock = DockStyle.Right;
            btnClose.Location = new Point(590, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(84, 37);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnFull
            // 
            btnFull.Dock = DockStyle.Left;
            btnFull.Location = new Point(0, 0);
            btnFull.Margin = new Padding(30, 3, 30, 3);
            btnFull.Name = "btnFull";
            btnFull.Size = new Size(84, 37);
            btnFull.TabIndex = 0;
            btnFull.Text = "Full";
            btnFull.UseVisualStyleBackColor = true;
            btnFull.Click += btnFull_Click;
            // 
            // chkFixSize
            // 
            chkFixSize.AutoSize = true;
            chkFixSize.Checked = true;
            chkFixSize.CheckState = CheckState.Checked;
            chkFixSize.Dock = DockStyle.Left;
            chkFixSize.Location = new Point(336, 0);
            chkFixSize.Name = "chkFixSize";
            chkFixSize.Size = new Size(67, 37);
            chkFixSize.TabIndex = 4;
            chkFixSize.Text = "Fix Size";
            chkFixSize.UseVisualStyleBackColor = true;
            // 
            // Uc_Viewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "Uc_Viewer";
            Size = new Size(674, 543);
            Load += Uc_Viewer_Load;
            MouseEnter += Uc_Viewer_MouseEnter;
            MouseLeave += Uc_Viewer_MouseLeave;
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnFull;
        private Button btnHide;
        private Button btnSmall;
        private CheckBox chkTitle;
        private CheckBox chkFixSize;
    }
}
