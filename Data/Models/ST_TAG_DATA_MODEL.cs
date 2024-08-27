namespace IX.DS.DA.Common.Models.TagData
{
    public class ST_ASSET_TAGDATA_MODEL
    {
        public UInt32 ASSET_CODE { get; set; }

        public ST_TAG_DATA_MODEL[] DATA { get; set; }
    }
    public class ST_TAG_DATA_MODEL
    {
        public UInt32 ASSET_CODE { get; set; }
        public UInt64 TAG_ST_CODE { get; set; }
        public bool IS_READ_ONLY { get; set; }
        public bool USE_CONTROL { get; set; }
        public string NODE_ID { get; set; }
        public string TAG_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public int CYCLE_TIME { get; set; }
        public string DESCRIPTION { get; set; }
    }
}
