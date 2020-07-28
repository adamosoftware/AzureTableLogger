using AzureTableLogger.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AzureTableLogger
{
    public abstract class ExceptionFilterBase : IAsyncExceptionFilter
    {
        private readonly ExceptionLogger _logger;

        public ExceptionFilterBase(IConfiguration config)
        {
            _logger = CreateLogger(config);
        }

        protected abstract ExceptionLogger CreateLogger(IConfiguration config);

        protected abstract string GetRedirectUrl(string exceptionId);

        protected virtual Dictionary<string, string> GetCustomData(HttpContext httpContext)
        {
            return null;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            try
            {
                var customData = GetCustomData(context.HttpContext);
                var result = await _logger.WriteAsync(context.Exception, context.HttpContext, customData);
                context.Result = new RedirectResult(GetRedirectUrl(result.RowKey));
            }
            catch (Exception exc)
            {
                throw new Exception($"Unable to log exception: {exc.Message}");
            }
        }

        public async Task<IActionResult> TryCatchRedirectAsync(
            HttpContext httpContext, Func<Task<IActionResult>> action,
            [CallerFilePath]string sourceFile = null, [CallerLineNumber]int lineNumber = 0)
        {
            try
            {
                return await action.Invoke();
            }
            catch (Exception exc)
            {
                var customData = GetCustomData(httpContext);
                var entity = await Extensions.WriteAsync(_logger, exc, httpContext, customData, sourceFile, lineNumber);
                return new RedirectResult(GetRedirectUrl(entity.RowKey));
            }
        }
    }
}
