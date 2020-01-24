using AzureTableLogger.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTableLogger.Netcore
{
    public static class Extensions
    {
        public async static Task<ExceptionEntity> WriteAsync(this ExceptionLogger logger, ExceptionContext context, Dictionary<string, string> customData = null)
        {            
            var log = new ExceptionEntity(logger.AppName, context.Exception)
            {
                UserName = context.HttpContext.User.Identity.Name,
                MethodName = context.HttpContext.Request.Path.Value,
                QueryString = context.HttpContext.Request.QueryString.Value,
                HttpMethod = context.HttpContext.Request.Method,
                Cookies = GetCookies(context.HttpContext.Request.Cookies),                
                CustomData = customData
            };

            if (context.HttpContext.Request.Method.ToLower().Equals("post"))
            {
                log.FormValues = GetFormValues(context.HttpContext.Request.Form);
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
