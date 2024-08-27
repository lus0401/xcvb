namespace IX.DS.FW_Mockup.Data.Models
{
    public class RT_TAG_DATA_MODEL
    {
        public UInt32 ASSET_CODE { get; set; }
        public UInt64 TAG_ST_CODE { get; set; }
        public string TIMESTAMP { get; set; }
        public string REAL_TIME_DATA { get; set; }
    }

    public class REAL_TIME_DATA_OPCUA 
    {
        public dynamic Value { get; set; }
        public int ServerPicoseconds { get; set; }
        public string ServerTimestamp { get; set; }
        public int SourcePicoseconds { get; set; }
        public string SourceTimestamp { get; set; }
        public string StatusCode { get; set; }
    }
}
