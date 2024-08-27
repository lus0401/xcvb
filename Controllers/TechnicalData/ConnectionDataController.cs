using Grpc.Core;
using Grpc.Net.Client;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.file.Data;
using IX.MW.DA.CLIENT.ASSETDATA.COMMON;
using IX.MW.DA.CLIENT.ASSETDATA.CONNECTIONDATA;
using IX.MW.DA.CLIENT.COMMON;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Common;
using System.Diagnostics;

namespace IX.DS.FW_Mockup.Controllers.TechnicalData
{
    [ApiController]
    [Route("/TechnicalData/[controller]")]
    public class ConnectionDataController : ControllerBase,IDisposable
    {
        private static readonly ConnectionDataVO[] MockupData = new[]
        {
            new ConnectionDataVO(){AssetCode=0x00000001,ConnectionData= $@"{{""asset_code"":0x00000001,""ip"":""127.0.0.1"",""port"":5003    }}" },
            new ConnectionDataVO(){AssetCode=0x00000002,ConnectionData= $@"{{""asset_code"":0x00000002,""ip"":""127.0.0.1"",""port"":5004    }}" },
            new ConnectionDataVO(){AssetCode=0x00000003,ConnectionData= $@"{{""asset_code"":0x00000003,""ip"":""127.0.0.1"",""port"":5005    }}"},
            new ConnectionDataVO(){AssetCode=0x0000000f,ConnectionData= $@"{{""asset_code"":0x0000000f,""ip"":""127.0.0.1"",""port"":5006    }}" }
        };

        public IConfiguration Conf { get; set; }
        public MemoryDataAdapter dataAdapter { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }

        private readonly ILogger<ConnectionDataController> _logger;

        public ConnectionDataController(ILogger<ConnectionDataController> logger, IConfiguration Conf)
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

        [HttpPost(Name = "InsertConnData")]
        public Result Post([FromBody] CONNECTIONDATA_FILE_TCP_MODEL data)
        {
            return InsertConnData(data);
        }

        private Result InsertConnData(CONNECTIONDATA_FILE_TCP_MODEL data)
        {
            Result result = new Result() { AssetCode = 0, IsSucc = false, Msg = "" };
            string strResponse = "";
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://192.168.0.67:5002"))
            {
                uint ASSET_CODE = dataAdapter.RegistConnectionData(data.fileid, data);
                ConnectionDataSvc.ConnectionDataSvcClient client = new ConnectionDataSvc.ConnectionDataSvcClient(DA_CHANNEL);
                result = client.InsertConnData(new ConnectionDataVO() { AssetCode = ASSET_CODE, ConnectionData = JsonConvert.SerializeObject(data) });
                result.AssetCode = ASSET_CODE;
                strResponse += $"InsertConnData-AssetCode:{ASSET_CODE} IsSucc:{result.IsSucc}  , msg:{result.Msg}" + Environment.NewLine;
                Debug.WriteLine($"InsertConnData-AssetCode:{ASSET_CODE} IsSucc:{result.IsSucc}  , msg:{result.Msg}");
            }
            return result;
        }

        [HttpPut(Name = "UpdateConnData")]
        public Result Put([FromBody] CONNECTIONDATA_FILE_TCP_MODEL data)
        {
            Result result = new Result();
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://192.168.0.67:5002"))
            {
                ConnectionDataSvc.ConnectionDataSvcClient client = new ConnectionDataSvc.ConnectionDataSvcClient(DA_CHANNEL);

                uint ASSET_CODE = dataAdapter.RegistConnectionData(data.fileid, data);
                result = client.UpdateConnData(new ConnectionDataVO() { AssetCode = ASSET_CODE, ConnectionData = JsonConvert.SerializeObject(data) });
                result.AssetCode = ASSET_CODE;
                Debug.WriteLine($"InsertConnData-AssetCode:{ASSET_CODE} IsSucc:{result.IsSucc}  , msg:{result.Msg}");

            }
            return result;
        }

        [HttpGet(Name = "GetConnDataData")]
        public CONNECTIONDATA_TCP Get(UInt32 AssetCode)
        {
            string strResponse = "";
            ConnectionDataVO data = new ConnectionDataVO();
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://192.168.0.67:5002"))
            {
                ConnectionDataSvc.ConnectionDataSvcClient client = new ConnectionDataSvc.ConnectionDataSvcClient(DA_CHANNEL);
                data = client.GetConnectionData(new Asset_Code() { AssetCode = AssetCode });
                strResponse += $"GetConnDataData- {data.AssetCode} {data.ConnectionData} \r\n";
                Debug.WriteLine($"GetConnDataData- {data.AssetCode} {data.ConnectionData} ");
            }
            CONNECTIONDATA_TCP data_des = JsonConvert.DeserializeObject<CONNECTIONDATA_TCP>(data.ConnectionData);
            return data_des;
        }

        public void Dispose()
        {
            this.DA_CHANNEL.Dispose();
        }
    }
}
