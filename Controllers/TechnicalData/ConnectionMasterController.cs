using Grpc.Core;
using Grpc.Net.Client;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.file.Data;
using IX.MW.DA.CLIENT.ASSETDATA.COMMON;
using IX.MW.DA.CLIENT.ASSETDATA.CONNECTIONMASTER;
using IX.MW.DA.CLIENT.COMMON;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IX.DS.FW_Mockup.Controllers.TechnicalData
{
    [ApiController]
    [Route("/TechnicalData/[controller]")]
    public class ConnectionMasterController : ControllerBase,IDisposable
    {
        private static readonly ConnectionMasterVO[] MockupData = new[]
        {
            new ConnectionMasterVO(){AssetCode=0x00000001,ConnectionType=CONNECTION_TYPE.Tcp,ProtocolType=PROTOCOL_TYPE.OpcUa },
            new ConnectionMasterVO(){AssetCode=0x00000002,ConnectionType=CONNECTION_TYPE.Tcp,ProtocolType=PROTOCOL_TYPE.Modbus },
            new ConnectionMasterVO(){AssetCode=0x00000003,ConnectionType=CONNECTION_TYPE.Serial,ProtocolType=PROTOCOL_TYPE.OpcUa },
            new ConnectionMasterVO(){AssetCode=0x0000000f,ConnectionType=CONNECTION_TYPE.Serial,ProtocolType=PROTOCOL_TYPE.Modbus }
        };

        private MemoryDataAdapter dataAdapter { get; set; }
        public IConfiguration Conf { get; set; }
        public MemoryDataAdapter DataAdapter { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }

        private readonly ILogger<ConnectionMasterController> _logger;

        public ConnectionMasterController(ILogger<ConnectionMasterController> logger, IConfiguration Conf)
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

        [HttpPost(Name = "InsertConnMaster")]
        public Result Post([FromBody] CONNECTION_MASTER_FILE_MODEL data)
        {
            Result result = new Result() { AssetCode = 0, IsSucc = false, Msg = "" };
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://192.168.0.67:5002"))
            {
                uint ASSET_CODE = dataAdapter.RegistConnectionMaster(data.fileid, data);
                ConnectionMasterSvc.ConnectionMasterSvcClient client = new ConnectionMasterSvc.ConnectionMasterSvcClient(DA_CHANNEL);
                ConnectionMasterVO vo = new ConnectionMasterVO();
                vo.AssetCode = ASSET_CODE;
                vo.ConnectionType = Enum.Parse<CONNECTION_TYPE>(data.CONNECTION_TYPE);
                vo.ProtocolType = Enum.Parse<PROTOCOL_TYPE>(data.PROTOCOL_TYPE);
                result = client.InsertConnMaster(vo);
                result.AssetCode = ASSET_CODE;
                Debug.WriteLine($"InsertConnMaster-AssetCode:{vo.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}");
            }
            return result;
        }

        [HttpPut(Name = "UpdateConnMaster")]
        public Result Put([FromBody] CONNECTION_MASTER_FILE_MODEL data)
        {
            Result result = new Result() { AssetCode = 0, IsSucc = false, Msg = "" };
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://192.168.0.67:5002"))
            {
                ConnectionMasterSvc.ConnectionMasterSvcClient client = new ConnectionMasterSvc.ConnectionMasterSvcClient(DA_CHANNEL);
                foreach (ConnectionMasterVO mockup_data in MockupData)
                {
                    mockup_data.ConnectionType = CONNECTION_TYPE.Tcp;
                    result = client.UpdateConnMaster(mockup_data);
                    result.AssetCode = (UInt32)Convert.ChangeType(dataAdapter.Assetcode_file_dic[data.fileid], typeof(UInt32));
                    Debug.WriteLine($"UpdateConnMaster-IsSucc:{result.IsSucc}  , msg:{result.Msg}");
                }
            }
            return result;
        }

        [HttpGet(Name = "GetConnMasterData")]
        public CONNECTION_MASTER_MODEL Get(UInt32 AssetCode)
        {
            CONNECTION_MASTER_MODEL _data = new CONNECTION_MASTER_MODEL();
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://192.168.0.67:5002"))
            {
                ConnectionMasterSvc.ConnectionMasterSvcClient client = new ConnectionMasterSvc.ConnectionMasterSvcClient(DA_CHANNEL);
                ConnectionMasterVO data = client.GetConnMasterData(new Asset_Code() { AssetCode = AssetCode });
                Debug.WriteLine($"GetConnMasterData- {data.AssetCode} {data.ConnectionType} {data.ProtocolType}");

                //foreach (ConnectionMasterVO mockup_data in MockupData)
                //{
                //    ConnectionMasterVO data = client.GetConnMasterData(new Asset_Code() { AssetCode = mockup_data.AssetCode });
                //    Debug.WriteLine($"GetConnMasterData- {data.AssetCode} {data.ConnectionType} {data.ProtocolType}");
                //}
                _data.CONNECTION_TYPE = Enum.GetName<CONNECTION_TYPE>(data.ConnectionType);
                _data.PROTOCOL_TYPE = Enum.GetName<PROTOCOL_TYPE>(data.ProtocolType);
                _data.ASSET_CODE = data.AssetCode;
            }
            return _data;
        }

        public void Dispose()
        {
            this.DA_CHANNEL.Dispose();
        }
    }
}
