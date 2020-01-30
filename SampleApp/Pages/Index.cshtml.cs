using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;

namespace SampleApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGetThrow()
        {
            throw new Exception("This is an unhandled exception that should be logged.");
        }

        public IActionResult OnPostFormError()
        {
            throw new Exception("This is an error from a form which should have form values saved.");
        }
    }
}
