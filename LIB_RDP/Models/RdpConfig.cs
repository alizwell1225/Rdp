namespace LIB_RDP.Models
{
    [Serializable]
    public class RdpConfig
    {
        public int ScreenWidth { get; set; } = 1920;
        public int ScreenHeight { get; set; } = 1080;
        public int ColorDepth { get; set; } = 32;
        public bool EnableCredSspSupport { get; set; } = true;
        public string Domain { get; set; } = string.Empty;
        public bool EnableCompression { get; set; } = true;
        public bool EnableBitmapPersistence { get; set; } = true;

        /// <summary>
        /// true=>自動調整對應大小
        /// false=>SmartSizing（智慧縮放）會讓遠端畫面被縮放以套入視窗
        /// </summary>
        public bool SmartSize { get; set; } = true;
    }
} 