using IX.DS.FW_Mockup.Data;

namespace IX.DS.FW_Mockup.file.Data
{
    public class ASSET_CONNECTION_FILE_MODEL
    {
        public string fileid { get; set; }
        public CONNECTION_MASTER_FILE_MODEL CONNECTION_MASTER { get; set; }
        public CONNECTIONDATA_FILE_TCP_MODEL CONNECTIONDATA { get; set; }
        public PROTOCOLDATA_FILE_Model PROTOCOL_DATA { get; set; }

    }
}
