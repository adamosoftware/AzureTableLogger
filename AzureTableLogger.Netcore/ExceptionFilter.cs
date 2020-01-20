using AzureTableLogger.Netcore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTableLogger
{
    public abstract class ExceptionFilterBase : IAsyncExceptionFilter
    {
        private readonly TableStorageLogger _logger;

        public ExceptionFilterBase(IConfiguration config)
        {
            _logger = CreateLogger(config);
        }

        protected abstract TableStorageLogger CreateLogger(IConfiguration config);

        protected abstract string GetRedirectUrl(string exceptionId);

        protected virtual Dictionary<string, string> GetCustomData(ExceptionContext context)
        {
            return null;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var customData = GetCustomData(context);
            var result = await _logger.WriteAsync(context, customData);
            context.Result = new RedirectResult(GetRedirectUrl(result.RowKey));            
        }
    }
}
