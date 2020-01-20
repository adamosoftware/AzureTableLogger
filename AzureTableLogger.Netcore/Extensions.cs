using AzureTableLogger.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTableLogger.Netcore
{
    public static class Extensions
    {
        public async static Task<ExceptionEntity> WriteAsync(this TableStorageLogger logger, ExceptionContext context, Dictionary<string, string> customData = null)
        {            
            var log = new ExceptionEntity(logger.AppName, context.Exception)
            {
                UserName = context.HttpContext.User.Identity.Name,
                MethodName = context.HttpContext.Request.Path.Value,
                QueryString = context.HttpContext.Request.QueryString.Value,
                HttpMethod = context.HttpContext.Request.Method,
                CustomData = customData
            };

            await logger.WriteLogAsync(log);

            return log;
        }
    }
}
