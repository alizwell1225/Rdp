using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using LIB_Log;
using LIB_RDP.Models;

namespace LIB_RDP.Core
{
    /// <summary>
    /// RDP設定管理器，負責保存和載入連線設定
    /// </summary>
    public class RdpConfigurationManager
    {
        private static readonly Lazy<RdpConfigurationManager> _instance = new Lazy<RdpConfigurationManager>(() => new RdpConfigurationManager());
        
        public static RdpConfigurationManager Instance => _instance.Value;
        
        //private readonly RdpLogger _logger;
        private Logger _logger;
        private readonly string _configDirectory;
        private readonly string _connectionsFilePath;
        
        private RdpConfigurationManager()
        {
            //_logger = RdpLogger.Instance;
            //_logger = new Logger(Path.Combine(AppContext.BaseDirectory, "Log"), "RDP"); 
            _configDirectory = Path.Combine(AppContext.BaseDirectory, "Config");
            _connectionsFilePath = Path.Combine(_configDirectory, "Rdp_Connections.xml");
            Directory.CreateDirectory(_configDirectory);
        }
        
        /// <summary>
        /// 保存連線設定
        /// </summary>
        public void SaveConnection(RdpConnectionProfile profile)
        {
            try
            {
                var profiles = LoadAllConnections();
                
                // 更新或添加設定
                var existingIndex = profiles.FindIndex(p => p.Id == profile.Id);
                if (existingIndex >= 0)
                    profiles[existingIndex] = profile;
                else
                    profiles.Add(profile);
                
                var serializer = new XmlSerializer(typeof(List<RdpConnectionProfile>));
                using (var writer = new FileStream(_connectionsFilePath, FileMode.Create))
                {
                    serializer.Serialize(writer, profiles);
                }
                
                _logger?.Info($"已保存連線設定: {profile.Name}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"保存連線設定失敗: {profile.Name}", ex);
                throw new RdpException($"保存連線設定失敗: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 載入所有連線設定
        /// </summary>
        public List<RdpConnectionProfile> LoadAllConnections()
        {
            try
            {
                if (!File.Exists(_connectionsFilePath))
                    return new List<RdpConnectionProfile>();

                var serializer = new XmlSerializer(typeof(List<RdpConnectionProfile>));

                using (var stream = new FileStream(_connectionsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _logger?.Info($"已載入連線設定");
                    return (List<RdpConnectionProfile>)serializer.Deserialize(stream);
                }



                //if (!File.Exists(_connectionsFilePath))
                //    return new List<RdpConnectionProfile>();
                
                //var serializer = new XmlSerializer(typeof(List<RdpConnectionProfile>));

                //using (var reader = new FileStream(_connectionsFilePath, FileMode.Open))
                //{
                //    var profiles = (List<RdpConnectionProfile>)serializer.Deserialize(reader) ?? 
                //                   new List<RdpConnectionProfile>();
                    
                //    _logger?.Info($"已載入 {profiles.Count} 個連線設定");
                //    return profiles;
                //}
            }
            catch (Exception ex)
            {
                _logger?.Error("載入連線設定失敗", ex);
                return new List<RdpConnectionProfile>();
            }
        }
        
        /// <summary>
        /// 載入特定連線設定
        /// </summary>
        public RdpConnectionProfile LoadConnection(string profileId)
        {
            var profiles = LoadAllConnections();
            return profiles.Find(p => p.Id == profileId);
        }
        
        /// <summary>
        /// 刪除連線設定
        /// </summary>
        public bool DeleteConnection(string profileId)
        {
            try
            {
                var profiles = LoadAllConnections();
                var profile = profiles.Find(p => p.Id == profileId);
                if (profile == null)
                    return false;
                
                profiles.RemoveAll(p => p.Id == profileId);
                
                string json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(_connectionsFilePath, json);
                _logger?.Info($"已刪除連線設定: {profile.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"刪除連線設定失敗: {profileId}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 導出設定到指定文件
        /// </summary>
        public void ExportConnections(string filePath)
        {
            try
            {
                var profiles = LoadAllConnections();
                string json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(filePath, json);
                _logger?.Info($"已導出 {profiles.Count} 個連線設定到: {filePath}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"導出連線設定失敗: {filePath}", ex);
                throw new RdpException($"導出連線設定失敗: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 從文件導入設定
        /// </summary>
        public int ImportConnections(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"文件不存在: {filePath}");
                
                string json = File.ReadAllText(filePath);
                var importedProfiles = JsonSerializer.Deserialize<List<RdpConnectionProfile>>(json);
                
                if (importedProfiles == null || importedProfiles.Count == 0)
                    return 0;
                
                var existingProfiles = LoadAllConnections();
                int importedCount = 0;
                
                foreach (var profile in importedProfiles)
                {
                    // 檢查是否已存在相同ID的設定
                    if (existingProfiles.Exists(p => p.Id == profile.Id))
                    {
                        // 生成新ID避免衝突
                        profile.Id = Guid.NewGuid().ToString();
                        profile.Name += " (導入)";
                    }
                    
                    existingProfiles.Add(profile);
                    importedCount++;
                }
                
                string outputJson = JsonSerializer.Serialize(existingProfiles, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(_connectionsFilePath, outputJson);
                _logger?.Info($"已導入 {importedCount} 個連線設定從: {filePath}");
                
                return importedCount;
            }
            catch (Exception ex)
            {
                _logger?.Error($"導入連線設定失敗: {filePath}", ex);
                throw new RdpException($"導入連線設定失敗: {ex.Message}", ex);
            }
        }

        public void SetLog(Logger logger)
        {
            _logger= logger;
        }

        public RdpConnectionProfile FindConnIndex(int profileIdx)
        {
            var profiles = LoadAllConnections();
            return profiles.Find(p => p.Index == profileIdx);
        }
    }
}