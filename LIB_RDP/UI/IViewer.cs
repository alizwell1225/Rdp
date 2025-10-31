using LIB_RDP.Models;
using LIB_RDP.Core;
using System;
using System.Windows.Forms;

namespace LIB_RDP.UI
{
    public interface IViewer : IDisposable
    {
        int Index { get; }
        void SetStatusText(string text, bool visible = true);
        void SetRdpConnection(RdpConnection conn);
        RdpConnection GetConnection();
        bool IsFullScreen { get; }
        void SaveCurrentState();
        void RestoreOriginalState();
        void SetMax(int w, int h);
        Control AsControl();
        event Action<int> MaximizeRequested;
        event Action<int> RestoreRequested;
    }
}
