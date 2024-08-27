using Grpc.Net.Client;
using IX.DS.DA.Common.Models.TagData;
using IX.DS.FW_Mockup.Data.Models;
using IX.DS.FW_Mockup.Data.Models.FILE;
using IX.DS.FW_Mockup.file.Data;
using IX.MW.DA.CLIENT.CONNECTION.RECEIVE;
using IX.MW.DA.CLIENT.TAG;
using IX.MW.DA.CLIENT.TAG.VALUE;
using Microsoft.AspNetCore.Server.IISIntegration;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using IX.DS.FW_Mockup.Data.Models;
using IX.DS.DA.Common.DataAdapter.Sqlite;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace IX.DS.FW_Mockup.Data
{
    class FileData
    {
        public string FILE_ID { get; set; }
        public UInt32 ASSET_CODE { get; set; }
    }

    class ConnectionMasterData
    {
        public string CONNECTION_TYPE { get; set; }
        public string PROTOCOL_TYPE { get; set; }
        public UInt32 ASSET_CODE { get; set; }
    }

    class ConnectionData
    {
        public string IP { get; set; }
        public int PORT { get; set; }
        public UInt32 ASSET_CODE { get; set; }
    }


    public class MemoryDataAdapter :IDisposable
    {
        uint[] TagAssetCodes = new uint[3] {3,4,5 };


        private bool isDisposed { get; set; } = false;
        private BackgroundWorker AssetCoreDataSender { get; set; }
        private GrpcChannel DA_Channel { get; set; }
        private  IConfiguration Conf { get; set; }
        private static MemoryDataAdapter instance { get; set; } = new MemoryDataAdapter();
        private MemoryDataAdapter()
        {
            using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
            {
                try
                {
                    string strQuery = $@"
SELECT FILE_ID, ASSET_CODE
FROM FILE_ASSETCODE_MAP";
                    IEnumerable<FileData?> FileData= DataAdapter.ReadDataList<FileData>(strQuery);
                    foreach (FileData? _FileData in FileData)
                    {
                        if (_FileData==null)
                        {
                            continue;
                        }
                        Assetcode_file_dic.Add(_FileData.FILE_ID, _FileData.ASSET_CODE);
                        strQuery = $@"
SELECT ASSET_CODE, CONNECTION_TYPE, PROTOCOL_TYPE
FROM CONNECTION_MASTER
where ASSET_CODE={_FileData.ASSET_CODE};";
                        CONNECTION_MASTER_MODEL? masterData = DataAdapter.ReadData<CONNECTION_MASTER_MODEL>(strQuery);
                        if (masterData!=null)
                        {
                            CONNECTION_MASTER_DIC.Add(_FileData.ASSET_CODE, masterData); 
                        }
                        strQuery = $@"
SELECT ASSET_CODE, IP, PORT
FROM CONNECTION_DATA_TCP
where ASSET_CODE={_FileData.ASSET_CODE};";
                        CONNECTIONDATA_TCP? ConnectionData = DataAdapter.ReadData<CONNECTIONDATA_TCP>(strQuery);
                        if (ConnectionData!=null)
                        {
                            CONNECTION_DATA_DIC.Add(_FileData.ASSET_CODE, ConnectionData); 
                        }

                        strQuery = $@"
SELECT ASSET_CODE, AUTHMODE, USERID, USERPW, SECURITY_MODE, SECURITY_POLICY, AuthFilePath, AuthFilePWD, URL
FROM PROTOCOL_DATA_OPCUA
where ASSET_CODE={_FileData.ASSET_CODE};";
                        PROTOCOLDATA_MODEL? PROTOCOLDATA_MODEL = DataAdapter.ReadData<PROTOCOLDATA_MODEL>(strQuery);
                        if (PROTOCOLDATA_MODEL!=null)
                        {
                            PROTOCOL_DATA_DIC.Add(_FileData.ASSET_CODE, PROTOCOLDATA_MODEL); 
                        }

                        TAG_STANDARD_DATA_DIC.Add(_FileData.ASSET_CODE, new Dictionary<UInt64, ST_TAG_DATA_MODEL>());

                        strQuery = $@"
SELECT ASSET_CODE, TAG_ST_CODE, IS_READ_ONLY, USE_CONTROL, NODE_ID, TAG_NAME, DATA_TYPE, DESCRIPTION, CYCLE_TIME
FROM ST_TAG_DATA_OPCUA
where ASSET_CODE={_FileData.ASSET_CODE};";
                        IEnumerable<ST_TAG_DATA_MODEL?> ST_TAGs = DataAdapter.ReadDataList<ST_TAG_DATA_MODEL>(strQuery);
                        foreach (ST_TAG_DATA_MODEL? ST_TAG_DATA_MODEL in ST_TAGs)
                        {
                            if (ST_TAG_DATA_MODEL!=null)
                            {
                                TAG_STANDARD_DATA_DIC[_FileData.ASSET_CODE].Add(ST_TAG_DATA_MODEL.TAG_ST_CODE, ST_TAG_DATA_MODEL); 
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                }
            }

        }

        private void Init()
        {
            string? AAS_URL = Conf.GetSection("AAS_URL").Value;
            string? DA_URL = Conf.GetSection("DA_URL").Value;
            if (AAS_URL!=null)
            {
                AssetCoreClient = new HttpClient();
                AssetCoreClient.BaseAddress = new Uri(AAS_URL);
                AssetCoreClient.DefaultRequestHeaders.Accept.Clear();
                AssetCoreClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                DA_Channel = GrpcChannel.ForAddress(DA_URL);

                AssetCoreDataSender = new BackgroundWorker();
                AssetCoreDataSender.DoWork += AssetCoreDataSender_DoWork;
                AssetCoreDataSender.RunWorkerCompleted += AssetCoreDataSender_RunWorkerCompleted;
                AssetCoreDataSender.RunWorkerAsync(); 
            }
        }

        public static MemoryDataAdapter getInstance(IConfiguration conf)
        {
            if (instance.Conf==null)
            {
                instance.Conf = conf;
                instance.Init();
            }
            return instance;
        }

        public static MemoryDataAdapter getInstance()
        {
            return instance;
        }

        #region Send data to Asset Core
        public int MyProperty { get; set; }
        private HttpClient AssetCoreClient { get; set; }
        private void AssetCoreDataSender_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (!isDisposed)
            {
                Task.Delay(100).Wait();
                AssetCoreDataSender.RunWorkerAsync();
            }
        }

        private static object LockObj = new object();

        private async void AssetCoreDataSender_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (!isDisposed)
            {
                int cnt = 0;
                
                // Asset Core에 요청
                for (int i = 0; i < TAG_STANDARD_DATA_DIC.Count; i++)
                {

                    uint AssetCode = TAG_STANDARD_DATA_DIC.Keys.ElementAt(i);
                    if (!TagAssetCodes.Contains(AssetCode))
                    {
                        continue;
                    }
                    if (!TAG_STANDARD_DATA_DIC.ContainsKey(AssetCode))
                    {
                        continue;
                    }
                    try
                    {
                            string strTagData = "";

                            cnt = 0;

                        TagValueDataSvc.TagValueDataSvcClient Client = new TagValueDataSvc.TagValueDataSvcClient(this.DA_Channel);
                        RTTagDataVOs TagValueDatas = Client.GetTagValueDataArray(new MW.DA.CLIENT.COMMON.Asset_Code() { AssetCode = AssetCode });

                        lock (LockObj)
                        {
                            foreach (RTTagDataVO tag in TagValueDatas.RTTagDataVO)
                            {
                                if (strTagData != "")
                                {
                                    strTagData += ",";
                                }
                                UInt64 Tag_ST_CODE = tag.TagStCode;
                                string strJsonTagData = tag.Value;
                                REAL_TIME_DATA_OPCUA TagData = JsonConvert.DeserializeObject<REAL_TIME_DATA_OPCUA>(strJsonTagData);
                                string Name = TAG_STANDARD_DATA_DIC[AssetCode][Tag_ST_CODE].TAG_NAME.ToLower();
                                string tagType = TAG_STANDARD_DATA_DIC[AssetCode][Tag_ST_CODE].DATA_TYPE.ToLower();
                                if (TagData.Value==null)
                                {
                                    if (tagType.ToLower()=="string")
                                    {
                                        TagData.Value = "0";
                                    }
                                }

                                dynamic Value = (TagData.Value != null && tagType == "string") ? $"\"{TagData.Value}\"" : TagData.Value;
 



                                strTagData += $"\"{Name}\":{Value}";
                            } 
                        }
                        try
                        {
                            string strJson = $"{{\"asset_code\":{AssetCode}" + (!string.IsNullOrEmpty(strTagData) ? "," : "") + strTagData.Replace("\r\n","") + "}";
                            object data = JsonConvert.DeserializeObject<dynamic>(strJson);
                            HttpResponseMessage response =  AssetCoreClient.PostAsJsonAsync("DTdata", data).Result;
                            response.EnsureSuccessStatusCode();
                            Debug.WriteLine(DateTime.Now + $"AssetCode:{AssetCode} --  CNT:{TagValueDatas.RTTagDataVO.Count}" + "----" + strTagData);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);

                        }
                    }
                    catch (Exception)
                    {


                    }                    
                }                                              
            }
        } 
        #endregion





        public UInt32 ASSET_CODE_MAX { get; set; } = 0x00000000;

        public IDictionary<string, UInt32> Assetcode_file_dic { get; set; } = new Dictionary<string, UInt32>();

        public IDictionary<UInt32, CONNECTION_MASTER_MODEL> CONNECTION_MASTER_DIC { get; set; } = new Dictionary<UInt32, CONNECTION_MASTER_MODEL>();
        public IDictionary<UInt32, CONNECTIONDATA_TCP> CONNECTION_DATA_DIC { get; set; } = new Dictionary<UInt32, CONNECTIONDATA_TCP>();
        public IDictionary<UInt32, PROTOCOLDATA_MODEL> PROTOCOL_DATA_DIC { get; set; } = new Dictionary<UInt32, PROTOCOLDATA_MODEL>();

        public IDictionary<UInt32, IDictionary<UInt64, ST_TAG_DATA_MODEL>> TAG_STANDARD_DATA_DIC { get; set; } = new Dictionary<UInt32, IDictionary<UInt64, ST_TAG_DATA_MODEL>>();

        public IDictionary<UInt32, IDictionary<UInt64, RT_TAG_DATA_MODEL>> TAG_REAL_DATA_DIC { get; set; } = new Dictionary<UInt32, IDictionary<UInt64, RT_TAG_DATA_MODEL>>();

        public IList<ASSET_RELATION_MODEL> ASSET_RELATION_List { get; set; } = new List<ASSET_RELATION_MODEL>();





        public UInt32 RegistConnectionMaster(string FileID, CONNECTION_MASTER_FILE_MODEL Master)
        {
            UInt32 AssetCode = 0;
            if (Assetcode_file_dic.Keys.Contains(FileID))
            {
                AssetCode = Assetcode_file_dic[FileID];
                if (!TagAssetCodes.Contains(AssetCode))
                {
                    using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
                    {
                        try
                        {
                            string strQuery = $@"
INSERT OR REPLACE INTO FILE_ASSETCODE_MAP (FILE_ID, ASSET_CODE)
VALUES ('{FileID}', {AssetCode})";
                            DataAdapter.SetData(strQuery);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    return AssetCode;
                }
                CONNECTION_MASTER_DIC[AssetCode] = new CONNECTION_MASTER_MODEL() { ASSET_CODE = AssetCode, CONNECTION_TYPE = Master.CONNECTION_TYPE, PROTOCOL_TYPE = Master.PROTOCOL_TYPE };
            }
            else
            {
                ASSET_CODE_MAX = ASSET_CODE_MAX + 0x00000001;
                AssetCode = ASSET_CODE_MAX;
                Assetcode_file_dic.Add(FileID, AssetCode);
                if (!TagAssetCodes.Contains(AssetCode))
                {
                    using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
                    {
                        try
                        {
                           string strQuery = $@"
INSERT OR REPLACE INTO FILE_ASSETCODE_MAP (FILE_ID, ASSET_CODE)
VALUES ('{FileID}', {AssetCode})";
                            DataAdapter.SetData(strQuery);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    return AssetCode;
                }
                CONNECTION_MASTER_DIC.Add(AssetCode, new CONNECTION_MASTER_MODEL() { ASSET_CODE = AssetCode, CONNECTION_TYPE = Master.CONNECTION_TYPE, PROTOCOL_TYPE = Master.PROTOCOL_TYPE });
            }
            using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
            {
                try
                {
                    string strQuery = $@"
INSERT OR REPLACE INTO Connection_Master (ASSET_CODE, CONNECTION_TYPE, PROTOCOL_TYPE)
VALUES ({CONNECTION_MASTER_DIC[AssetCode].ASSET_CODE}, '{CONNECTION_MASTER_DIC[AssetCode].CONNECTION_TYPE}', '{CONNECTION_MASTER_DIC[AssetCode].PROTOCOL_TYPE}')";
                    DataAdapter.SetData(strQuery);

                    strQuery = $@"
INSERT OR REPLACE INTO FILE_ASSETCODE_MAP (FILE_ID, ASSET_CODE)
VALUES ('{FileID}', {AssetCode})";
                    DataAdapter.SetData(strQuery);
                }
                catch (Exception ex)
                {
                }
            }



            return AssetCode;
        }


        public UInt32 RegistConnectionData(string FileID, CONNECTIONDATA_FILE_TCP_MODEL data)
        {
            UInt32 AssetCode = 0x00000000;
            if (Assetcode_file_dic.Keys.Contains(FileID))
            {
                AssetCode = Assetcode_file_dic[FileID];
            }
            else
            {
                return AssetCode;
            }
            if (!TagAssetCodes.Contains(AssetCode))
            {
                return AssetCode;
            }
            CONNECTION_DATA_DIC[AssetCode] = new CONNECTIONDATA_TCP() { ASSET_CODE = AssetCode, IP = data.IP, PORT = data.PORT };

            using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
            {
                try
                {
                    string strQuery = $@"
INSERT OR REPLACE INTO CONNECTION_DATA_TCP (ASSET_CODE, IP, PORT)
VALUES ({CONNECTION_DATA_DIC[AssetCode].ASSET_CODE}, '{CONNECTION_DATA_DIC[AssetCode].IP}', '{CONNECTION_DATA_DIC[AssetCode].PORT}')";
                    DataAdapter.SetData(strQuery);
                }
                catch (Exception ex)
                {
                }
            }

            return AssetCode;
        }


        public UInt32 RegistProtocolData(string FileID, PROTOCOLDATA_FILE_Model data)
        {
            UInt32 AssetCode = 0x00000000;
            if (Assetcode_file_dic.Keys.Contains(FileID))
            {
                AssetCode = Assetcode_file_dic[FileID];
                if (!TagAssetCodes.Contains(AssetCode))
                {
                    return AssetCode;
                }
                PROTOCOL_DATA_DIC[AssetCode] = new PROTOCOLDATA_MODEL()
                {
                    ASSET_CODE = AssetCode
                    ,
                    AuthFilePath = data.AuthFilePath
                    ,
                    AuthFilePWD = data.AuthFilePWD
                    ,
                    AUTHMODE = data.AUTHMODE
                    ,
                    SECURITY_MODE = data.SECURITY_MODE
                    ,
                    SECURITY_POLICY = data.SECURITY_POLICY
                    ,
                    URL = data.URL
                    ,
                    USERID = data.USERID
                    ,
                    USERPW = data.USERPW
                };

                using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
                {
                    try
                    {
                        string strQuery = $@"
INSERT OR REPLACE INTO PROTOCOL_DATA_OPCUA (ASSET_CODE, AUTHMODE, USERID, USERPW, SECURITY_MODE, SECURITY_POLICY, AuthFilePath, AuthFilePWD, URL)
VALUES ({PROTOCOL_DATA_DIC[AssetCode].ASSET_CODE},'{PROTOCOL_DATA_DIC[AssetCode].AUTHMODE}', '{PROTOCOL_DATA_DIC[AssetCode].USERID}', '{PROTOCOL_DATA_DIC[AssetCode].USERPW}', '{PROTOCOL_DATA_DIC[AssetCode].SECURITY_MODE}', '{PROTOCOL_DATA_DIC[AssetCode].SECURITY_POLICY}', '{PROTOCOL_DATA_DIC[AssetCode].AuthFilePath}', '{PROTOCOL_DATA_DIC[AssetCode].AuthFilePWD}', '{PROTOCOL_DATA_DIC[AssetCode].URL}')";
                        DataAdapter.SetData(strQuery);
                    }
                    catch (Exception ex)
                    {
                    }
                }

            }
            return AssetCode;
        }

        public UInt64[] RegistTagStandard(string FileID, TagStandardFileDataModel[] TagStandards)
        {
            if (Assetcode_file_dic.ContainsKey(FileID))
            {
                UInt32 AssetCode = Assetcode_file_dic[FileID];
                if (!TagAssetCodes.Contains(AssetCode))
                {
                    return new UInt64 [0];
                }
                if (TAG_STANDARD_DATA_DIC.ContainsKey(AssetCode))
                {
                    TAG_STANDARD_DATA_DIC.Remove(AssetCode);
                }
                TAG_STANDARD_DATA_DIC.Add(AssetCode, new Dictionary<UInt64, ST_TAG_DATA_MODEL>());
                UInt64 TagStCode = (AssetCode * ((UInt64)0x100000000));
                for (int i = 0; i < TagStandards.Length; i++)
                {
                    TagStCode += 0x00000001;
                    ST_TAG_DATA_MODEL tag_st_model = new ST_TAG_DATA_MODEL()
                    {
                        TAG_ST_CODE = TagStCode,
                        CYCLE_TIME = TagStandards[i].CYCLE_TIME,
                        DATA_TYPE = TagStandards[i].DATA_TYPE,
                        DESCRIPTION = TagStandards[i].DESCRIPTION,
                        IS_READ_ONLY = TagStandards[i].IS_READ_ONLY,
                        NODE_ID = TagStandards[i].NODE_ID,
                        TAG_NAME = TagStandards[i].TAG_NAME,
                        USE_CONTROL = TagStandards[i].USE_CONTROL
                    };
                    TAG_STANDARD_DATA_DIC[AssetCode].Add(TagStCode, tag_st_model);
                    using (IRdbDataAdapter DataAdapter = new SqliteDataAdapter())
                    {
                        try
                        {
                            string strQuery = $@"
INSERT OR REPLACE INTO ST_TAG_DATA_OPCUA (ASSET_CODE, TAG_ST_CODE, IS_READ_ONLY, USE_CONTROL, NODE_ID, TAG_NAME, DATA_TYPE, CYCLE_TIME, DESCRIPTION)
VALUES ({AssetCode}, {TagStCode}, '{TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].IS_READ_ONLY}', '{TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].USE_CONTROL}', '{TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].NODE_ID}', '{TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].TAG_NAME}', '{TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].DATA_TYPE}', {TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].CYCLE_TIME}, '{TAG_STANDARD_DATA_DIC[AssetCode][TagStCode].DESCRIPTION}')";
                            DataAdapter.SetData(strQuery);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }



                return TAG_STANDARD_DATA_DIC[AssetCode].Keys.ToArray();
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            this.AssetCoreDataSender.Dispose();
            DA_Channel.Dispose();
        }

        public IDictionary<UInt32, CONNECTION_STATUS> CONNECTION_STATUS_DIC { get; set; } = new Dictionary<UInt32, CONNECTION_STATUS>();
    }


}
