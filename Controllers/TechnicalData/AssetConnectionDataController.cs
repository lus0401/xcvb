using Grpc.Core;
using Grpc.Net.Client;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.file.Data;
using IX.MW.DA.CLIENT.ASSETDATA.ASSETCONNECTIONDATA;
using IX.MW.DA.CLIENT.ASSETDATA.COMMON;
using IX.MW.DA.CLIENT.COMMON;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Common;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IX.DS.FW_Mockup.Controllers.TechnicalData
{
    [ApiController]
    [Route("/TechnicalData/[controller]")]
    public class AssetConnectionDataController : ControllerBase,IDisposable
    {
        private readonly ILogger<AssetConnectionDataController> _logger;
        public IConfiguration Conf { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }
        private MemoryDataAdapter dataAdapter { get; set; }
        public AssetConnectionDataController(ILogger<AssetConnectionDataController> logger, IConfiguration Conf)
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

        [HttpPost(Name = "InsertAssetConnectionData")]
        public Result Post([FromBody]ASSET_CONNECTION_FILE_MODEL ASSET_CONNECTION_DATA)
        {
            uint ASSET_CODE = dataAdapter.RegistConnectionMaster(ASSET_CONNECTION_DATA.fileid, ASSET_CONNECTION_DATA.CONNECTION_MASTER);
            dataAdapter.RegistConnectionData(ASSET_CONNECTION_DATA.fileid, ASSET_CONNECTION_DATA.CONNECTIONDATA);
            dataAdapter.RegistProtocolData(ASSET_CONNECTION_DATA.fileid, ASSET_CONNECTION_DATA.PROTOCOL_DATA);
            AssetConnectionDataVO data= new AssetConnectionDataVO() 
            {
                AssetCode = ASSET_CODE,
                ConnectionMasterVO=new ConnectionMasterVO() 
                {
                    AssetCode=ASSET_CODE,
                    ConnectionType= Enum.Parse<CONNECTION_TYPE>(ASSET_CONNECTION_DATA.CONNECTION_MASTER .CONNECTION_TYPE),
                    ProtocolType= Enum.Parse<PROTOCOL_TYPE>(ASSET_CONNECTION_DATA.CONNECTION_MASTER.PROTOCOL_TYPE)
                },
                ConnectionData= JsonConvert.SerializeObject(new CONNECTIONDATA_TCP() {ASSET_CODE=ASSET_CODE,IP=ASSET_CONNECTION_DATA.CONNECTIONDATA.IP,PORT=ASSET_CONNECTION_DATA.CONNECTIONDATA.PORT }),
                ProtocolData = JsonConvert.SerializeObject(new PROTOCOLDATA_MODEL() 
                {
                    ASSET_CODE = ASSET_CODE
                   ,AuthFilePath= ASSET_CONNECTION_DATA.PROTOCOL_DATA.AuthFilePath
                   ,AuthFilePWD = ASSET_CONNECTION_DATA.PROTOCOL_DATA.AuthFilePWD
                   ,AUTHMODE = ASSET_CONNECTION_DATA.PROTOCOL_DATA.AUTHMODE
                   ,SECURITY_MODE = ASSET_CONNECTION_DATA.PROTOCOL_DATA.SECURITY_MODE
                   ,SECURITY_POLICY = ASSET_CONNECTION_DATA.PROTOCOL_DATA.SECURITY_POLICY
                   ,URL = ASSET_CONNECTION_DATA.PROTOCOL_DATA.URL
                   ,USERID = ASSET_CONNECTION_DATA.PROTOCOL_DATA.USERID
                   ,USERPW = ASSET_CONNECTION_DATA.PROTOCOL_DATA.USERPW
                })

            };
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
                AssetConnectionDataSvc.AssetConnectionDataSvcClient client = new AssetConnectionDataSvc.AssetConnectionDataSvcClient(DA_CHANNEL);
                Result result = client.InsertAssetConnectionData(data);
                Debug.WriteLine($"InsertAssetConnectionData-AssetCode:{data.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}");
                return result;
            }

        }

        [HttpPut(Name = "UpdateAssetConnectionData")]
        public Result Put([FromBody] ASSET_CONNECTION_FILE_MODEL ASSET_CONNECTION_DATA)
        {
            if (!this.dataAdapter.Assetcode_file_dic.ContainsKey(ASSET_CONNECTION_DATA.fileid))
            {
                return new Result() {AssetCode=0,IsSucc=false,Msg="해당 에셋파일은 등록되지 않은 파일입니다. 데이터를 업데이트 할수 없습니다" };
            }
            Result result =new Result();
            uint ASSET_CODE = dataAdapter.Assetcode_file_dic[ASSET_CONNECTION_DATA.fileid];
            dataAdapter.RegistConnectionData(ASSET_CONNECTION_DATA.fileid, ASSET_CONNECTION_DATA.CONNECTIONDATA);
            dataAdapter.RegistProtocolData(ASSET_CONNECTION_DATA.fileid, ASSET_CONNECTION_DATA.PROTOCOL_DATA);
            AssetConnectionDataVO data = new AssetConnectionDataVO()
            {
                AssetCode = ASSET_CODE,
                ConnectionMasterVO = new ConnectionMasterVO()
                {
                    AssetCode = ASSET_CODE,
                    ConnectionType = Enum.Parse<CONNECTION_TYPE>(ASSET_CONNECTION_DATA.CONNECTION_MASTER.CONNECTION_TYPE),
                    ProtocolType = Enum.Parse<PROTOCOL_TYPE>(ASSET_CONNECTION_DATA.CONNECTION_MASTER.PROTOCOL_TYPE)
                },
                ConnectionData = JsonConvert.SerializeObject(new CONNECTIONDATA_TCP() { ASSET_CODE = ASSET_CODE, IP = ASSET_CONNECTION_DATA.CONNECTIONDATA.IP, PORT = ASSET_CONNECTION_DATA.CONNECTIONDATA.PORT }),
                ProtocolData = JsonConvert.SerializeObject(new PROTOCOLDATA_MODEL()
                {
                    ASSET_CODE = ASSET_CODE
                   ,
                    AuthFilePath = ASSET_CONNECTION_DATA.PROTOCOL_DATA.AuthFilePath
                   ,
                    AuthFilePWD = ASSET_CONNECTION_DATA.PROTOCOL_DATA.AuthFilePWD
                   ,
                    AUTHMODE = ASSET_CONNECTION_DATA.PROTOCOL_DATA.AUTHMODE
                   ,
                    SECURITY_MODE = ASSET_CONNECTION_DATA.PROTOCOL_DATA.SECURITY_MODE
                   ,
                    SECURITY_POLICY = ASSET_CONNECTION_DATA.PROTOCOL_DATA.SECURITY_POLICY
                   ,
                    URL = ASSET_CONNECTION_DATA.PROTOCOL_DATA.URL
                   ,
                    USERID = ASSET_CONNECTION_DATA.PROTOCOL_DATA.USERID
                   ,
                    USERPW = ASSET_CONNECTION_DATA.PROTOCOL_DATA.USERPW
                })

            };
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
                AssetConnectionDataSvc.AssetConnectionDataSvcClient client = new AssetConnectionDataSvc.AssetConnectionDataSvcClient(DA_CHANNEL);
                result = client.UpdateAssetConnectionData(data);
                Debug.WriteLine($"UpdateAssetConnectionData-IsSucc:{result.IsSucc}  , msg:{result.Msg}");
            }
            return result;
        }

        [HttpGet(Name = "GetAssetConnectionData")]
        public AssetConnectionDataVO Get(UInt32 Asset_code)
        {
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
                AssetConnectionDataSvc.AssetConnectionDataSvcClient client = new AssetConnectionDataSvc.AssetConnectionDataSvcClient(DA_CHANNEL);
                AssetConnectionDataVO data = client.GetAssetConnectionData(new Asset_Code() { AssetCode = Asset_code });
                Debug.WriteLine($"GetAssetConnectionData- {data.AssetCode} {data.ConnectionMasterVO.ConnectionType} {data.ConnectionMasterVO.ProtocolType}");
                return data;
                
            }
        }

        public void Dispose()
        {
            this.DA_CHANNEL.Dispose();
        }
    }
}
