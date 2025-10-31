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
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            InnerViewer = new LIB_RDP.Core.RdpViewer();
            btnHide = new System.Windows.Forms.Button();
            btnSink = new System.Windows.Forms.Button();
            btnFull = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(InnerViewer);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnHide);
            splitContainer1.Panel2.Controls.Add(btnSink);
            splitContainer1.Panel2.Controls.Add(btnFull);
            splitContainer1.Size = new System.Drawing.Size(674, 543);
            splitContainer1.SplitterDistance = 487;
            splitContainer1.TabIndex = 0;
            // 
            // rdpViewer
            // 
            InnerViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            InnerViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            InnerViewer.Location = new System.Drawing.Point(0, 0);
            InnerViewer.Name = "InnerViewer";
            InnerViewer.Size = new System.Drawing.Size(674, 487);
            InnerViewer.TabIndex = 0;
            // 
            // btnHide
            // 
            btnHide.Dock = System.Windows.Forms.DockStyle.Right;
            btnHide.Location = new System.Drawing.Point(590, 0);
            btnHide.Name = "btnHide";
            btnHide.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
            btnHide.Size = new System.Drawing.Size(84, 52);
            btnHide.TabIndex = 0;
            btnHide.Text = "Hide";
            btnHide.UseVisualStyleBackColor = true;
            btnHide.Click += btnHide_Click;
            // 
            // btnSink
            // 
            btnSink.Dock = System.Windows.Forms.DockStyle.Left;
            btnSink.Location = new System.Drawing.Point(84, 0);
            btnSink.Name = "btnSink";
            btnSink.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
            btnSink.Size = new System.Drawing.Size(84, 52);
            btnSink.TabIndex = 0;
            btnSink.Text = "Sink";
            btnSink.UseVisualStyleBackColor = true;
            btnSink.Click += btnSink_Click;
            // 
            // btnFull
            // 
            btnFull.Dock = System.Windows.Forms.DockStyle.Left;
            btnFull.Location = new System.Drawing.Point(0, 0);
            btnFull.Name = "btnFull";
            btnFull.Size = new System.Drawing.Size(84, 52);
            btnFull.TabIndex = 0;
            btnFull.Text = "Full";
            btnFull.UseVisualStyleBackColor = true;
            btnFull.Click += btnFull_Click;
            // 
            // Uc_Viewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "Uc_Viewer";
            Size = new System.Drawing.Size(674, 543);
            MouseEnter += Uc_Viewer_MouseEnter;
            MouseLeave += Uc_Viewer_MouseLeave;
            splitContainer1.Panel1.ResumeLayout(false);
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
