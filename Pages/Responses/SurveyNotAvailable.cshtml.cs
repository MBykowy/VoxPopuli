using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VoxPopuli.Pages.Responses
{
    public class SurveyNotAvailableModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Message { get; set; } = "This survey is not available.";

        public void OnGet()
        {
            // Message already set through binding
        }
    }
}