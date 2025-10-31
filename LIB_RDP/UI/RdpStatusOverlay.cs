using System;
using System.Drawing;
using System.Windows.Forms;

namespace LIB_RDP.UI
{
    /// <summary>
    /// A lightweight transparent status overlay shown on top of a viewer (left-top).
    /// Background is transparent; text color and font are configurable.
    /// </summary>
    public class RdpStatusOverlay : Label
    {
        public RdpStatusOverlay()
        {
            // Transparent background so underlying control is visible
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            AutoSize = true;
            Font = new Font("微軟正黑體", 9f, FontStyle.Regular);
            Location = new Point(6, 6);
            Visible = false;
            // Make sure mouse events pass through unless explicitly needed
            SetStyle(ControlStyles.Selectable, false);
        }

        public void AttachTo(Control parent)
        {
            if (parent == null) return;
            parent.Controls.Add(this);
            BringToFront();
        }

        public void Detach()
        {
            if (Parent != null)
                Parent.Controls.Remove(this);
        }

        public new string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Visible = !string.IsNullOrEmpty(value);
            }
        }
    }
}
