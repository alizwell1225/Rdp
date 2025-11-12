using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LIB_RDP.Core;
using LIB_RDP.Models;
using Timer = System.Windows.Forms.Timer;

namespace LIB_RDP.UI;

public partial class Uc_Viewer : UserControl, IViewer
{
    public event Action<int> ActMaxViewer;

    public event Action<int> ActSinkViewer;

    // 設定滑鼠進入/離開事件來控制 panel 顯示
    private readonly Timer hoverTimer = new();

    // Polling timer to detect mouse over for controls that don't raise mouse events (ActiveX/interop)
    private readonly Timer pollTimer = new();

    // index assigned by the host form when initialized

    public Uc_Viewer()
    {
        InitializeComponent();
        // 預設隱藏右側 panel（panel2）—由 hover 顯示
        try
        {
            // 把 Panel2 摺疊，讓 Panel1 真正填滿整個 SplitContainer
            splitContainer1.Panel2Collapsed = true;
        }
        catch
        {
        }

        hoverTimer.Interval = 150; // 離開後延遲自動隱藏（ms）
        hoverTimer.Tick += TimerTick;
        InnerViewer.MouseEnter += Uc_Viewer_MouseEnter;
        InnerViewer.MouseLeave += Uc_Viewer_MouseLeave;

        // Make sure the control can receive key events when it has focus
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        KeyDown += Uc_Viewer_KeyDown;
        KeyUp += Uc_Viewer_KeyUp;

        // Poll frequently to detect mouse entering over native/ActiveX child controls
        pollTimer.Interval = 150;
        pollTimer.Tick += PollTimerTick;
        pollTimer.Start();

        // Wire mouse events recursively for existing children and listen for future additions
        WireMouseEvents(this);
        if (splitContainer1?.Panel2 != null) WireMouseEvents(splitContainer1.Panel2);
        WireMouseEvents(InnerViewer);
    }

