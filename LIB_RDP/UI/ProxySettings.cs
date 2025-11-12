namespace LIB_RDP.UI;

public class ProxySettings
{
    public int RpcCount { get; private set; } = 12;
    public List<ConnectionInfo> ConnectionsInfo { get; set; } = new List<ConnectionInfo>();

    public ProxySettings(int Count)
    {
        RpcCount= Count;
        // Initialize with default connections
        for (int i = 0; i < RpcCount; i++)
        {
            ConnectionsInfo.Add(new ConnectionInfo());
        }
    }
}