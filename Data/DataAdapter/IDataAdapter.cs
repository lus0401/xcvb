namespace IX.MW.DA.Svc.Common.DataAdapter
{
    public interface IDataAdapter<T>
    {
        void WriteData(T data);
        T ReadData(dynamic data);

        IList<T> ReadDataList(dynamic data);
    }
}
