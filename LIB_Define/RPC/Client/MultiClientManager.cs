using LIB_Define.RDP;
using LIB_Define.RPC.Client_org;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grpc.Core;

namespace LIB_Define.RPC.Client
{
    /// <summary>
    /// Helper class for managing multiple RpcClient instances
    /// </summary>
    public static class MultiClientManager
    {
        public static event Action<int,bool> EvConfigurationSaved;
        private static string _configPath;
        /// <summary>
        /// Show multi-client configuration dialog
        /// </summary>
        /// <param name="configPath">Path to multi-client config file</param>
        /// <returns>True if user saved changes, False if cancelled</returns>
        public static bool ShowMultiClientConfigDialog(string configPath = "./Config/multi_client_config.json")
        {
            try
            {
                
                // Normalize path
                configPath = NormalizePath(configPath);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var form = new MultiClientConfigForm(configPath))
                {
                    var rtn = form.ShowDialog() == DialogResult.OK;
                    if (rtn)
                        form.EvConfigurationSaved += EvConfigurationSaved;
                    return rtn;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing multi-client configuration dialog: {ex.Message}",
                    "Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Load multi-client configuration
        /// </summary>
        /// <param name="configPath">Path to config file</param>
        /// <returns>MultiClientConfig instance</returns>
        public static MultiClientConfig LoadMultiClientConfig(string configPath = "./Config/multi_client_config.json")
        {
            configPath = NormalizePath(configPath);
            _configPath = configPath;
            if (File.Exists(configPath))
            {
                return MultiClientConfig.Load(configPath);
            }
            return new MultiClientConfig();
        }

        /// <summary>
        /// Create multiple RpcClient instances from configuration
        /// </summary>
        /// <param name="config">Multi-client configuration</param>
        /// <returns>List of RpcClient instances</returns>
        public static List<RpcClient> CreateClients(MultiClientConfig config)
        {
            var clients = new List<RpcClient>();

            foreach (var clientRef in config.Clients)
            {
                if (clientRef.Enabled)
                {
                    try
                    {
                        // Load or create the config
                        var grpcConfig = clientRef.LoadConfig();
                        
                        // Ensure individual config file exists
                        clientRef.SaveConfig(grpcConfig);

                        // Create RpcClient instance
                        var client = new RpcClient(clientRef.ConfigPath, clientRef.Index);
                        clients.Add(client);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating client {clientRef.Index}: {ex.Message}",
                            "Client Creation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }

            return clients;
        }

        /// <summary>
        /// Create multiple RpcClient instances and start them all
        /// </summary>
        /// <param name="configPath">Path to multi-client config file</param>
        /// <returns>List of connected RpcClient instances</returns>
        public static async Task<List<RpcClient>> CreateAndStartClientsAsync(string configPath = "./Config/multi_client_config.json")
        {
            configPath = NormalizePath(configPath);
            var config = LoadMultiClientConfig(configPath);
            var clients = CreateClients(config);

            // Start all clients
            var tasks = new List<Task>();
            foreach (var client in clients)
            {
                tasks.Add(client.StartConnect());
            }

            // Wait for all to connect
            await Task.WhenAll(tasks);

            return clients;
        }

        /// <summary>
        /// Stop all clients
        /// </summary>
        /// <param name="clients">List of RpcClient instances to stop</param>
        public static async Task StopAllClientsAsync(List<RpcClient> clients)
        {
            if (clients == null || clients.Count == 0)
                return;

            var tasks = new List<Task>();
            foreach (var client in clients)
            {
                // Add disconnection logic here if needed
                // For now, we'll just wait a moment
                tasks.Add(Task.Delay(100));
            }

            await Task.WhenAll(tasks);
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
