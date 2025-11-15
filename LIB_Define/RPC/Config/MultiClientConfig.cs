using LIB_RPC;

namespace LIB_Define.RPC.Client_org
{
    /// <summary>
    /// Multi-client configuration manager for running multiple RpcClients on one machine
    /// Only stores the number of clients and their config file paths
    /// </summary>
    public class MultiClientConfig
    {
        public int ClientCount { get; set; } = 12;
        public List<ClientInstanceReference> Clients { get; set; } = new List<ClientInstanceReference>();

        public MultiClientConfig()
        {
            // Initialize with default configs
            for (int i = 0; i < ClientCount; i++)
            {
                Clients.Add(new ClientInstanceReference
                {
                    Index = i,
                    Enabled = true,
                    ConfigPath = NormalizePath($"./Config/client_{i}_config.json")
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
                var config = System.Text.Json.JsonSerializer.Deserialize<MultiClientConfig>(json) ?? new MultiClientConfig();
                
                // Normalize all paths after loading
                foreach (var client in config.Clients)
                {
                    client.ConfigPath = NormalizePath(client.ConfigPath);
                }
                
                return config;
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
                // Normalize all paths before saving
                foreach (var client in Clients)
                {
                    client.ConfigPath = NormalizePath(client.ConfigPath);
                }
                
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
                Clients.Add(new ClientInstanceReference
                {
                    Index = Clients.Count,
                    Enabled = true,
                    ConfigPath = NormalizePath($"./Config/client_{Clients.Count}_config.json")
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

        /// <summary>
        /// Normalize path to use forward slashes
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            return path.Replace('\\', '/');
        }
    }

    /// <summary>
    /// Reference to a single client instance - only stores index, enabled state, and config path
    /// Actual client settings are stored in the individual config file
    /// </summary>
    public class ClientInstanceReference
    {
        public int Index { get; set; }
        public bool Enabled { get; set; }
        public string ConfigPath { get; set; } = string.Empty;

        /// <summary>
        /// Load the full GrpcConfig from the config file
        /// </summary>
        public GrpcConfig LoadConfig()
        {
            if (string.IsNullOrEmpty(ConfigPath) || !File.Exists(ConfigPath))
            {
                // Return default config
                return CreateDefaultConfig();
            }

            try
            {
                return GrpcConfig.Load(ConfigPath);
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Save the GrpcConfig to the config file
        /// </summary>
        public void SaveConfig(GrpcConfig config)
        {
            if (string.IsNullOrEmpty(ConfigPath))
                return;

            config.Save(ConfigPath);
        }

        /// <summary>
        /// Create a default config for this client
        /// </summary>
        private GrpcConfig CreateDefaultConfig()
        {
            var config = new GrpcConfig
            {
                Host = "localhost",
                Port = 50051 + Index,
                LogFilePath = NormalizePath($"./Logs/client_{Index}_grpc.log"),
                StorageRoot = NormalizePath($"./Storage/client_{Index}")
            };
            config.ClientDownloadPath = config.StorageRoot;
            return config;
        }

        /// <summary>
        /// Normalize path to use forward slashes
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            return path.Replace('\\', '/');
        }
    }
}
