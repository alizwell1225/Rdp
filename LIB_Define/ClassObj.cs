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

        public List<string> WorkList = new List<string>(); //工作清單 Modes
        public List<Color> WorkList_Color = new List<Color>(); //工作清單 Modes_Color
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
    }

    public enum ShowPictureType
    {
        None = 0,
        Flow = 1,
        Map = 2,
    }
}
