using AzureTableLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Testing.Exceptions;

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

        private static ExceptionLogger GetLogger()
        {
            var config = GetConfig();
            return new ExceptionLogger(config["StorageAccount:Name"], config["StorageAccount:Key"], "AzureTableLogger", "Tests");
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
        public void SimpleLogWithData()
        {
            var logger = GetLogger();

            try
            {
                throw new CustomException("this is a custom exception", 234);
            }
            catch (CustomException exc)
            {
                var entry = logger.WriteAsync(exc).Result;
                var log = logger.GetAsync(entry.RowKey).Result;
                Assert.IsTrue(log.CustomData["libraryId"].Equals("234"));
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
