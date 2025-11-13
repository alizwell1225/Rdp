using System.Drawing;
using LIB_RDP.Models;

namespace LIB_RDP.Interfaces
{
    public interface IRdpConnection
    {
        bool Connect(string hostName, string userName, string password,bool usePingFlag=false);
        // 新增非同步 Connect，回傳是否最終確認連線成功（含驗證畫面）
        System.Threading.Tasks.Task<bool> ConnectAsync(string hostName, string userName, string password, int timeoutSeconds = 30, bool usePingFlag = false);
        void Disconnect();
        void Configure(RdpConfig config);
        bool IsConnected { get; }
        RdpState State { get; }
        Bitmap GetScreenshot();
    }
} 