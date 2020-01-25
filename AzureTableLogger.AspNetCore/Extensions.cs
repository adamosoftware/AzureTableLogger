using AzureTableLogger.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AzureTableLogger.AspNetCore
{
    public static class Extensions
    {
        public async static Task<ExceptionEntity> WriteAsync(this ExceptionLogger logger, 
            Exception exception, HttpContext httpContext, Dictionary<string, string> customData = null,            
            [CallerFilePath]string sourceFile = null, [CallerLineNumber]int lineNumber = 0)
        {            
            var log = new ExceptionEntity(logger.AppName, exception)
            {
                UserName = httpContext.User.Identity.Name,
                MethodName = httpContext.Request.Path.Value,
                QueryString = httpContext.Request.QueryString.Value,
                HttpMethod = httpContext.Request.Method,
                Cookies = GetCookies(httpContext.Request.Cookies),                
                CustomData = customData,
                SourceFile = sourceFile,
                LineNumber = lineNumber
            };

            if (httpContext.Request.Method.ToLower().Equals("post"))
            {
                log.FormValues = GetFormValues(httpContext.Request.Form);
            }

            await logger.WriteLogAsync(log);

            return log;
        }

        private static Dictionary<string, string> GetFormValues(IFormCollection form)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var field in form.Keys)
            {
                if (!field.ToLower().Contains("requestverificationtoken"))
                {
                    result.Add(field, form[field].ToString());
                }                
            }
            return result;
        }

        private static Dictionary<string, string> GetCookies(IRequestCookieCollection cookies)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var cookie in cookies) result.Add(cookie.Key, cookie.Value);
            return result;
        }
    }
}
