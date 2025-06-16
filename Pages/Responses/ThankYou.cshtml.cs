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

        public Response? Response { get; set; }
        public Survey? Survey { get; set; }
        public bool IsAnonymous { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0)
            {
                return NotFound();
            }

            // Retrieve the response with careful null handling
            var response = await _context.Responses
                .Include(r => r.Survey)
                .FirstOrDefaultAsync(r => r.ResponseId == Id);

            if (response == null)
            {
                return NotFound();
            }

            Response = response;
            Survey = response.Survey;
            IsAnonymous = response.IsAnonymous;

            return Page();
        }
    }
}