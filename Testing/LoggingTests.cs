using AzureTableLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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

        private static ExceptionLogger GetLogger(string appName = "AzureTableLogger", string tableName = "Tests")
        {
            var config = GetConfig();
            return new ExceptionLogger(config["StorageAccount:Name"], config["StorageAccount:Key"], appName, tableName);
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

        [TestMethod]
        public void NestedMessage()
        {
            try
            {
                try
                {
                    try
                    {
                        throw new Exception("here is the innermost exception");
                    }
                    catch (Exception exc)
                    {
                        throw new Exception("this is the mid-level exception", exc);
                    }
                }
                catch (Exception exc)
                {
                    throw new Exception("this is the outermost exception", exc);
                }
            }
            catch (Exception outer)
            {
                var logger = GetLogger();
                var log = logger.WriteAsync(outer).Result;
                var lookup = logger.GetAsync(log.ExceptionId).Result;
                Assert.IsTrue(lookup.FullMessage.Equals("this is the outermost exception\r\n- this is the mid-level exception\r\n-- here is the innermost exception"));
            }
        }

        [TestMethod]
        public void Query()
        {
            var logger = GetLogger();
            var results = logger.QueryAsync().Result;
            Assert.IsTrue(results.All(ent => ent.PartitionKey.Equals(logger.AppName)));
        }

        [TestMethod]
        public void QueryWhere()
        {
            var logger = GetLogger();
            var results = logger.QueryAsync("this exception").Result;
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public void QuerySqlChartify()
        {
            // dealing with symptoms from this issue https://github.com/Azure/azure-storage-net/issues/77

            var logger = GetLogger("SqlChartify", "Exceptions");
            var results = logger.QueryAsync().Result;
            Assert.IsTrue(results.All(ent => ent.PartitionKey.Equals(logger.AppName)));

        }

        [TestMethod]
        public void Purge()
        {
            var logger = GetLogger();
            logger.PurgeAfterAsync(TimeSpan.FromHours(3)).Wait();

            var results = logger.QueryAsync().Result;
            Assert.IsTrue(results.All(ent => DateTime.UtcNow.Subtract(ent.Timestamp.UtcDateTime) < TimeSpan.FromDays(3)));
        }
    }
}
