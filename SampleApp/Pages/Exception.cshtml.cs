using AzureTableLogger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SampleApp.Filters;
using System.Threading.Tasks;

namespace SampleApp.Pages
{
    public class ExceptionModel : PageModel
    {
        private readonly ExceptionLogger _logger = null;

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public ExceptionModel(IConfiguration config)
        {
            _logger = MyLogBuilder.Create(config);
        }

        public async Task OnGetAsync()
        {
            var exception = await _logger.GetAsync(Id);
            ErrorMessage = exception.Message;
        }

        public string ErrorMessage { get; set; }
    }
}
