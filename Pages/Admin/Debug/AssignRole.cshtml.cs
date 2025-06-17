using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoxPopuli.Pages.Admin.Debug
{
    public class AssignRoleModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHostEnvironment _environment;

        public AssignRoleModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHostEnvironment environment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public List<RoleModel> AvailableRoles { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public bool IsDevelopment => _environment.IsDevelopment();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsDevelopment)
                return NotFound();

            await LoadRolesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!IsDevelopment)
                return NotFound();

            if (string.IsNullOrEmpty(Email))
            {
                StatusMessage = "Please enter an email address";
                await LoadRolesAsync();
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                StatusMessage = $"User with email {Email} not found";
                await LoadRolesAsync();
                return Page();
            }

            foreach (var role in AvailableRoles)
            {
                if (role.IsSelected && !await _roleManager.RoleExistsAsync(role.Name))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role.Name));
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Where(r =>
                AvailableRoles.Any(ar => ar.Name == r && !ar.IsSelected)).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            var rolesToAdd = AvailableRoles
                .Where(r => r.IsSelected && !currentRoles.Contains(r.Name))
                .Select(r => r.Name)
                .ToList();

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            StatusMessage = $"Roles updated for user {user.Email}";
            await LoadRolesAsync();
            return Page();
        }

        private async Task LoadRolesAsync()
        {
            AvailableRoles = new List<RoleModel>();

            string[] defaultRoles = { "Admin", "Supervisor" };

            var existingRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var combinedRoles = defaultRoles.Union(existingRoles).Distinct();

            foreach (var role in combinedRoles)
            {
                AvailableRoles.Add(new RoleModel { Name = role, IsSelected = false });
            }
        }

        public class RoleModel
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}