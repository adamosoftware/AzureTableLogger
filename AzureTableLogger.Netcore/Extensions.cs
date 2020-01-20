using AzureTableLogger.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureTableLogger.AspNetCore
{
    public static class Extensions
    {
        public async static Task<ExceptionEntity> WriteAsync(this TableStorageLogger logger, ExceptionContext context)
        {
            var log = new ExceptionEntity(logger.AppName, context.Exception)
            {
                UserName = context.HttpContext.User.Identity.Name,
                MethodName = context.HttpContext.Request.Path.Value,
                QueryString = context.HttpContext.Request.QueryString.Value,
                HttpMethod = context.HttpContext.Request.Method
            };

            await logger.AddLogAsync(log);

            return log;
        }

    }
}
