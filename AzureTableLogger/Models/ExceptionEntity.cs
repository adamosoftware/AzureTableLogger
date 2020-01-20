﻿using Microsoft.WindowsAzure.Storage.Table;
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
        public string QueryString { get; set; }
        public string HttpMethod { get; set; }
        public Dictionary<string, string> CustomData { get; set; }
    }
}
