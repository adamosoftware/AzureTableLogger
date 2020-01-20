using AzureTableLogger;
using Microsoft.Extensions.Configuration;

namespace SampleApp.Filters
{
    public class MyExceptionLogger : ExceptionLogger
    {
        public MyExceptionLogger(IConfiguration config) : base(config["StorageAccount:Name"], config["StorageAccount:Key"], "SampleApp")
        {
        }
    }
}
