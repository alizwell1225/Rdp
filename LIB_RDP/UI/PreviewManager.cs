using LIB_RDP.Models;
using LIB_RDP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LIB_RDP.UI
{
    public class PreviewManager : IPreviewManager
    {
        private readonly Dictionary<int, IViewer> _viewers = new();

        public event Action<int, string, bool> ViewerStatusChanged;

        public void Initialize(int viewerCount, IDictionary<string, RdpConnection> existingConnections)
        {
            // No-op for now; existing connections are handled when viewers register
        }

        public void RegisterViewer(IViewer viewer)
        {
            if (viewer == null) return;
            _viewers[viewer.Index] = viewer;
            viewer.MaximizeRequested += OnViewerMaximizeRequested;
            viewer.RestoreRequested += OnViewerRestoreRequested;
        }

        public void UnregisterViewer(IViewer viewer)
        {
            if (viewer == null) return;
            _viewers.Remove(viewer.Index);
            try
            {
                viewer.MaximizeRequested -= OnViewerMaximizeRequested;
                viewer.RestoreRequested -= OnViewerRestoreRequested;
            }
            catch { }
        }

        private void OnViewerMaximizeRequested(int index)
        {
            ViewerStatusChanged?.Invoke(index, "maximize", true);
        }

        private void OnViewerRestoreRequested(int index)
        {
            ViewerStatusChanged?.Invoke(index, "restore", true);
        }

        public void Connect(int index, string host, string user, string password)
        {
            // For now this is a passthrough: the real connection logic remains in FormRDP
            // Future: move connection management here
        }

        public void Disconnect(int index)
        {
            if (_viewers.TryGetValue(index, out var v))
            {
                var conn = v.GetConnection();
                if (conn != null)
                {
                    conn.Disconnect();
                    conn.Dispose();
                    ViewerStatusChanged?.Invoke(index, "disconnected", true);
                }
                v.SetRdpConnection(null);
            }
        }
    }
}
