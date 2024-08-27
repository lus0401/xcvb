using Grpc.Core;
using Grpc.Net.Client;
using IX.DS.FW_Mockup.Data;
using IX.MW.DA.CLIENT.ASSETDATA;
using IX.MW.DA.CLIENT.ASSETDATA.COMMON;
using IX.MW.DA.CLIENT.ASSETDATA.PROTOCOLDATA;
using IX.MW.DA.CLIENT.COMMON;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IX.DS.FW_Mockup.Controllers.TechnicalData
{
    [ApiController]
    [Route("/TechnicalData/[controller]")]
    public class ProtocolDataController : ControllerBase,IDisposable
    {
        private static readonly ProtocolDataVO[] MockupData = new[]
        {
            new ProtocolDataVO(){AssetCode=0x00000001,ProtocolData= $@"{{""ASSET_CODE"":0x00000001,""AUTHMODE"":""NONE1"",""USERID"":"""",""USERPW"":"""",""SECURITY_MODE"":""NONE"",""SECURITY_POLICY"":"""",""AuthFilePath"":"""",""AuthFilePWD"":"""",""URL"":""""}}" },
            new ProtocolDataVO(){AssetCode=0x00000002,ProtocolData= $@"{{""ASSET_CODE"":0x00000002,""AUTHMODE"":""NONE2"",""USERID"":"""",""USERPW"":"""",""SECURITY_MODE"":""NONE"",""SECURITY_POLICY"":"""",""AuthFilePath"":"""",""AuthFilePWD"":"""",""URL"":""""}}" },
            new ProtocolDataVO(){AssetCode=0x00000003,ProtocolData= $@"{{""ASSET_CODE"":0x00000003,""AUTHMODE"":""NONE3"",""USERID"":"""",""USERPW"":"""",""SECURITY_MODE"":""NONE"",""SECURITY_POLICY"":"""",""AuthFilePath"":"""",""AuthFilePWD"":"""",""URL"":""""}}"},
            new ProtocolDataVO(){AssetCode=0x0000000f,ProtocolData= $@"{{""ASSET_CODE"":0x00000004,""AUTHMODE"":""NONE4"",""USERID"":"""",""USERPW"":"""",""SECURITY_MODE"":""NONE"",""SECURITY_POLICY"":"""",""AuthFilePath"":"""",""AuthFilePWD"":"""",""URL"":""""}}" }
        };


        private MemoryDataAdapter dataAdapter { get; set; }
        public IConfiguration Conf { get; set; }
        public MemoryDataAdapter DataAdapter { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }


        private readonly ILogger<ProtocolDataController> _logger;

        public ProtocolDataController(ILogger<ProtocolDataController> logger, IConfiguration Conf)
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





        [HttpPost(Name = "InsertProtocolData")]
        public string Post(JsonContent param)
        {
            string strResponse = "";
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
                ProtocolDataSvc.ProtocolDataSvcClient client = new ProtocolDataSvc.ProtocolDataSvcClient(DA_CHANNEL);
                foreach (ProtocolDataVO mockup_data in MockupData)
                {
                    Result result = client.InsertProtocolData(mockup_data);
                    strResponse += $"InsertProtocolData-AssetCode:{mockup_data.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}" + Environment.NewLine;
                    Debug.WriteLine($"InsertProtocolData-AssetCode:{mockup_data.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}");
                }
            }
            return strResponse;
        }

        [HttpPut(Name = "UpdateProtocolData")]
        public string Put()
        {
            string strResponse = "";
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
                ProtocolDataSvc.ProtocolDataSvcClient client = new ProtocolDataSvc.ProtocolDataSvcClient(DA_CHANNEL);
                foreach (ProtocolDataVO mockup_data in MockupData)
                {
                    Result result = client.UpdateProtocolData(mockup_data);
                    strResponse += $"UpdateProtocolData-AssetCode:{mockup_data.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}" + Environment.NewLine;
                    Debug.WriteLine($"UpdateProtocolData-AssetCode:{mockup_data.AssetCode} IsSucc:{result.IsSucc}  , msg:{result.Msg}");
                }
            }
            return strResponse;
        }

        [HttpGet(Name = "GetProtocolData")]
        public string Get()
        {
            string strResponse = "";
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
                ProtocolDataSvc.ProtocolDataSvcClient client = new ProtocolDataSvc.ProtocolDataSvcClient(DA_CHANNEL);
                foreach (ProtocolDataVO mockup_data in MockupData)
                {
                    ProtocolDataVO data = client.GetProtocolData(new Asset_Code() { AssetCode = mockup_data.AssetCode });
                    strResponse += $"GetProtocolData- {data.AssetCode} {data.ProtocolData} \r\n";
                    Debug.WriteLine($"GetProtocolData- {data.AssetCode} {data.ProtocolData} ");
                }
            }
            return strResponse;
        }

        public void Dispose()
        {
            DA_CHANNEL.Dispose();
        }
    }
}
