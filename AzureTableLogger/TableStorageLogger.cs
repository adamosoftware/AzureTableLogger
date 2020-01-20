using AzureTableLogger.Models;
using Microsoft.AspNetCore.Mvc.Filters;
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

        public async Task<ExceptionEntity> WriteAsync(Exception exception, string machineName = null, string userName = null, [CallerMemberName]string methodName = null)
        {
            var log = new ExceptionEntity(_appName, exception)
            {
                MachineName = machineName,
                UserName = userName,
                MethodName = methodName
            };

            await AddLogAsync(log);

            return log;
        }

        public async Task<ExceptionEntity> WriteAsync(ExceptionContext context)
        {            
            var log = new ExceptionEntity(_appName, context.Exception)
            {                
                UserName = context.HttpContext.User.Identity.Name,
                MethodName = context.HttpContext.Request.Path.Value,
                QueryString = context.HttpContext.Request.QueryString.Value,
                HttpMethod = context.HttpContext.Request.Method
            };

            await AddLogAsync(log);

            return log;
        }

        private async Task AddLogAsync(ExceptionEntity log)
        {
            CloudTable table = await InitTableAsync();
            var operation = TableOperation.Insert(log);
            await table.ExecuteAsync(operation);
        }

        private async Task<CloudTable> InitTableAsync()
        {
            var account = new CloudStorageAccount(_credentials, true);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(_tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<ExceptionEntity> GetAsync(string rowKey)
        {
            var table = await InitTableAsync();
            var operation = TableOperation.Retrieve<ExceptionEntity>(_appName, rowKey);
            var result = await table.ExecuteAsync(operation);
            return result.Result as ExceptionEntity;
        }
    }
}
