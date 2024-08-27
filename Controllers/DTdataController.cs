using Grpc.Net.Client;
using IX.DS.DA.Common.Models.TagData;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.Data.Models;
using IX.MW.DA.CLIENT.COMMON;
using IX.MW.DA.CLIENT.TAG;
using IX.MW.DA.CLIENT.TAG.VALUE;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IX.DS.FW_Mockup.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class DTdataController : ControllerBase, IDisposable
    {
        public IConfiguration Conf { get; set; }
        public GrpcChannel DA_CHANNEL { get; set; }

        public DTdataController(IConfiguration conf)
        {
            this.Conf = conf;
            string? strDA_URL = Conf.GetSection("DA_URL").Value;
            if (strDA_URL == null)
            {
                Debug.WriteLine("Not Defined DA URL");
                throw new Exception("Not Defined DA URL");
            }
            this.DA_CHANNEL = GrpcChannel.ForAddress(strDA_URL);
        }


        // POST api/<DTdataController>
        [HttpPost("cmd")]
        public Result cmd([FromBody] DT_CMD_MODEL cmd)
        {
            try
            {
                // cmd 받아서 해당 태그의 태그기준정보코드 검색
                ST_TAG_DATA_MODEL? ST_TAG_DATA= MemoryDataAdapter.getInstance().TAG_STANDARD_DATA_DIC[cmd.ASSET_CODE].Values.FirstOrDefault((st) => { return st.TAG_NAME == cmd.TAG_NAME; });
                if (ST_TAG_DATA==null)
                {
                    throw new Exception($"등록된 태그가 아닙니다({cmd.TAG_NAME}  - {cmd.ASSET_CODE})");
                }
                RTTagDataVO data = new RTTagDataVO();
                data.AssetCode = cmd.ASSET_CODE;
                data.TagStCode = ST_TAG_DATA.TAG_ST_CODE;
                data.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                data.Value = cmd.VALUE.ToString();




                TagValueDataSvc.TagValueDataSvcClient client = new TagValueDataSvc.TagValueDataSvcClient(DA_CHANNEL);
                Result Result= client.InsertTagValueData(data);
                return Result;
            }
            catch (Exception ex) 
            {
                return new Result() { IsSucc = false,Msg=ex.Message };
            }
        }

        [HttpGet]
        public IList<ASSET_RELATION_MODEL> Get()
        {
            return MemoryDataAdapter.getInstance().ASSET_RELATION_List;
        }



        public void Dispose()
        {
           this.DA_CHANNEL.Dispose();
        }
    }
}


