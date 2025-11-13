using System.Text.Json.Serialization;

namespace LIB_Define
{
    [Serializable]
    public class FlowChartOBJ
    {
        public int ID; //選取用
        public int Location_X; //OBJ Left/Top
        public int Location_Y;
        public int Center_X; //Center Pos
        public int Center_Y;
        public int Location_X2; //for Line

        public int Location_Y2;

        //flow ID Link 串接用
        public bool IsLink = false;
        public int LinkID_Yes;
        public int LinkID_No;
        public int LinkID_To;
        public int LinkID_IN;
        public int LinkID_RETURN_IN; //6-23

        public int SeT_IFModeID; //Table ID
        public int Set_IFOption; //選項
        public int Set_IFIDcheck; //被比較的ID
        public int Set_IFIndex; //Table INT index
        public int Set_IFCondition; //判斷條件

        // 大小尺寸
        public int Size_Width;
        public int Size_Height;

        public FlowChartOBJ()
        {
            ID = 0;
            Location_X = 0;
            Location_Y = 0;
            Size_Width = 100;
            Size_Height = 100;
            LinkID_IN = 255;
            LinkID_No = 255;
            LinkID_Yes = 255;
            LinkID_To = 255;
            LinkID_RETURN_IN = 255;
            //------------判斷用
            SeT_IFModeID = 0; //ID
            Set_IFIDcheck = 0;
            Set_IFOption = 0;
            Set_IFIndex = 0;
            Set_IFCondition = 2;
        }

        public List<string> WorkList { get; set; } = new List<string>(); //工作清單 Modes
        
        // Note: Color is not JSON-serializable by default. Store as string (e.g., #RRGGBB) or ARGB int
        [JsonIgnore]
        public List<Color> WorkList_Color { get; set; } = new List<Color>(); //工作清單 Modes_Color
        
        // Color values serialized as ARGB integers for JSON compatibility
        public List<int> WorkList_ColorArgb { get; set; } = new List<int>();
        
        private string type = ""; //種類
        private string ContentText = ""; //元件內容
        private string Name = ""; //元件名稱 唯一性
        private string Content = ""; //標註內容說明用
        private int ExeCounter = 0; //執行計數

        public int DoCounter
        {
            get { return ExeCounter; }
            set { ExeCounter = value; }
        }

        public string AddressName
        {
            get { return Name; }
            set { Name = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Caption
        {
            get { return ContentText; }
            set { ContentText = value; }
        }

        public string Info
        {
            get { return Content; }
            set { Content = value; }
        }
        
        // Helper methods for color conversion
        public void SyncColorsToArgb()
        {
            WorkList_ColorArgb.Clear();
            foreach (var color in WorkList_Color)
            {
                WorkList_ColorArgb.Add(color.ToArgb());
            }
        }
        
        public void SyncColorsFromArgb()
        {
            WorkList_Color.Clear();
            foreach (var argb in WorkList_ColorArgb)
            {
                WorkList_Color.Add(Color.FromArgb(argb));
            }
        }

        public override string ToString()
        {
            return $"ID: {ID}, Type: {Type}, Name: {Name}, Location: ({Location_X}, {Location_Y}), Size: ({Size_Width}x{Size_Height})";
        }
    }

    public enum ShowPictureType
    {
        None = 0,
        Flow = 1,
        Map = 2,
    }

    /// <summary>
    /// Message for transferring images between RpcServer and RpcClient
    /// </summary>
    public class ImageTransferMessage
    {
        public ShowPictureType PictureType { get; set; } = ShowPictureType.None;
        
        /// <summary>
        /// Image file path (if sending by path)
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Base64-encoded image data (if sending by data)
        /// </summary>
        public string ImageDataBase64 { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional: original file name
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}
