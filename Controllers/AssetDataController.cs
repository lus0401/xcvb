using Grpc.Net.Client;
using IX.DS.DA.Common.Models.TagData;
using IX.DS.FW_Mockup.Controllers.TechnicalData;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.Data.Models.FILE;
using IX.DS.FW_Mockup.file.Data;
using IX.MW.DA.CLIENT.ASSETDATA.ASSETCONNECTIONDATA;
using IX.MW.DA.CLIENT.ASSETDATA.COMMON;
using IX.MW.DA.CLIENT.COMMON;
using IX.MW.DA.CLIENT.TAG;
using IX.MW.DA.CLIENT.TAG.ST;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IX.DS.FW_Mockup.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class AssetDataController : ControllerBase,IDisposable
    {


        private readonly ILogger<AssetConnectionDataController> _logger;
        public IConfiguration Conf { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }
        private MemoryDataAdapter dataAdapter { get; set; }
        public AssetDataController(ILogger<AssetConnectionDataController> logger,IConfiguration Conf)
        {
            _logger = logger;

            this.Conf = Conf;
            string? strDA_URL = Conf.GetSection("DA_URL").Value;
            if (strDA_URL == null)
            {
                Debug.WriteLine("Not Defined DA URL");
                throw new Exception("Not Defined DA URL");
            }
            this.DA_CHANNEL = GrpcChannel.ForAddress(strDA_URL);
            this.dataAdapter = MemoryDataAdapter.getInstance();
        }







        // POST api/<AssetDataController>
        [HttpPost]
        public Result Post([FromBody] dynamic value)
        {
            Result result = new Result();
            uint ASSET_CODE = 0;
            try
            {
                dynamic AssetAllData = JsonConvert.DeserializeObject<dynamic>(value.ToString());
                dynamic OpData = AssetAllData.OperationalData;
                string strFileID = OpData.fileid.Value;
                TagStandardFileDataModel[] TagData = JsonConvert.DeserializeObject<TagStandardFileDataModel[]>(OpData.data.ToString());
                CONNECTION_MASTER_FILE_MODEL CONNECTION_MASTER_FILE = JsonConvert.DeserializeObject<CONNECTION_MASTER_FILE_MODEL>(AssetAllData.TechnicalData.data.Connection_Master.ToString());
                CONNECTIONDATA_FILE_TCP_MODEL CONNECTION_DATA_FILE = JsonConvert.DeserializeObject<CONNECTIONDATA_FILE_TCP_MODEL>(AssetAllData.TechnicalData.data.Connection_Data.ToString());
                PROTOCOLDATA_FILE_Model PROTOCOL_DATA_FILE = JsonConvert.DeserializeObject<PROTOCOLDATA_FILE_Model>(AssetAllData.TechnicalData.data.Protocol_Data.ToString());
                ASSET_CONNECTION_FILE_MODEL TechnicalData = JsonConvert.DeserializeObject<ASSET_CONNECTION_FILE_MODEL>(AssetAllData.TechnicalData.ToString());
                TechnicalData.CONNECTIONDATA = CONNECTION_DATA_FILE;
                TechnicalData.CONNECTION_MASTER = CONNECTION_MASTER_FILE;
                TechnicalData.PROTOCOL_DATA = PROTOCOL_DATA_FILE;

                ASSET_CODE = dataAdapter.RegistConnectionMaster(TechnicalData.fileid, TechnicalData.CONNECTION_MASTER);
                dataAdapter.RegistConnectionData(TechnicalData.fileid, TechnicalData.CONNECTIONDATA);
                dataAdapter.RegistProtocolData(TechnicalData.fileid, TechnicalData.PROTOCOL_DATA);
                AssetConnectionDataVO data = new AssetConnectionDataVO()
                {
                    AssetCode = ASSET_CODE,
                    ConnectionMasterVO = new ConnectionMasterVO()
                    {
                        AssetCode = ASSET_CODE,
                        ConnectionType = Enum.Parse<CONNECTION_TYPE>(TechnicalData.CONNECTION_MASTER.CONNECTION_TYPE),
                        ProtocolType = Enum.Parse<PROTOCOL_TYPE>(TechnicalData.CONNECTION_MASTER.PROTOCOL_TYPE)
                    },
                    ConnectionData = JsonConvert.SerializeObject(new CONNECTIONDATA_TCP() { ASSET_CODE = ASSET_CODE, IP = TechnicalData.CONNECTIONDATA.IP, PORT = TechnicalData.CONNECTIONDATA.PORT }),
                    ProtocolData = JsonConvert.SerializeObject(new PROTOCOLDATA_MODEL()
                    {
                        ASSET_CODE = ASSET_CODE
                       ,
                        AuthFilePath = TechnicalData.PROTOCOL_DATA.AuthFilePath
                       ,
                        AuthFilePWD = TechnicalData.PROTOCOL_DATA.AuthFilePWD
                       ,
                        AUTHMODE = TechnicalData.PROTOCOL_DATA.AUTHMODE
                       ,
                        SECURITY_MODE = TechnicalData.PROTOCOL_DATA.SECURITY_MODE
                       ,
                        SECURITY_POLICY = TechnicalData.PROTOCOL_DATA.SECURITY_POLICY
                       ,
                        URL = TechnicalData.PROTOCOL_DATA.URL
                       ,
                        USERID = TechnicalData.PROTOCOL_DATA.USERID
                       ,
                        USERPW = TechnicalData.PROTOCOL_DATA.USERPW
                    })

                };
                string? strDA_URL = Conf.GetSection("DA_URL").Value;
                if (strDA_URL==null)
                {
                    Debug.WriteLine("Not Defined DA URL");
                    throw new Exception("Not Defined DA URL");
                }
                //using (GrpcChannel channel = GrpcChannel.ForAddress(strDA_URL))
                //using (GrpcChannel channel = GrpcChannel.ForAddress("http://0.tcp.jp.ngrok.io:17594"))
                {
                    AssetConnectionDataSvc.AssetConnectionDataSvcClient client = new AssetConnectionDataSvc.AssetConnectionDataSvcClient(DA_CHANNEL);
                    result = client.InsertAssetConnectionData(data);
                    Debug.WriteLine($"InsertAssetConnectionData-AssetCode:{data.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}");
                
                    StAssetTagDataVO AssetTagStandard = new StAssetTagDataVO() { AssetCode=ASSET_CODE};
                    UInt64[] TagStCodes = dataAdapter.RegistTagStandard(strFileID, TagData);
                    StTagDataSvc.StTagDataSvcClient client2 = new StTagDataSvc.StTagDataSvcClient(DA_CHANNEL);
                    foreach (UInt64 tag_st_code in TagStCodes)
                    {
                        AssetTagStandard.StTagData.Add(new StTagDataVO()
                        {
                            TagStCode = tag_st_code,
                            StandardData = JsonConvert.SerializeObject(new ST_TAG_DATA_MODEL()
                            {
                                ASSET_CODE = ASSET_CODE,
                                TAG_ST_CODE = tag_st_code,
                                NODE_ID = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].NODE_ID??"",
                                CYCLE_TIME = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].CYCLE_TIME ,
                                DATA_TYPE = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].DATA_TYPE ?? "",
                                DESCRIPTION = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].DESCRIPTION ?? "",
                                IS_READ_ONLY = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].IS_READ_ONLY ,
                                TAG_NAME = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].TAG_NAME ?? "",
                                USE_CONTROL = dataAdapter.TAG_STANDARD_DATA_DIC[ASSET_CODE][tag_st_code].USE_CONTROL 
                            })
                        }); ;
                    }
                    result= client2.InsertStAssetTagData(AssetTagStandard);
                }



            }
            catch (Exception ex)
            {
                result = new Result() { AssetCode = ASSET_CODE, IsSucc = false, Msg = ex.Message };
            }


            return result;
        }

        public void Dispose()
        {
            this.DA_CHANNEL.Dispose();
        }
    }
}

public class AssetAllDataModel
{
    public ASSET_CONNECTION_FILE_MODEL OperationalData { get; set; }
    public string TechnicalData { get; set; }
}