using AzureTableLogger;
using Microsoft.Extensions.Configuration;

namespace SampleApp.Filters
{
    public static class MyLogBuilder
    {
        public static ExceptionLogger Create(IConfiguration config)
        {
            return new ExceptionLogger(config["StorageAccount:Name"], config["StorageAccount:Key"], "SampleApp");
        }
    }
}
