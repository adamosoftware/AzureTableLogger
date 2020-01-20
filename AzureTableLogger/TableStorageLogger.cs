using AzureTableLogger.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AzureTableLogger
{
    public class TableStorageLogger
    {
        private readonly StorageCredentials _credentials;
        private readonly string _tableName;        

        public TableStorageLogger(string accountName, string accountKey, string tableName, string appName)
        {
            _credentials = new StorageCredentials(accountName, accountKey);
            _tableName = tableName;
            AppName = appName;
        }

        public string AppName { get; }

        public async Task WriteLogAsync(ExceptionEntity log)
        {
            CloudTable table = await InitTableAsync();
            var operation = TableOperation.Insert(log);
            await table.ExecuteAsync(operation);
        }

        public async Task<ExceptionEntity> WriteAsync(Exception exception, string machineName = null, string userName = null, [CallerMemberName]string methodName = null)
        {
            var log = new ExceptionEntity(AppName, exception)
            {
                MachineName = machineName,
                UserName = userName,
                MethodName = methodName
            };

            if (exception.Data.Count > 0)
            {
                log.CustomData = new Dictionary<string, string>();
                foreach (string key in exception.Data.Keys)
                {
                    log.CustomData.Add(key, exception.Data[key]?.ToString());
                }
            }

            await WriteLogAsync(log);

            return log;
        }

        public async Task<ExceptionEntity> GetAsync(string rowKey)
        {
            var table = await InitTableAsync();
            var operation = TableOperation.Retrieve<ExceptionEntity>(AppName, rowKey);
            var result = await table.ExecuteAsync(operation);
            return result.Result as ExceptionEntity;
        }

        private async Task<CloudTable> InitTableAsync()
        {
            var account = new CloudStorageAccount(_credentials, true);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
