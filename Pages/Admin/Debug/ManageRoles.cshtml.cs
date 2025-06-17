using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace VoxPopuli.Pages.Admin.Debug
{
    public class ManageRolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHostEnvironment _hostEnvironment;

        public ManageRolesModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public bool IsDevelopment => _hostEnvironment.IsDevelopment();

        public IActionResult OnGet()
        {
            if (!IsDevelopment)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!IsDevelopment)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(Email))
            {
                StatusMessage = "Email is required.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                StatusMessage = $"User with email {Email} not found.";
                return Page();
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("Supervisor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Supervisor"));
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            if (!await _userManager.IsInRoleAsync(user, "Supervisor"))
            {
                await _userManager.AddToRoleAsync(user, "Supervisor");
            }

            StatusMessage = $"User {Email} has been given Admin and Supervisor roles.";
            return Page();
        }
    }
}