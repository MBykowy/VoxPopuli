using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        public ResponsesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SurveyResponsesViewModel SurveyResponses { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get survey with all related data
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Answers)
                        .ThenInclude(a => a.Question)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Answers)
                        .ThenInclude(a => a.SelectedOption)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Respondent)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                return NotFound();
            }

            // Check if current user is authorized to view responses
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Map to view model
            SurveyResponses = new SurveyResponsesViewModel
            {
                SurveyId = survey.SurveyId,
                Title = survey.Title,
                Description = survey.Description,
                CreatedAt = survey.CreatedAt,
                ResponseCount = survey.Responses.Count,
                Responses = survey.Responses
                    .OrderByDescending(r => r.SubmittedAt)
                    .Select(r => new ResponseDetailViewModel
                    {
                        ResponseId = r.ResponseId,
                        SubmittedAt = r.SubmittedAt,
                        IsAnonymous = r.IsAnonymous,
                        RespondentName = r.IsAnonymous ? "Anonymous" : (r.Respondent?.UserName ?? "Unknown User"),
                        Answers = r.Answers.Select(a => new ResponseAnswerViewModel
                        {
                            QuestionId = a.QuestionId,
                            QuestionText = a.Question?.QuestionText ?? "Unknown Question",
                            AnswerText = a.AnswerText,
                            Rating = a.RatingValue,
                            SelectedOptionText = a.SelectedOption?.OptionText
                        }).ToList()
                    }).ToList()
            };

            return Page();
        }
    }
}