using System;
using System.Windows.Forms;

namespace LIB_RDP.UI
{
    public class MenuManager
    {
        private readonly IFormRDP _form;
        private readonly MenuStrip _menuStrip;
        private readonly ToolStripMenuItem _viewMenu;
        private readonly ToolStripMenuItem _autoHideMenuItem;
        private readonly ToolStripMenuItem _autoHideDelayMenu;

        public MenuManager(IFormRDP form, MenuStrip menuStrip)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _menuStrip = menuStrip;
            _viewMenu = new ToolStripMenuItem("檢視(&V)");

            // 初始化選單項目
            _autoHideMenuItem = new ToolStripMenuItem("自動隱藏工具列")
            {
                CheckOnClick = true,
                Checked = true // 預設開啟
            };
            _autoHideMenuItem.Click += AutoHideMenuItem_Click;

            var showNowMenuItem = new ToolStripMenuItem("顯示工具列");
            showNowMenuItem.Click += (s, e) => _form.ShowTopPanel();

            var hideNowMenuItem = new ToolStripMenuItem("立即隱藏工具列");
            hideNowMenuItem.Click += (s, e) => _form.HideTopPanel();

            _autoHideDelayMenu = new ToolStripMenuItem("隱藏延遲時間");
            var delayTimes = new[] { 3, 5, 10, 15 };
            foreach (var seconds in delayTimes)
            {
                var item = new ToolStripMenuItem($"{seconds} 秒")
                {
                    Tag = seconds * 1000,
                    Checked = seconds == 5
                };
                item.Click += AutoHideDelay_Click;
                _autoHideDelayMenu.DropDownItems.Add(item);
            }

            _viewMenu.DropDownItems.Add(_autoHideMenuItem);
            _viewMenu.DropDownItems.Add(new ToolStripSeparator());
            _viewMenu.DropDownItems.Add(showNowMenuItem);
            _viewMenu.DropDownItems.Add(hideNowMenuItem);
            _viewMenu.DropDownItems.Add(new ToolStripSeparator());
            _viewMenu.DropDownItems.Add(_autoHideDelayMenu);

            _menuStrip.Items.Add(_viewMenu);

            var exitMenu = new ToolStripMenuItem("關閉程式(&X)");
            exitMenu.Click += ExitMenu_Click;
            _menuStrip.Items.Add(exitMenu);
            _form.ControlsAddMenuStrip(_menuStrip);
            //_form.Controls.Add(_menuStrip);
        }

        public MenuManager(IFormRDP form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _menuStrip = new MenuStrip();
            _viewMenu = new ToolStripMenuItem("檢視(&V)");

            // 初始化選單項目
            _autoHideMenuItem = new ToolStripMenuItem("自動隱藏工具列")
            {
                CheckOnClick = true,
                Checked = true  // 預設開啟
            };
            _autoHideMenuItem.Click += AutoHideMenuItem_Click;

            var showNowMenuItem = new ToolStripMenuItem("顯示工具列");
            showNowMenuItem.Click += (s, e) => _form.ShowTopPanel();

            var hideNowMenuItem = new ToolStripMenuItem("立即隱藏工具列");
            hideNowMenuItem.Click += (s, e) => _form.HideTopPanel();

            _autoHideDelayMenu = new ToolStripMenuItem("隱藏延遲時間");
            var delayTimes = new[] { 3, 5, 10, 15 };
            foreach (var seconds in delayTimes)
            {
                var item = new ToolStripMenuItem($"{seconds} 秒")
                {
                    Tag = seconds * 1000,
                    Checked = seconds == 5
                };
                item.Click += AutoHideDelay_Click;
                _autoHideDelayMenu.DropDownItems.Add(item);
            }

            _viewMenu.DropDownItems.Add(_autoHideMenuItem);
            _viewMenu.DropDownItems.Add(new ToolStripSeparator());
            _viewMenu.DropDownItems.Add(showNowMenuItem);
            _viewMenu.DropDownItems.Add(hideNowMenuItem);
            _viewMenu.DropDownItems.Add(new ToolStripSeparator());
            _viewMenu.DropDownItems.Add(_autoHideDelayMenu);

            _menuStrip.Items.Add(_viewMenu);

            var exitMenu = new ToolStripMenuItem("關閉程式(&X)");
            exitMenu.Click += ExitMenu_Click;
            _menuStrip.Items.Add(exitMenu);

            _form.ControlsAddMenuStrip(_menuStrip);
            //_form.Controls.Add(_menuStrip)
        }

        public MenuStrip MenuStrip => _menuStrip;

        private void AutoHideMenuItem_Click(object sender, EventArgs e)
        {
            _form.AutoHideEnabled = _autoHideMenuItem.Checked;
            if (!_form.AutoHideEnabled)
            {
                _form.ShowTopPanel();
            }
        }

        private void AutoHideDelay_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                foreach (ToolStripMenuItem menuItem in _autoHideDelayMenu.DropDownItems)
                {
                    menuItem.Checked = menuItem == item;
                }

                _form.AutoHideDelay = (int)item.Tag;
                _form.UpdateAutoHideTimerInterval();
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "確定要關閉程式嗎？",
                "關閉確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2  // 預設選擇 "否"
            );

            if (result == DialogResult.Yes)
            {
                _form.CloseApplication();
            }
        }
    }
}
