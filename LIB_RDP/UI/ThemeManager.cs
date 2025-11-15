using System;
using System.Drawing;
using System.Windows.Forms;

namespace LIB_RDP.UI
{
    /// <summary>
    /// 主題管理器 - 統一管理應用程式的配色與樣式
    /// </summary>
    public static class ThemeManager
    {
        private static ThemeMode _currentTheme = ThemeMode.Dark;

        public enum ThemeMode
        {
            Light,
            Dark,
            Auto
        }

        public static ThemeMode CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler ThemeChanged;

        // 主色調
        public static Color PrimaryColor => Color.FromArgb(0, 120, 212);          // Windows 藍
        public static Color PrimaryHoverColor => Color.FromArgb(16, 110, 190);    // 懸停時稍深
        public static Color PrimaryPressedColor => Color.FromArgb(0, 90, 158);    // 按下時更深

        // 狀態色
        public static Color SuccessColor => Color.FromArgb(16, 124, 16);          // 成功 - 綠
        public static Color WarningColor => Color.FromArgb(255, 185, 0);          // 警告 - 橘
        public static Color ErrorColor => Color.FromArgb(232, 17, 35);            // 錯誤 - 紅
        public static Color InfoColor => Color.FromArgb(0, 153, 188);             // 資訊 - 青

        // 背景色系
        public static Color BackgroundColor => IsDarkMode 
            ? Color.FromArgb(32, 32, 32)    // 深色背景
            : Color.FromArgb(243, 243, 243); // 淺色背景

        public static Color SurfaceColor => IsDarkMode
            ? Color.FromArgb(45, 45, 48)    // 深色表面
            : Color.White;                   // 淺色表面

        public static Color CardColor => IsDarkMode
            ? Color.FromArgb(37, 37, 38)    // 深色卡片
            : Color.FromArgb(250, 250, 250); // 淺色卡片

        // 文字色
        public static Color ForegroundColor => IsDarkMode
            ? Color.FromArgb(241, 241, 241)  // 深色模式文字
            : Color.FromArgb(23, 23, 23);    // 淺色模式文字

        public static Color SecondaryForegroundColor => IsDarkMode
            ? Color.FromArgb(200, 200, 200)  // 深色次要文字
            : Color.FromArgb(96, 96, 96);    // 淺色次要文字

        public static Color DisabledForegroundColor => IsDarkMode
            ? Color.FromArgb(128, 128, 128)  // 深色禁用文字
            : Color.FromArgb(161, 161, 161); // 淺色禁用文字

        // 邊框色
        public static Color BorderColor => IsDarkMode
            ? Color.FromArgb(63, 63, 70)     // 深色邊框
            : Color.FromArgb(218, 218, 218); // 淺色邊框

        public static Color DividerColor => IsDarkMode
            ? Color.FromArgb(51, 51, 55)     // 深色分隔線
            : Color.FromArgb(230, 230, 230); // 淺色分隔線

        // 輔助屬性
        public static bool IsDarkMode => _currentTheme == ThemeMode.Dark || (_currentTheme == ThemeMode.Auto && IsSystemDarkMode());

        /// <summary>
        /// 檢測系統是否使用深色模式 (Windows 10+)
        /// </summary>
        private static bool IsSystemDarkMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    return value is int intValue && intValue == 0;
                }
            }
            catch
            {
                return false; // 預設淺色
            }
        }

        /// <summary>
        /// 應用主題到 Form
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            form.BackColor = BackgroundColor;
            form.ForeColor = ForegroundColor;

            ApplyThemeToControls(form.Controls);
        }

        /// <summary>
        /// 遞迴應用主題到所有控制項
        /// </summary>
        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Panel panel)
                {
                    panel.BackColor = SurfaceColor;
                    panel.ForeColor = ForegroundColor;
                }
                else if (control is DataGridView dgv)
                {
                    ApplyThemeToDataGridView(dgv);
                }
                else if (control is Button button)
                {
                    ApplyThemeToButton(button);
                }
                else if (control is Label label)
                {
                    label.ForeColor = ForegroundColor;
                }
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = SurfaceColor;
                    textBox.ForeColor = ForegroundColor;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }

                // 遞迴處理子控制項
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        /// <summary>
        /// 應用主題到 DataGridView
        /// </summary>
        public static void ApplyThemeToDataGridView(DataGridView dgv)
        {
            // 整體樣式
            dgv.BackgroundColor = SurfaceColor;
            dgv.GridColor = BorderColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;

            // 預設儲存格樣式
            dgv.DefaultCellStyle.BackColor = SurfaceColor;
            dgv.DefaultCellStyle.ForeColor = ForegroundColor;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(5);

            // 交替列顏色
            dgv.AlternatingRowsDefaultCellStyle.BackColor = IsDarkMode
                ? Color.FromArgb(40, 40, 42)
                : Color.FromArgb(248, 248, 248);

            // 欄位標頭樣式
            dgv.ColumnHeadersDefaultCellStyle.BackColor = CardColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ForegroundColor;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(dgv.Font.FontFamily, dgv.Font.Size, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // 列標頭樣式
            dgv.RowHeadersDefaultCellStyle.BackColor = CardColor;
            dgv.RowHeadersDefaultCellStyle.ForeColor = ForegroundColor;
            dgv.RowHeadersVisible = false; // 通常隱藏列標頭

            // 選取模式
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
        }

        /// <summary>
        /// 應用主題到 Button
        /// </summary>
        public static void ApplyThemeToButton(Button button, bool isPrimary = false)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(0); ;// new Padding(12, 6, 12, 6);

            if (isPrimary)
            {
                button.BackColor = PrimaryColor;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = PrimaryColor;
                button.FlatAppearance.MouseOverBackColor = PrimaryHoverColor;
                button.FlatAppearance.MouseDownBackColor = PrimaryPressedColor;
            }
            else
            {
                button.BackColor = SurfaceColor;
                button.ForeColor = ForegroundColor;
                button.FlatAppearance.BorderColor = BorderColor;
                button.FlatAppearance.MouseOverBackColor = IsDarkMode
                    ? Color.FromArgb(55, 55, 58)
                    : Color.FromArgb(240, 240, 240);
            }
        }

        /// <summary>
        /// 獲取連線狀態顏色
        /// </summary>
        public static Color GetConnectionStateColor(string state)
        {
            return state switch
            {
                "已連線" or "Connected" => SuccessColor,
                "連線中" or "Connecting" => WarningColor,
                "未連線" or "Disconnected" => SecondaryForegroundColor,
                "錯誤" or "Error" => ErrorColor,
                _ => SecondaryForegroundColor
            };
        }

        /// <summary>
        /// 繪製圓角矩形
        /// </summary>
        public static System.Drawing.Drawing2D.GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}