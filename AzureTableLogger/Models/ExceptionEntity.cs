using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureTableLogger.Models
{
    public class ExceptionEntity : TableEntity
    {
        public ExceptionEntity(string appName, Exception exception)
        {
            PartitionKey = appName;
            RowKey = Guid.NewGuid().ToString();
            Message = exception.Message;
            StackTrace = exception.StackTrace;
        }

        public string MachineName { get; set; }
        public string UserName { get; set; }
        public string MethodName { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
