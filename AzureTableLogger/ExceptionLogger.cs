using AzureTableLogger.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AzureTableLogger
{
    public class ExceptionLogger
    {
        private readonly StorageCredentials _credentials;
        private readonly string _tableName;        

        public ExceptionLogger(string accountName, string accountKey, string appName, string tableName = "Exceptions")
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

            await WriteLogAsync(log);

            return log;
        }

        public async Task<ExceptionEntity> GetAsync(string exceptionId)
        {
            var table = await InitTableAsync();
            var operation = TableOperation.Retrieve<ExceptionEntity>(AppName, exceptionId);
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
