using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public string SourceFile { get; set; }
        public int LineNumber { get; set; }
        public Dictionary<string, string> CustomData { get; set; }
        public Dictionary<string, string> FormValues { get; set; }
        public Dictionary<string, string> Cookies { get; set; }

        public string ExceptionId { get { return RowKey; } }

        private const string customDataPrefix = "__";
        private const string formValuesPrefix = "frm_";
        private const string cookiePrefix = "cookie_";

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            // help from https://stackoverflow.com/a/14595487/2023653
            var results = base.WriteEntity(operationContext);

            WriteCustomDictionary(results, CustomData, customDataPrefix);
            WriteCustomDictionary(results, FormValues, formValuesPrefix);
            WriteCustomDictionary(results, Cookies, cookiePrefix);

            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            CustomData = ReadCustomDictionary(customDataPrefix, properties);
            FormValues = ReadCustomDictionary(formValuesPrefix, properties);
            Cookies = ReadCustomDictionary(cookiePrefix, properties);
        }

        private static Dictionary<string, string> ReadCustomDictionary(string prefix, IDictionary<string, EntityProperty> properties)
        {
            var result = new Dictionary<string, string>();

            foreach (var item in properties)
            {
                if (item.Key.StartsWith(prefix))
                {
                    string realKey = item.Key.Substring(prefix.Length);
                    result.Add(realKey, properties[item.Key]?.StringValue);
                }
            }

            return result;
        }

        private static void WriteCustomDictionary(IDictionary<string, EntityProperty> entity, Dictionary<string, string> dictionary, string prefix)
        {
            if (dictionary != null)
            {
                foreach (var keyPair in dictionary.Where(kp => !kp.Key.StartsWith(".AspNet")))
                {
                    string key = (prefix + keyPair.Key).Replace("-", "_");
                    entity.Add(key, new EntityProperty(keyPair.Value));
                }
            }
        }
    }
}
