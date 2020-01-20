using AzureTableLogger.Extensions;
using AzureTableLogger.Models;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// returns the 50 most recent exceptions that meet the search criteria
        /// </summary>
        public async Task<IEnumerable<ExceptionEntity>> QueryAsync(Func<ExceptionEntity, bool> filter = null)
        {                       
            var table = await InitTableAsync();
            var query = table.CreateQuery<ExceptionEntity>();
            query.FilterString = TableQuery.GenerateFilterCondition(nameof(ExceptionEntity.PartitionKey), QueryComparisons.Equal, AppName);            

            var results = (await query.ExecuteAsync(filter, 50)).OrderByDescending(item => item.Timestamp);
            return results;
        }

        public async Task PurgeAfterAsync(TimeSpan timeSpan)
        {
            var table = await InitTableAsync();

            throw new NotImplementedException();
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
