using AzureTableLogger;
using Microsoft.Extensions.Configuration;

namespace SampleApp.Filters
{
    public class MyExceptionFilter : ExceptionFilterBase
    {
        public MyExceptionFilter(IConfiguration config) : base(config)
        {
        }

        protected override ExceptionLogger CreateLogger(IConfiguration config)
        {
            return MyLogBuilder.Create(config);
        }

        protected override string GetRedirectUrl(string exceptionId)
        {
            return $"/Exception?id={exceptionId}";
        }
    }
}
