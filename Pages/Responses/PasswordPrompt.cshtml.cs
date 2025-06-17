using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VoxPopuli.Data;
using BCrypt.Net;

namespace VoxPopuli.Pages.Responses
{
    public class PasswordPromptModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PasswordPromptModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public string Password { get; set; } = "";

        [TempData]
        public string ErrorMessage { get; set; }

        public string SurveyTitle { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var survey = await _context.Surveys
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
                return NotFound();

            if (string.IsNullOrEmpty(survey.PasswordHash))
                return RedirectToPage("/Responses/Take", new { id = Id });

            SurveyTitle = survey.Title;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Password is required";
                return await OnGetAsync();
            }

            var survey = await _context.Surveys
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
                return NotFound();

            bool passwordValid = BCrypt.Net.BCrypt.Verify(Password, survey.PasswordHash);

            if (!passwordValid)
            {
                ErrorMessage = "Incorrect password";
                return await OnGetAsync();
            }

            return RedirectToPage("/Responses/Take", new { id = Id });
        }
    }
}