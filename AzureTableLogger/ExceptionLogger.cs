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

        public async Task<ExceptionEntity> WriteAsync(
            Exception exception, string machineName = null, string userName = null,
            [CallerMemberName]string methodName = null, [CallerFilePath]string sourceFile = null, [CallerLineNumber]int lineNumber = 0)
        {
            var log = new ExceptionEntity(AppName, exception)
            {
                MachineName = machineName,
                UserName = userName,
                MethodName = methodName,
                SourceFile = sourceFile,
                LineNumber = lineNumber
            };

            await WriteLogAsync(log);

            return log;
        }

        public async Task TryCatchAsync(Func<Task> action, Action<Exception> displayError,
            string machineName = null, string userName = null,
            [CallerMemberName]string methodName = null, [CallerFilePath]string sourceFile = null, [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                await action.Invoke();
            }
            catch (Exception exc)
            {
                await WriteAsync(exc, machineName, userName, methodName, sourceFile, lineNumber);
                displayError.Invoke(exc);
            }
        }

        public async Task<ExceptionEntity> GetAsync(string exceptionId)
        {
            var table = await InitTableAsync();
            var operation = TableOperation.Retrieve<ExceptionEntity>(AppName, exceptionId);
            var result = await table.ExecuteAsync(operation);
            return result.Result as ExceptionEntity;
        }

        /// <summary>
        /// returns by default the 100 most recent exceptions that meet the search criteria
        /// </summary>
        public async Task<IEnumerable<ExceptionEntity>> QueryAsync(Func<ExceptionEntity, bool> filter = null, int maxResults = 100)
        {
            var table = await InitTableAsync();
            var query = GetQuery().OrderByDesc(nameof(ExceptionEntity.Timestamp));

            List<ExceptionEntity> results = new List<ExceptionEntity>();

            var token = default(TableContinuationToken);
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                var returnResults = (filter != null) ? segment.Results.Where(filter) : segment.Results;
                results.AddRange(returnResults);
                token = segment.ContinuationToken;
            } while (token != null && results.Count <= maxResults);

            return results.Take(maxResults);
        }

        public async Task<IEnumerable<ExceptionEntity>> QueryAsync(string query, int maxResults = 100)
        {
            if (string.IsNullOrEmpty(query))
            {
                return await QueryAsync(maxResults: maxResults);
            }

            string[] words = query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower()).ToArray();

            Func<ExceptionEntity, bool> filter = (ent) =>
            {
                string searchText = string.Join("\r\n", new string[]
                {
                    ent.FullMessage, ent.UserName, ent.MethodName, ent.ExceptionType, ent.RowKey
                }).ToLower();

                return words.All(word => searchText.Contains(word));
            };

            return await QueryAsync(filter, maxResults);
        }

        private TableQuery<ExceptionEntity> GetQuery()
        {
            return new TableQuery<ExceptionEntity>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ExceptionEntity.PartitionKey), QueryComparisons.Equal, AppName));
        }

        public async Task PurgeAfterAsync(TimeSpan timeSpan)
        {
            var table = await InitTableAsync();
            var query = GetQuery().OrderBy(nameof(ExceptionEntity.Timestamp));

            var continuation = default(TableContinuationToken);
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuation);

                var delete = new TableBatchOperation();
                foreach (var entity in segment.Results)
                {
                    var age = DateTime.UtcNow.Subtract(entity.Timestamp.UtcDateTime);
                    if (age > timeSpan)
                    {
                        delete.Add(TableOperation.Delete(entity));
                    }
                }

                if (delete.Any())
                {
                    await table.ExecuteBatchAsync(delete);
                }

                continuation = segment.ContinuationToken;
            } while (continuation != null);
        }

        private async Task<CloudTable> InitTableAsync()
        {
            var table = GetTable();
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public CloudTable GetTable()
        {
            var account = GetStorageAccount();
            var client = account.CreateCloudTableClient();
            return client.GetTableReference(_tableName);
        }

        public CloudStorageAccount GetStorageAccount()
        {
            return new CloudStorageAccount(_credentials, true);
        }
    }
}
