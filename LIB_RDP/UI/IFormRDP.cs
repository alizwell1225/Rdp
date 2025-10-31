using System.Windows.Forms;

namespace LIB_RDP.UI;

public interface IFormRDP
{
    void CloseApplication();
    int AutoHideDelay { get; set; }
    bool AutoHideEnabled { get; set; }
    void UpdateAutoHideTimerInterval();
    void ShowTopPanel();
    void HideTopPanel();
    void ControlsAddMenuStrip(MenuStrip menuStrip);
}