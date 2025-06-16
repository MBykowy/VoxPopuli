using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Surveys;
using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VoxPopuli.Pages.Surveys
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, IMapper mapper, ILogger<CreateModel> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [BindProperty]
        public SurveyCreateViewModel Survey { get; set; } = new SurveyCreateViewModel();

        public void OnGet()
        {
            // Default values already set in the view model constructor
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Model validation error: {ErrorMessage}", error.ErrorMessage);
                }
                return Page();
            }

            // Get the current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify the user ID exists in the database
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID not found in claims");
                ModelState.AddModelError(string.Empty, "User not authenticated properly. Please log in again.");
                return Page();
            }

            // Verify user exists in database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found in database", userId);
                ModelState.AddModelError(string.Empty, "Your user account was not found. Please log in again.");
                return Page();
            }

            try
            {
                // Map to domain model
                var surveyEntity = _mapper.Map<Survey>(Survey);

                // Set properties that might get lost or overridden during mapping
                surveyEntity.CreatorUserId = userId;
                surveyEntity.CreatedAt = DateTime.UtcNow;

                // Log for debugging
                _logger.LogInformation("Creating survey with Title: {Title}, CreatorUserId: {UserId}",
                    surveyEntity.Title, surveyEntity.CreatorUserId);

                // If password is provided, hash it
                if (!string.IsNullOrEmpty(Survey.Password))
                {
                    surveyEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Survey.Password);
                }

                // Save to database
                _context.Surveys.Add(surveyEntity);
                await _context.SaveChangesAsync();

                // Add success message to TempData
                TempData["SuccessMessage"] = $"Survey '{surveyEntity.Title}' has been created successfully!";
                TempData["NewSurveyId"] = surveyEntity.SurveyId;

                // Redirect to List instead of Dashboard (which requires Admin/Supervisor roles)
                return RedirectToPage("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating survey");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the survey. Please try again.");
                return Page();
            }
        }
    }
}