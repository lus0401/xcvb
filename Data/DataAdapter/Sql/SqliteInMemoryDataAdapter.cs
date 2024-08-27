using Dapper;
using IX.MW.DA.Svc.Common.DataAdapter;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Data.SQLite;
namespace IX.DS.DA.Common.DataAdapter.Sqlite
{
    public class SqliteInMemoryDataAdapter : IRdbDataAdapter
    {

        public Mutex mutex { get; set; } = new Mutex();
        private SQLiteConnection  DataWriter { get; set; }

        private string ConnectionString { get { return $"Data Source=InMemorySample;Mode=Memory;Cache=Shared"; } }


        public bool IsDisposed { get; set; } = false;
        [Inject]
        public ILogger _logger { get; set; }
        public SqliteInMemoryDataAdapter()
        {
            if (this.DataWriter == null)
            {
                this.DataWriter = new SQLiteConnection(this.ConnectionString);
                this.DataWriter.Open();
            }
        }


        public T? ReadData<T>(string strQuery)
        {
            T? data = default(T);
            if (!IsDisposed)
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString ))
                {
                    data = conn.QueryFirstOrDefault<T>(strQuery);
                }
            }
            return data;
        }


        public T? ReadData<T>(string strQuery, object param)
        {
            T? data = default(T);
            if (!IsDisposed)
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    data = conn.QueryFirstOrDefault<T>(strQuery, param);
                }
            }
            return data;
        }



        public IEnumerable<T?> ReadDataList<T>(string strQuery)
        {
            IEnumerable<T?> data = new List<T?>();
            if (!IsDisposed)
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    data = conn.Query<T?>(strQuery);
                }
            }
            return data;
        }
        public IEnumerable<T?> ReadDataList<T>(string strQuery, object param)
        {
            IEnumerable<T?> data = new List<T?>();
            if (!IsDisposed)
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    data = conn.Query<T?>(strQuery, param);
                }
            }
            return data;
        }

        public void SetData(string strQuery, object data)
        {
            if (!IsDisposed)
            {
                if (mutex.WaitOne(10000))
                {
                    try
                    {
                        this.DataWriter.Execute(strQuery, data);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    throw new Exception("time_out");
                }
                mutex.ReleaseMutex();    
                
            }
        }

        public void SetData(string strQuery)
        {
            if (!IsDisposed)
            {
                if (mutex.WaitOne(4000))
                {
                    try
                    {
                        this.DataWriter.Execute(strQuery);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {
                    throw new Exception("time_out");
                }
                mutex.ReleaseMutex();

            }
        }


        public void Dispose()
        {
            this.IsDisposed = true;
            if (this.DataWriter.State == System.Data.ConnectionState.Open)
            {
                this.DataWriter.Dispose();
            }
            if (this.mutex != null)
            {
                this.mutex.Dispose();
            }
        }
    }
}