    ~Uc_Viewer()
    {
        try
        {
            InnerViewer.GetRdpConnection()?.Disconnect();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public RdpViewer InnerViewer { get; private set; }

    public int Index { get; private set; } = -1;

    public event Action<int> MaximizeRequested;
    public event Action<int> RestoreRequested;

    public void SetRdpConnection(RdpConnection conn)
    {
        InnerViewer.SetConnection(conn);
    }

    // expose the current connection from the inner RdpViewer
    public RdpConnection GetConnection()
    {
        return InnerViewer.GetConnection();
    }

    // convenience: update the status overlay text (Uc_Viewer encapsulates overlay on inner viewer)
    public void SetStatusText(string text, bool visible)
    {
        try
        {
            foreach (Control ctrl in InnerViewer.Controls)
                if (ctrl is RdpStatusOverlay overlay)
                {
                    overlay.Text = visible ? text : string.Empty;
                    overlay.Visible = visible && !string.IsNullOrEmpty(text);
                    break;
                }
        }
        catch
        {
        }
    }

    // pass-through helpers so host form can control fullscreen/size behaviour via the Uc_Viewer
    public bool IsFullScreen => InnerViewer.IsFullScreen;

    public void SaveCurrentState()
    {
        InnerViewer.SaveCurrentState();
    }

    public void SetMax(int w, int h)
    {
        InnerViewer.SetMax(w, h);
    }

    public void RestoreOriginalState()
    {
        InnerViewer.RestoreOriginalState();
    }

    // IViewer helper for PreviewManager
    public Control AsControl()
    {
        return this;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        hoverTimer.Stop();
        // 如果滑鼠不在任何相關控制上，隱藏 panel2
        if (!IsMouseOverControl(this) && !IsMouseOverControl(InnerViewer) &&
            !IsMouseOverControl(splitContainer1.Panel2))
            try
            {
                splitContainer1.Panel2Collapsed = true;
            }
            catch
            {
            }
    }

    // Local helper method defined as lambda-like method
    private bool IsMouseOverControl(Control c)
    {
        try
        {
            if (c == null || c.IsDisposed) return false;
            var pos = c.PointToClient(Cursor.Position);
            return c.ClientRectangle.Contains(pos);
        }
        catch
        {
            return false;
        }
    }

    // Helper local to show panel immediately
    private void ShowPanel()
    {
        hoverTimer.Stop();
        try
        {
            splitContainer1.Panel2Collapsed = false;
        }
        catch
        {
        }
    }

    private void PollTimerTick(object sender, EventArgs e)
    {
        try
        {
            // Only show when Ctrl is held down and mouse over a relevant area.
            if (IsCtrlPressed() && (IsMouseOverControl(this) || IsMouseOverControl(InnerViewer) ||
                                    (splitContainer1?.Panel2 != null && IsMouseOverControl(splitContainer1.Panel2))))
                ShowPanel();
        }
        catch
        {
        }
    }

    // Recursively attach mouse handlers to a control and its children so that
    // managed child controls will trigger the show/hide behaviour. Also
    // listens for ControlAdded to wire dynamically created children.
    private void WireMouseEvents(Control c)
    {
        if (c == null) return;

        try
        {
            // Avoid wiring the same event multiple times by removing then adding.
            c.MouseEnter -= Uc_Viewer_MouseEnter;
            c.MouseLeave -= Uc_Viewer_MouseLeave;

            c.MouseEnter += Uc_Viewer_MouseEnter;
            c.MouseLeave += Uc_Viewer_MouseLeave;

            c.ControlAdded -= C_ControlAdded;
            c.ControlAdded += C_ControlAdded;

            // Recurse into existing children
            foreach (Control child in c.Controls) WireMouseEvents(child);
        }
        catch
        {
        }
    }

    private void C_ControlAdded(object sender, ControlEventArgs e)
    {
        // Wire mouse events for newly added child controls
        WireMouseEvents(e.Control);
    }

    public void Init(int width, int height, int index, int cols, Dictionary<string, RdpConnection> existingConnections)
    {
        // store index so this control can ask the host form to maximize/restore
        Index = index;

        // Make the Uc_Viewer control itself sized/positioned so the host can arrange
        // multiple Uc_Viewer instances easily. The inner RdpViewer will be docked
        // to fill the user control so it automatically fits when the Uc_Viewer
        // is resized by the layout code in FormRDP.
        Size = new Size(Math.Max(1, width - 10), Math.Max(1, height - 10));
        Location = new Point(index % cols * width + 5, index / cols * height + 5);

        // Style the inner viewer and dock it so content fills the Uc_Viewer.
        InnerViewer.BorderStyle = BorderStyle.FixedSingle;
        InnerViewer.Dock = DockStyle.Fill;
        InnerViewer.BackColor = ThemeManager.CardColor;

        var overlay = new RdpStatusOverlay
        {
            ForeColor = ThemeManager.ForegroundColor,
            Font = new Font("微軟正黑體", 9f, FontStyle.Regular)
        };
        InnerViewer.AttachStatusOverlay(overlay);

        SetRdpConnection(index, existingConnections);

        // Apply theme colors to this control and the action buttons (if designer created them)
        try
        {
            BackColor = ThemeManager.CardColor;
            splitContainer1.Panel2.BackColor = ThemeManager.CardColor;
            // style buttons if they exist
            btnFull.BackColor = ThemeManager.CardColor;
            btnFull.ForeColor = ThemeManager.ForegroundColor;
            btnFull.FlatStyle = FlatStyle.Flat;
            btnFull.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            btnFull.FlatAppearance.MouseOverBackColor = ThemeManager.PrimaryHoverColor;
            btnFull.FlatAppearance.MouseDownBackColor = ThemeManager.PrimaryPressedColor;

            btnSink.BackColor = ThemeManager.CardColor;
            btnSink.ForeColor = ThemeManager.ForegroundColor;
            btnSink.FlatStyle = FlatStyle.Flat;
            btnSink.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            btnSink.FlatAppearance.MouseOverBackColor = ThemeManager.PrimaryHoverColor;
            btnSink.FlatAppearance.MouseDownBackColor = ThemeManager.PrimaryPressedColor;

            btnHide.BackColor = ThemeManager.CardColor;
            btnHide.ForeColor = ThemeManager.ForegroundColor;
            btnHide.FlatStyle = FlatStyle.Flat;
            btnHide.FlatAppearance.BorderColor = ThemeManager.BorderColor;
            btnHide.FlatAppearance.MouseOverBackColor = ThemeManager.PrimaryHoverColor;
            btnHide.FlatAppearance.MouseDownBackColor = ThemeManager.PrimaryPressedColor;
        }
        catch
        {
        }
    }

    public void SetRdpConnection(int index, Dictionary<string, RdpConnection> existingConnections)
    {
        // support both legacy keys ("viewer_{index}") and older "rdpViewer_{index}" keys
        var keyViewer = $"viewer_{index}";
        var keyLegacy = $"rdpViewer_{index}";
        if (existingConnections.TryGetValue(keyViewer, out var conn) ||
            existingConnections.TryGetValue(keyLegacy, out conn))
        {
            conn.Configure(new RdpConfig
                { ScreenWidth = InnerViewer.Width, ScreenHeight = InnerViewer.Height, ColorDepth = 32 });
            InnerViewer.SetConnection(conn);
            InnerViewer.SetScreenSize();
        }
        else
        {
            var label = new Label
            {
                Text = $"預覽 #{index + 1}\n未連線",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = ThemeManager.SecondaryForegroundColor,
                BackColor = ThemeManager.CardColor,
                Font = new Font("微軟正黑體", 10, FontStyle.Regular)
            };
            InnerViewer.Controls.Add(label);
        }
    }


    private void btnFull_Click(object sender, EventArgs e)
    {
        try
        {
            // Ask the host form to maximize this viewer into the preview panel
            //var frm = this.FindForm() as LIB_RDP.UI.FormRDP;
            //frm?.MaximizeViewer(viewerIndex);
            ActMaxViewer?.Invoke(Index);
            splitContainer1.Panel2Collapsed = true;
            //MaximizeRequested?.Invoke(viewerIndex);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"無法放大視窗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnSink_Click(object sender, EventArgs e)
    {
        try
        {
            // Ask the host form to restore this viewer to original size
            //var frm = this.FindForm() as LIB_RDP.UI.FormRDP;
            //frm?.RestoreViewer(viewerIndex);
            ActSinkViewer?.Invoke(Index);
            splitContainer1.Panel2Collapsed = true;
            this.GetConnection()?.Disconnect();
            
            //RestoreRequested?.Invoke(viewerIndex);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"無法還原視窗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnHide_Click(object sender, EventArgs e)
    {
        splitContainer1.Panel2Collapsed = true;
    }

    private void Uc_Viewer_MouseEnter(object sender, EventArgs e)
    {
        // Only show when Ctrl is pressed
        if (IsCtrlPressed()) ShowPanel();
    }

    private void Uc_Viewer_MouseLeave(object sender, EventArgs e)
    {
        hoverTimer.Start();
    }

    private void Uc_Viewer_KeyDown(object sender, KeyEventArgs e)
    {
        // When Ctrl is pressed while mouse is over relevant area, show panel
        if (e.Control && (IsMouseOverControl(this) || IsMouseOverControl(InnerViewer) ||
                          (splitContainer1?.Panel2 != null && IsMouseOverControl(splitContainer1.Panel2)))) ShowPanel();
    }

    private void Uc_Viewer_KeyUp(object sender, KeyEventArgs e)
    {
        // If Ctrl released, hide panel immediately
        if (!e.Control)
            try
            {
                splitContainer1.Panel2Collapsed = true;
            }
            catch
            {
            }
    }

    private bool IsCtrlPressed()
    {
        try
        {
            return (ModifierKeys & Keys.Control) == Keys.Control;
        }
        catch
        {
            return false;
        }
    }
}