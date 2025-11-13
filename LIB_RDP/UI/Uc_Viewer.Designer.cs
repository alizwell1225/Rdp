namespace LIB_RDP.UI
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
            btnHide = new Button();
            btnSink = new Button();
            btnFull = new Button();
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
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnHide);
            splitContainer1.Panel2.Controls.Add(btnSink);
            splitContainer1.Panel2.Controls.Add(btnFull);
            splitContainer1.Size = new Size(674, 543);
            splitContainer1.SplitterDistance = 502;
            splitContainer1.TabIndex = 0;
            // 
            // btnHide
            // 
            btnHide.Dock = DockStyle.Right;
            btnHide.Location = new Point(590, 0);
            btnHide.Name = "btnHide";
            btnHide.Size = new Size(84, 37);
            btnHide.TabIndex = 0;
            btnHide.Text = "Hide";
            btnHide.UseVisualStyleBackColor = true;
            btnHide.Click += btnHide_Click;
            // 
            // btnSink
            // 
            btnSink.Dock = DockStyle.Left;
            btnSink.Location = new Point(84, 0);
            btnSink.Name = "btnSink";
            btnSink.Size = new Size(84, 37);
            btnSink.TabIndex = 0;
            btnSink.Text = "Sink";
            btnSink.UseVisualStyleBackColor = true;
            btnSink.Click += btnSink_Click;
            // 
            // btnFull
            // 
            btnFull.Dock = DockStyle.Left;
            btnFull.Location = new Point(0, 0);
            btnFull.Name = "btnFull";
            btnFull.Size = new Size(84, 37);
            btnFull.TabIndex = 0;
            btnFull.Text = "Full";
            btnFull.UseVisualStyleBackColor = true;
            btnFull.Visible = false;
            btnFull.Click += btnFull_Click;
            // 
            // Uc_Viewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "Uc_Viewer";
            Size = new Size(674, 543);
            MouseEnter += Uc_Viewer_MouseEnter;
            MouseLeave += Uc_Viewer_MouseLeave;
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.Button btnSink;
        private System.Windows.Forms.Button btnFull;
    }
}
