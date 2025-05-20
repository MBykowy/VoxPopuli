    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using VoxPopuli.Data;
    using VoxPopuli.Models.Domain;
    using VoxPopuli.Models.ViewModels.Surveys;
    using AutoMapper;
    using BCrypt.Net;

    namespace VoxPopuli.Pages.Surveys
    {
        [Authorize]
        public class CreateModel : PageModel
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public CreateModel(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
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
                    return Page();
                }

                // Set creator user ID
                Survey.CreatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Map to domain model
                var surveyEntity = _mapper.Map<Survey>(Survey);
                surveyEntity.CreatedAt = DateTime.UtcNow;

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

                // Redirect to Dashboard instead of List
                return RedirectToPage("Dashboard");
            }

        }
    }
