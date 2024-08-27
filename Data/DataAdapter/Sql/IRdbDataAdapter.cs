
namespace IX.DS.DA.Common.DataAdapter.Sqlite
{
    public interface IRdbDataAdapter : IDisposable
    {
        bool IsDisposed { get; set; }

        T? ReadData<T>(string strQuery);
        T? ReadData<T>(string strQuery, object param);
        IEnumerable<T?> ReadDataList<T>(string strQuery);
        IEnumerable<T?> ReadDataList<T>(string strQuery, object param);
        void SetData(string strQuery, object data);
        void SetData(string strQuery);
    }
}