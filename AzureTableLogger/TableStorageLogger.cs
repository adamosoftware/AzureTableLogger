using AzureTableLogger.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AzureTableLogger
{
    public class TableStorageLogger
    {
        private readonly StorageCredentials _credentials;
        private readonly string _tableName;
        private readonly string _appName;

        public TableStorageLogger(string accountName, string accountKey, string tableName, string appName)
        {
            _credentials = new StorageCredentials(accountName, accountKey);
            _tableName = tableName;
            _appName = appName;
        }

        public async Task WriteAsync(Exception exception, string machineName = null, string userName = null, [CallerMemberName]string methodName = null)
        {
            var log = new ExceptionEntity(_appName, exception)
            {
                MachineName = machineName,
                UserName = userName,
                MethodName = methodName
            };

            var account = new CloudStorageAccount(_credentials, true);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(_tableName);
            var operation = TableOperation.Insert(log);
            await table.ExecuteAsync(operation);
        }
    }
}
