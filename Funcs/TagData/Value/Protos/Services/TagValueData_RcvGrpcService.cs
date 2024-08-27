using Grpc.Core;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.Data.Models;
using IX.MW.DA.CLIENT.COMMON;
using IX.MW.DA.CLIENT.Funcs.TagData.Vlaues.Receive;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace IX.MW.DA.CLIENT.TAG.VALUE.RECEIVE
{
    public class TagValueData_RcvGrpcService :TagValueDataRcvSvc.TagValueDataRcvSvcBase
    {
        private readonly ILogger<TagValueData_RcvGrpcService> _logger;

        public MemoryDataAdapter dataAdapter { get; private set; }
        private TagValueData_RcvGrpcService _reposit { get; set; }

        public TagValueData_RcvGrpcService(ILogger<TagValueData_RcvGrpcService> logger)
        {
            _logger = logger;
            this.dataAdapter = MemoryDataAdapter.getInstance();
        }



        public override Task<Result> TagValue_Changed(RTTagDataVO request, ServerCallContext context)
        {
            if (!dataAdapter.TAG_REAL_DATA_DIC.Keys.Contains(request.AssetCode))
            {
                dataAdapter.TAG_REAL_DATA_DIC.Add(request.AssetCode, new Dictionary<UInt64, RT_TAG_DATA_MODEL>());
            }
            if (!dataAdapter.TAG_REAL_DATA_DIC[request.AssetCode].ContainsKey(request.TagStCode))
            {
                dataAdapter.TAG_REAL_DATA_DIC[request.AssetCode].Add(request.TagStCode, null);
            }
            dataAdapter.TAG_REAL_DATA_DIC[request.AssetCode][request.TagStCode] = new RT_TAG_DATA_MODEL() {ASSET_CODE=request.AssetCode,REAL_TIME_DATA=request.Value,TAG_ST_CODE=request.TagStCode };
            Debug.WriteLine($"[Mockup.TagValue_Changed] 데이터: {JsonConvert.SerializeObject(request)}");
            return Task.FromResult(new Result() {AssetCode=request.AssetCode,IsSucc=true,Msg="" }); ;
        }

        public override Task<IX.MW.DA.CLIENT.COMMON.Results> InsertTagValueDataArray(RTTagDataVOs request, ServerCallContext context)
        {
            _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}  [Mockup.InsertTagValueDataArray] 데이터: {JsonConvert.SerializeObject(request)}");
            return Task.FromResult(new IX.MW.DA.CLIENT.COMMON.Results()); ;
        }



    }
}
