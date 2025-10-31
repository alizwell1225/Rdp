using LIB_RDP.Models;
using LIB_RDP.Core;
using System;
using System.Collections.Generic;

namespace LIB_RDP.UI
{
    public interface IPreviewManager
    {
        void Initialize(int viewerCount, IDictionary<string, RdpConnection> existingConnections);
        void RegisterViewer(IViewer viewer);
        void UnregisterViewer(IViewer viewer);
        void Connect(int index, string host, string user, string password);
        void Disconnect(int index);
        event Action<int, string, bool> ViewerStatusChanged; // index, text, visible
    }
}
