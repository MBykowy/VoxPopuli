using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using System.Threading.Tasks;

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
        public int Id { get; set; }

        public Survey? Survey { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _context.Responses
                .Include(r => r.Survey)
                .FirstOrDefaultAsync(r => r.ResponseId == Id);

            if (response == null)
                return RedirectToPage("/Index");

            Survey = response.Survey;
            return Page();
        }
    }
}