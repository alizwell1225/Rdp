using System.Collections.Generic;
using LIB_RDP.Models;

namespace LIB_RDP.Interfaces
{
    /// <summary>
    /// RDP配置管理介面
    /// </summary>
    public interface IRdpConfigurationManager
    {
        /// <summary>
        /// 保存連線配置
        /// </summary>
        /// <param name="profile">連線配置</param>
        void SaveConnection(RdpConnectionProfile profile);
        
        /// <summary>
        /// 載入所有連線配置
        /// </summary>
        /// <returns>連線配置清單</returns>
        List<RdpConnectionProfile> LoadAllConnections();
        
        /// <summary>
        /// 載入特定連線配置
        /// </summary>
        /// <param name="profileId">配置ID</param>
        /// <returns>連線配置，若不存在則回傳null</returns>
        RdpConnectionProfile LoadConnection(string profileId);
        
        /// <summary>
        /// 刪除連線配置
        /// </summary>
        /// <param name="profileId">配置ID</param>
        /// <returns>是否成功刪除</returns>
        bool DeleteConnection(string profileId);
        
        /// <summary>
        /// 導出配置到指定檔案
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        void ExportConnections(string filePath);
        
        /// <summary>
        /// 從檔案導入配置
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        /// <returns>成功導入的配置數量</returns>
        int ImportConnections(string filePath);
    }
}
