using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;

namespace VoxPopuli.Pages.Responses
{
    public class ThankYouModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ThankYouModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        // Remove private modifier and use new keyword
        public new Response? Response { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == null)
            {
                return RedirectToPage("/Index");
            }

            Response = await _context.Responses
                .Include(r => r.Survey)
                .FirstOrDefaultAsync(r => r.ResponseId == Id);

            if (Response == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
