using AzureTableLogger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SampleApp.Filters
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        private readonly TableStorageLogger _logger;

        public ExceptionFilter(IConfiguration config)
        {
            _logger = new TableStorageLogger(config["StorageAccount:Name"], config["StorageAccount:Key"], "Exceptions", "SampleApp");
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var result = await _logger.WriteAsync(context);

            context.Result = new RedirectResult($"/Exception?key={result.RowKey}");
        }
    }
}
