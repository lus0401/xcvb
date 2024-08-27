using Grpc.Core;
using IX.DS.DA.Common.Models;
using IX.DS.FW_Mockup.Data;
using IX.DS.FW_Mockup.Data.Models;
using IX.MW.DA.CLIENT.ASSETDATA.COMMON;
using IX.MW.DA.CLIENT.COMMON;
using Newtonsoft.Json;
using System.Diagnostics;

namespace IX.MW.DA.CLIENT.CONNECTION.RECEIVE 
{
    public class Connection_RcvGrpcService :ConnectionRcvSvc.ConnectionRcvSvcBase
    {
        private readonly ILogger<Connection_RcvGrpcService> _logger;

        public MemoryDataAdapter dataAdapter { get; private set; }

        public Connection_RcvGrpcService(ILogger<Connection_RcvGrpcService> logger)
        {
            _logger = logger;
            this.dataAdapter = MemoryDataAdapter.getInstance();
        }


        public override Task<Result> AssetConnStatus_Changed(CONNECTION_STATUS request, ServerCallContext context)
        {
            Debug.WriteLine($"AssetConnStatus_Changed - {request.AssetCode } : {request.CONNECTIONSTATUS}");
            return Task.FromResult(new Result() { AssetCode = request.AssetCode, IsSucc = true, Msg = "" }); ;
        }

    }
}
