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
    }
} 