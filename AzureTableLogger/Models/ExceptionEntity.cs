using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace AzureTableLogger.Models
{
    public class ExceptionEntity : TableEntity
    {
        public ExceptionEntity()
        {
        }

        public ExceptionEntity(string appName, Exception exception)
        {
            string getFullMessage(Exception nested, int depth = 0)
            {
                string result = nested.Message;

                if (nested.InnerException != null)
                {
                    depth++;
                    result += "\r\n" + new string('-', depth) + " " + getFullMessage(nested.InnerException, depth);
                }

                return result;
            }

            PartitionKey = appName;
            RowKey = Guid.NewGuid().ToString();
            Message = exception.Message;
            FullMessage = getFullMessage(exception);
            StackTrace = exception.StackTrace;
            ExceptionType = exception.GetType().Name;

            if (exception.Data.Count > 0)
            {
                CustomData = new Dictionary<string, string>();
                foreach (string key in exception.Data.Keys)
                {
                    CustomData.Add(key, exception.Data[key]?.ToString());
                }
            }
        }

        public string ExceptionType { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public string MethodName { get; set; }
        public string Message { get; set; }
        public string FullMessage { get; set; }
        public string StackTrace { get; set; }
        public string QueryString { get; set; }
        public string HttpMethod { get; set; }
        public Dictionary<string, string> CustomData { get; set; }        

        public string ExceptionId { get { return RowKey; } }

        private const string customDataPrefix = "__";

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            // help from https://stackoverflow.com/a/14595487/2023653
            var results = base.WriteEntity(operationContext);

            if (CustomData != null)
            {
                foreach (var keyPair in CustomData)
                {
                    results.Add(customDataPrefix + keyPair.Key, new EntityProperty(keyPair.Value));
                }                
            }

            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            CustomData = new Dictionary<string, string>();

            foreach (var item in properties)
            {
                if (item.Key.StartsWith(customDataPrefix))
                {
                    string realKey = item.Key.Substring(customDataPrefix.Length);
                    CustomData.Add(realKey, properties[item.Key]?.StringValue);
                }
            }
        }
    }
}
