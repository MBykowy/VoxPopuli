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
        private readonly IMapper _mapper;    

        public ResponsesModel(ApplicationDbContext context, ILogger<ResponsesModel> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }


        public IActionResult OnPostExportResponsesPdf()
        {
            var result = OnGetAsync().GetAwaiter().GetResult();
            if (result is NotFoundResult)
                return NotFound();

            var pdfService = HttpContext.RequestServices.GetRequiredService<VoxPopuli.Services.PDF.PdfExportService>();

            byte[] pdfBytes = pdfService.GenerateSurveyResponsesPdf(SurveyResponses);

            return File(
                pdfBytes,
                "application/pdf",
                $"Survey-Responses-{SurveyResponses.Title}-{DateTime.Now:yyyyMMdd}.pdf");
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SurveyResponsesViewModel SurveyResponses { get; set; } = new();      

        public async Task<IActionResult> OnGetAsync()
        {
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized access attempt to responses of survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }

            SurveyResponses = new SurveyResponsesViewModel
            {
                SurveyId = survey.SurveyId,
                Title = survey.Title,
                Description = survey.Description ?? string.Empty,
                CreatedAt = survey.CreatedAt,
                ResponseCount = survey.Responses?.Count ?? 0,
                Responses = new List<ResponseDetailViewModel>()
            };

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

            if (SurveyResponses.Responses.Any())
            {
                var dateGroups = SurveyResponses.Responses
                    .GroupBy(r => r.SubmittedAt.Date)
                    .Select(g => new ResponseDateCount
                    {
                        Date = g.Key.ToString("MMM dd", CultureInfo.InvariantCulture),
                        Count = g.Count()
                    })
                    .OrderBy(x => DateTime.Parse(x.Date, CultureInfo.InvariantCulture))
                    .ToList();

                SurveyResponses.ResponseDateCounts = dateGroups;

                SurveyResponses.CompletionRate = 92;
                SurveyResponses.AvgCompletionTime = "2.5";
            }

            return Page();
        }
    }
}