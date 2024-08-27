namespace IX.DS.DA.Common.DataAdapter.File
{
    public interface IFileDataAdapter
    {
        string FilePath { get; set; }

        T ReadData<T>(string FilePath);
        IList<T> ReadDataList<T>(string DirPath, string searchPattern);
        void WriteData<T>(string FilePath, T data);
    }
}