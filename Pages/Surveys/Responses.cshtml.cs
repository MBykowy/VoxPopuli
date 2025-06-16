using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Pages.Surveys
{
    [Authorize]
    public class ResponsesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResponsesModel> _logger;

        public ResponsesModel(ApplicationDbContext context, ILogger<ResponsesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SurveyResponsesViewModel SurveyResponses { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Accessing responses for survey ID: {SurveyId}", Id);

            try
            {
                // Get survey with all related data
                var survey = await _context.Surveys
                    .AsNoTracking() // For better performance on read-only operations
                    .Include(s => s.Questions)
                    .Include(s => s.Responses)
                        .ThenInclude(r => r.Answers)
                            .ThenInclude(a => a.Question)
                    .Include(s => s.Responses)
                        .ThenInclude(r => r.Answers)
                            .ThenInclude(a => a.SelectedOption)
                    .FirstOrDefaultAsync(s => s.SurveyId == Id);

                if (survey == null)
                {
                    _logger.LogWarning("Survey with ID {SurveyId} not found", Id);
                    return NotFound();
                }

                // Check if current user is authorized to view responses
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning("User {UserId} not authorized to view responses for survey {SurveyId}",
                        currentUserId, Id);
                    return Forbid();
                }

                _logger.LogInformation("Found {Count} responses for survey {SurveyId}",
                    survey.Responses?.Count ?? 0, Id);

                // Map to view model
                SurveyResponses = new SurveyResponsesViewModel
                {
                    SurveyId = survey.SurveyId,
                    Title = survey.Title,
                    Description = survey.Description,
                    CreatedAt = survey.CreatedAt,
                    ResponseCount = survey.Responses?.Count ?? 0,
                    Responses = (survey.Responses ?? new List<Response>())
                        .OrderByDescending(r => r.SubmittedAt)
                        .Select(r => new ResponseDetailViewModel
                        {
                            ResponseId = r.ResponseId,
                            SubmittedAt = r.SubmittedAt,
                            IsAnonymous = r.IsAnonymous,
                            RespondentName = r.IsAnonymous ? "Anonymous" :
                                (r.RespondentUserId ?? "Unknown User"),
                            Answers = r.Answers?.Select(a => new ResponseAnswerViewModel
                            {
                                QuestionId = a.QuestionId,
                                QuestionText = a.Question?.QuestionText ?? "Unknown Question",
                                AnswerText = a.AnswerText,
                                Rating = a.RatingValue,
                                SelectedOptionText = a.SelectedOption?.OptionText
                            }).ToList() ?? new List<ResponseAnswerViewModel>()
                        }).ToList()
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading responses for survey {SurveyId}", Id);
                TempData["ErrorMessage"] = "There was an error loading the survey responses.";
                return RedirectToPage("List");
            }
        }
    }
}