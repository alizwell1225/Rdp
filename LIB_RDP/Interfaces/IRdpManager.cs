using System.Collections.Generic;

namespace LIB_RDP.Interfaces
{
    public interface IRdpManager
    {
        IRdpConnection CreateConnection();
        void RemoveConnection(string connectionId);
        IEnumerable<IRdpConnection> ActiveConnections { get; }
    }
} 