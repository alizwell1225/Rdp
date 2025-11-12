using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LIB_RPC;

namespace LIB_Define
{
    /// <summary>
    /// Multi-client configuration manager for running multiple RpcClients on one machine
    /// </summary>
    public class MultiClientConfig
    {
        public int ClientCount { get; set; } = 12;
        public List<ClientInstanceConfig> Clients { get; set; } = new List<ClientInstanceConfig>();

        public MultiClientConfig()
        {
            // Initialize with default configs
            for (int i = 0; i < ClientCount; i++)
            {
                Clients.Add(new ClientInstanceConfig
                {
                    Index = i,
                    Enabled = true,
                    ConfigPath = $"./Config/client_{i}_config.json"
                });
            }
        }

        public static MultiClientConfig Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return new MultiClientConfig();
            
            try
            {
                var json = File.ReadAllText(path);
                return System.Text.Json.JsonSerializer.Deserialize<MultiClientConfig>(json) ?? new MultiClientConfig();
            }
            catch
            {
                return new MultiClientConfig();
            }
        }

        public void Save(string path)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                var folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save multi-client config: {ex.Message}");
            }
        }

        public void EnsureClientCount(int count)
        {
            ClientCount = count;
            
            // Add missing clients
            while (Clients.Count < count)
            {
                Clients.Add(new ClientInstanceConfig
                {
                    Index = Clients.Count,
                    Enabled = true,
                    ConfigPath = $"./Config/client_{Clients.Count}_config.json"
                });
            }
            
            // Remove excess clients
            while (Clients.Count > count)
            {
                Clients.RemoveAt(Clients.Count - 1);
            }
            
            // Update indices
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Index = i;
            }
        }
    }

    /// <summary>
    /// Configuration for a single client instance
    /// </summary>
    public class ClientInstanceConfig
    {
        public int Index { get; set; }
        public bool Enabled { get; set; }
        public string ConfigPath { get; set; } = string.Empty;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 50051;
        public string LogFilePath { get; set; } = string.Empty;
        public string StorageRoot { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public ClientInstanceConfig()
        {
            LogFilePath = $"./Logs/client_{Index}_grpc.log";
            StorageRoot = $"./Storage/client_{Index}";
            DisplayName = $"Client {Index}";
        }

        public GrpcConfig ToGrpcConfig()
        {
            var config = new GrpcConfig
            {
                Host = this.Host,
                Port = this.Port,
                LogFilePath = string.IsNullOrEmpty(this.LogFilePath) 
                    ? $"./Logs/client_{Index}_grpc.log" 
                    : this.LogFilePath,
                StorageRoot = string.IsNullOrEmpty(this.StorageRoot)
                    ? $"./Storage/client_{Index}"
                    : this.StorageRoot
            };
            config.ClientDownloadPath = config.StorageRoot;
            return config;
        }

        public void FromGrpcConfig(GrpcConfig config)
        {
            this.Host = config.Host;
            this.Port = config.Port;
            this.LogFilePath = config.LogFilePath;
            this.StorageRoot = config.GetClientDownloadPath();
        }

        public void SaveIndividualConfig()
        {
            if (string.IsNullOrEmpty(ConfigPath))
                return;

            var config = ToGrpcConfig();
            config.Save(ConfigPath);
        }

        public void LoadIndividualConfig()
        {
            if (string.IsNullOrEmpty(ConfigPath) || !File.Exists(ConfigPath))
                return;

            try
            {
                var config = GrpcConfig.Load(ConfigPath);
                FromGrpcConfig(config);
            }
            catch
            {
                // Use default values if load fails
            }
        }
    }
}
