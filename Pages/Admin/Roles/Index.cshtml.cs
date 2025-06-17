using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace VoxPopuli.Pages.Admin.Roles
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public List<IdentityRole> Roles { get; set; } = new();

        [BindProperty]
        [Required(ErrorMessage = "Role name is required")]
        [Display(Name = "New Role Name")]
        public string NewRoleName { get; set; }

        public async Task OnGetAsync()
        {
            Roles = await _roleManager.Roles.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await _roleManager.Roles.ToListAsync();
                return Page();
            }

            if (await _roleManager.RoleExistsAsync(NewRoleName))
            {
                ModelState.AddModelError(string.Empty, $"Role '{NewRoleName}' already exists.");
                Roles = await _roleManager.Roles.ToListAsync();
                return Page();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(NewRoleName));

            if (result.Succeeded)
            {
                StatusMessage = $"Role '{NewRoleName}' created successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            Roles = await _roleManager.Roles.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            if (role.Name == "Admin")
            {
                StatusMessage = "The Admin role cannot be deleted.";
                return RedirectToPage();
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Role '{role.Name}' deleted successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            Roles = await _roleManager.Roles.ToListAsync();
            return Page();
        }
    }
}