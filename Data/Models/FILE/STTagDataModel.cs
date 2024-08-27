using IX.DS.DA.Common.Models.TagData;

namespace IX.DS.FW_Mockup.Data.Models.FILE
{
    /// <summary>
    /// 태그 기준정보
    /// </summary>
    public class STTagDataModel
    {
        public string fileid { get; set; }

        public TagStandardFileDataModel[] data { get; set; }
    }
    public class TagStandardFileDataModel
    {
        public bool IS_READ_ONLY { get; set; }
        public bool USE_CONTROL { get; set; }
        public string NODE_ID { get; set; }
        public string TAG_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public int CYCLE_TIME { get; set; }
        public string DESCRIPTION { get; set; }
    }

}
