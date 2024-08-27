using Grpc.Core;
using Grpc.Net.Client;
using IX.DS.FW_Mockup.Data;
using IX.MW.DA.CLIENT.CONNECTION;
using IX.MW.DA.CLIENT.COMMON;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace IX.DS.FW_Mockup.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectAssetController : ControllerBase,IDisposable
    {

        private readonly ILogger<ConnectAssetController> _logger;
        public GrpcChannel DA_CHANNEL { get; set; }
        private bool _disposed = false;

        public IConfiguration Conf { get; set; }

        public ConnectAssetController(ILogger<ConnectAssetController> logger, IConfiguration Conf)
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
        }

        [HttpGet(Name = "ConnAssetAsync")]
        public Result Get(UInt32 asset_code)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ConnectAssetController));
            }
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://127.0.0.1:5002"))
            //using (GrpcChannel channel = GrpcChannel.ForAddress("http://0.tcp.jp.ngrok.io:17594"))
            
            ConnectionSvc.ConnectionSvcClient client = new ConnectionSvc.ConnectionSvcClient(this.DA_CHANNEL);
            return client.ConnAssetAsync(new Asset_Code() { AssetCode = asset_code });
        }

        public void Dispose()
        {
            // this.DA_CHANNEL.Dispose();  
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    DA_CHANNEL?.Dispose();
                }

                // Dispose unmanaged resources (if any)

                _disposed = true;
            }
        }

        ~ConnectAssetController()
        {
            Dispose(false);
        }
    }
}
