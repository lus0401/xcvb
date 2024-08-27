using Grpc.Net.Client;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.Data.Models;
using IX.DS.FW_Mockup.Data.Models.FILE;
using IX.MW.DA.CLIENT.COMMON;
using IX.MW.DA.CLIENT.TAG;
using IX.MW.DA.CLIENT.TAG.VALUE;


//using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IX.DS.FW_Mockup.Controllers.OperationalData
{
    [Route("/OperationalData/[controller]")]
    [ApiController]
    public class TagValueController : ControllerBase,IDisposable
    {
        public IConfiguration Conf { get; set; }
        public MemoryDataAdapter DataAdapter { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }

        public TagValueController(IConfiguration Conf)
        {
            this.Conf = Conf;
            string? strDA_URL = Conf.GetSection("DA_URL").Value;
            if (strDA_URL == null)
            {
                Debug.WriteLine("Not Defined DA URL");
                throw new Exception("Not Defined DA URL");
            }
            this.DA_CHANNEL = GrpcChannel.ForAddress(strDA_URL);
            this.DataAdapter = MemoryDataAdapter.getInstance();
        }


        //// GET: api/<TagStandardController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<TagStandardController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<TagStandardController>
        [HttpGet(Name = "GetTagValueData")]
        public RTTagDataVO Get(UInt64 Tag_St_Code)
        {
            Tag_St_Code TagStCode = new Tag_St_Code() { TagStCode = Tag_St_Code };
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://0.tcp.jp.ngrok.io:17594"))
            {
                TagValueDataSvc.TagValueDataSvcClient client = new TagValueDataSvc.TagValueDataSvcClient(DA_CHANNEL);
                RTTagDataVO data = client.GetTagValueData(TagStCode);
                return data;
            }
        }

        [HttpGet("GetTagValueDataArray")]
        public RTTagDataVOs GetTagValueDataArray(UInt32 Asset_Code)
        {
            Asset_Code _Asset_Code = new Asset_Code() { AssetCode = Asset_Code };
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://0.tcp.jp.ngrok.io:17594"))
            {
                TagValueDataSvc.TagValueDataSvcClient client = new TagValueDataSvc.TagValueDataSvcClient(DA_CHANNEL);
                RTTagDataVOs data = client.GetTagValueDataArray(_Asset_Code);
                return data;
            }
        }

        [HttpGet("FwTagValue")]
        public RT_TAG_DATA_MODEL FwTagValue(UInt32 Asset_code,UInt64 Tag_St_Code)
        {
            return this.DataAdapter.TAG_REAL_DATA_DIC[Asset_code][Tag_St_Code];
        }


        // POST api/<TagStandardController>
        [HttpPost(Name = "InsertTagValueData")]
        public Result Post(RT_TAG_DATA_MODEL RTTagDataVO)
        {
            Asset_Codes asset_Codes = new Asset_Codes();
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://0.tcp.jp.ngrok.io:17594"))
            {
                RTTagDataVO PARAM = new RTTagDataVO()
                {
                    AssetCode = RTTagDataVO.ASSET_CODE,
                    TagStCode = RTTagDataVO.TAG_ST_CODE,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Value = RTTagDataVO.REAL_TIME_DATA
                };
                TagValueDataSvc.TagValueDataSvcClient client = new TagValueDataSvc.TagValueDataSvcClient(DA_CHANNEL);
                Result data = client.InsertTagValueData(PARAM);
                return data;
            }
        }



        // POST api/<TagStandardController>
        [HttpPost("InsertTagValueDataArray")]
        public IX.MW.DA.CLIENT.COMMON.Results Post(RT_TAG_DATA_MODEL[] RTTagDataVO)
        {
            Asset_Codes asset_Codes = new Asset_Codes();
            RTTagDataVOs PARAM_LIST = new RTTagDataVOs();
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://0.tcp.jp.ngrok.io:17594"))
            {
                foreach (RT_TAG_DATA_MODEL data in RTTagDataVO)
                {
                    RTTagDataVO PARAM = new RTTagDataVO()
                    {
                        AssetCode = data.ASSET_CODE,
                        TagStCode = data.TAG_ST_CODE,
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        Value = data.REAL_TIME_DATA
                    };
                    PARAM_LIST.RTTagDataVO.Add(PARAM);
                }

                TagValueDataSvc.TagValueDataSvcClient client = new TagValueDataSvc.TagValueDataSvcClient(DA_CHANNEL);
                IX.MW.DA.CLIENT.COMMON.Results Results = client.InsertTagValueDataArray(PARAM_LIST);
                return Results;
            }
        }

        public void Dispose()
        {
            this.DA_CHANNEL.Dispose();
        }
    }

}
