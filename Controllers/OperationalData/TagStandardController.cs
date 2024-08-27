using Grpc.Net.Client;
using IX.DS.DA.Common.Models.TagData;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.Data.Models.FILE;
using IX.MW.DA.CLIENT.COMMON;
using IX.MW.DA.CLIENT.TAG;
using IX.MW.DA.CLIENT.TAG.ST;





//using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IX.DS.FW_Mockup.Controllers.OperationalData
{
    [Route("/OperationalData/[controller]")]
    [ApiController]
    public class TagStandardController : ControllerBase,IDisposable
    {
        public IConfiguration Conf { get; set; }
        public MemoryDataAdapter DataAdapter { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }

        public TagStandardController(IConfiguration Conf)
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
        [HttpPost(Name = "InsertStAssetTagData")]
        public Result Post([FromBody] STTagDataModel data)
        {

            StAssetTagDataVO AssetTagStandard = new StAssetTagDataVO();
            AssetTagStandard.AssetCode = DataAdapter.Assetcode_file_dic[data.fileid];
            UInt64[] TagStCodes = DataAdapter.RegistTagStandard(data.fileid, data.data);

            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            {
               StTagDataSvc.StTagDataSvcClient client = new StTagDataSvc.StTagDataSvcClient(DA_CHANNEL);


                foreach (UInt64 tag_st_code in TagStCodes)
                {
                    AssetTagStandard.StTagData.Add(new StTagDataVO()
                    {
                        TagStCode = tag_st_code,
                        StandardData = JsonConvert.SerializeObject(new ST_TAG_DATA_MODEL()
                        {
                            ASSET_CODE = AssetTagStandard.AssetCode,
                            TAG_ST_CODE = tag_st_code,
                            NODE_ID = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].NODE_ID,
                            CYCLE_TIME = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].CYCLE_TIME,
                            DATA_TYPE = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].DATA_TYPE,
                            DESCRIPTION = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].DESCRIPTION,
                            IS_READ_ONLY = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].IS_READ_ONLY,
                            TAG_NAME = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].TAG_NAME,
                            USE_CONTROL = DataAdapter.TAG_STANDARD_DATA_DIC[AssetTagStandard.AssetCode][tag_st_code].USE_CONTROL
                        })
                    }); ;
                }
                return client.InsertStAssetTagData(AssetTagStandard);
            }
        }




        [HttpGet("FwTagStandard")]
        public ST_TAG_DATA_MODEL FwTagStandard(UInt32 Asset_code, UInt64 Tag_St_Code)
        {
            return this.DataAdapter.TAG_STANDARD_DATA_DIC[Asset_code][Tag_St_Code];
        }


        // PUT api/<TagStandardController>/5
        [HttpPut("{id}")]
        public void Put([FromBody] STTagDataModel data)
        {
            //string strFileId = data.fileid;
            //TagStandardFileDataModel[] _data = data.data;
            //DataAdapter.
        }

        public void Dispose()
        {
            this.DA_CHANNEL.Dispose();
        }

        //// DELETE api/<TagStandardController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
