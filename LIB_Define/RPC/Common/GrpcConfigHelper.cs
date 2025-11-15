using System;
using System.IO;
using System.Windows.Forms;
using LIB_RPC;

namespace LIB_Define.RPC.ALL
{
    /// <summary>
    /// Helper class for showing GrpcConfig dialog for RpcClient and RpcServer
    /// </summary>
    public static class GrpcConfigHelper
    {
        /// <summary>
        /// Show configuration dialog for RpcClient
        /// </summary>
        /// <param name="configPath">Path to config file (e.g., "./Config/client_config.json")</param>
        /// <returns>True if user saved changes, False if cancelled</returns>
        public static bool ShowClientConfigDialog(string configPath)
        {
            return ShowConfigDialog(configPath, isServerMode: false);
        }

        /// <summary>
        /// Show configuration dialog for RpcServer
        /// </summary>
        /// <param name="configPath">Path to config file (e.g., "./Config/server_config.json")</param>
        /// <returns>True if user saved changes, False if cancelled</returns>
        public static bool ShowServerConfigDialog(string configPath)
        {
            return ShowConfigDialog(configPath, isServerMode: true);
        }

        /// <summary>
        /// Show configuration dialog
        /// </summary>
        /// <param name="configPath">Path to config file</param>
        /// <param name="isServerMode">True for server, False for client</param>
        /// <returns>True if user saved changes, False if cancelled</returns>
        public static bool ShowConfigDialog(string configPath, bool isServerMode)
        {
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var form = new GrpcConfigForm(configPath, isServerMode))
                {
                    return form.ShowDialog() == DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing configuration dialog: {ex.Message}",
                    "Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Get configuration with optional dialog
        /// </summary>
        /// <param name="configPath">Path to config file</param>
        /// <param name="isServerMode">True for server, False for client</param>
        /// <param name="showDialogIfNotExists">Show dialog if config file doesn't exist</param>
        /// <returns>GrpcConfig instance</returns>
        public static GrpcConfig GetConfig(string configPath, bool isServerMode, bool showDialogIfNotExists = true)
        {
            GrpcConfig config;

            if (File.Exists(configPath))
            {
                config = GrpcConfig.Load(configPath);
            }
            else
            {
                config = new GrpcConfig();

                if (showDialogIfNotExists)
                {
                    using (var form = new GrpcConfigForm(configPath, isServerMode))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            config = form.Config;
                        }
                    }
                }
            }

            return config;
        }
    }
}
