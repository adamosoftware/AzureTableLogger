using AzureTableLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Testing
{
    [TestClass]
    public class LoggingTests
    {
        private static IConfigurationRoot GetConfig()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();
        }

        private static TableStorageLogger GetLogger()
        {
            var config = GetConfig();
            return new TableStorageLogger(config["StorageAccount:Name"], config["StorageAccount:Key"], "Tests", "AzureTableLogger");
        }

        [TestMethod]
        public void SimpleLog()
        {            
            var logger = GetLogger();

            try
            {
                throw new Exception("this is a sample exception");
            }
            catch (Exception exc)
            {
                logger.WriteAsync(exc).Wait();
            }
        }

        [TestMethod]
        public void SimpleLogAndRetrieve()
        {
            var logger = GetLogger();

            try
            {
                throw new Exception("this is a sample exception");
            }
            catch (Exception exc)
            {
                var result = logger.WriteAsync(exc).Result;
                var lookup = logger.GetAsync(result.RowKey).Result;
                Assert.IsTrue(lookup.MethodName.Equals("SimpleLogAndRetrieve"));
            }
        }
    }
}
