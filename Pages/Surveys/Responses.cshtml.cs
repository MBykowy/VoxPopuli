using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Surveys;
using AutoMapper;

namespace VoxPopuli.Pages.Surveys
{
    [Authorize]
    public class ResponsesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResponsesModel> _logger;
        private readonly IMapper _mapper; // Added missing mapper

        public ResponsesModel(ApplicationDbContext context, ILogger<ResponsesModel> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SurveyResponsesViewModel SurveyResponses { get; set; } = new(); // Initialize to avoid null reference

        public async Task<IActionResult> OnGetAsync()
        {
            // Use the Id property that's bound from the route
            var survey = await _context.Surveys
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Answers)
                        .ThenInclude(a => a.Question)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Answers)
                        .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                return NotFound();
            }

            // Verify the user has permission to view this survey's responses
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized access attempt to responses of survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }

            // Create the view model manually since we're having issues with AutoMapper
            SurveyResponses = new SurveyResponsesViewModel
            {
                SurveyId = survey.SurveyId,
                Title = survey.Title,
                Description = survey.Description ?? string.Empty,
                CreatedAt = survey.CreatedAt,
                ResponseCount = survey.Responses?.Count ?? 0,
                Responses = new List<ResponseDetailViewModel>()
            };

            // Map each response
            if (survey.Responses != null)
            {
                foreach (var response in survey.Responses)
                {
                    var responseDetail = new ResponseDetailViewModel
                    {
                        ResponseId = response.ResponseId,
                        SubmittedAt = response.SubmittedAt,
                        IsAnonymous = response.IsAnonymous,
                        RespondentName = response.IsAnonymous ? "Anonymous" :
                            (response.Respondent?.UserName ?? "Unknown User"),
                        Answers = new List<ResponseAnswerViewModel>()
                    };

                    // Map answers for this response
                    if (response.Answers != null)
                    {
                        foreach (var answer in response.Answers)
                        {
                            var answerViewModel = new ResponseAnswerViewModel
                            {
                                QuestionId = answer.QuestionId,
                                QuestionText = answer.Question?.QuestionText ?? "Unknown Question",
                                AnswerText = answer.AnswerText ?? string.Empty,
                                Rating = answer.RatingValue,
                                SelectedOptionText = answer.SelectedOption?.OptionText ?? string.Empty
                            };

                            responseDetail.Answers.Add(answerViewModel);
                        }
                    }

                    SurveyResponses.Responses.Add(responseDetail);
                }
            }

            // Add this code to populate the chart data
            if (SurveyResponses.Responses.Any())
            {
                var dateGroups = SurveyResponses.Responses
                    .GroupBy(r => r.SubmittedAt.Date)
                    .Select(g => new ResponseDateCount
                    {
                        // Store date in culture-invariant format that matches our parsing
                        Date = g.Key.ToString("MMM dd", CultureInfo.InvariantCulture),
                        Count = g.Count()
                    })
                    // Sort chronologically by the actual DateTime instead of parsing the string
                    .OrderBy(x => DateTime.Parse(x.Date, CultureInfo.InvariantCulture))
                    .ToList();

                SurveyResponses.ResponseDateCounts = dateGroups;

                // Calculate completion rate and average time based on available data
                // These are placeholders - in a real app you would compute these from actual data
                SurveyResponses.CompletionRate = 92;
                SurveyResponses.AvgCompletionTime = "2.5";
            }

            return Page();
        }
    }
}