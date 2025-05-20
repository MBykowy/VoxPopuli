using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VoxPopuli.Data;
using VoxPopuli.Models.ViewModels.Responses;
using VoxPopuli.Models.Domain;
using AutoMapper;

namespace VoxPopuli.Pages.Responses
{
    public class TakeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TakeModel(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public TakeSurveyViewModel Survey { get; set; } = default!;

        [BindProperty]
        public SurveySubmissionViewModel Submission { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
                return NotFound();

            if (!survey.IsActive)
                return RedirectToPage("SurveyNotAvailable", new { message = "This survey is not currently active." });

            var now = DateTime.UtcNow;
            if (survey.StartDate.HasValue && survey.StartDate.Value > now)
                return RedirectToPage("SurveyNotAvailable", new { message = $"This survey will be available starting {survey.StartDate.Value:MMMM dd, yyyy}." });

            if (survey.EndDate.HasValue && survey.EndDate.Value < now)
                return RedirectToPage("SurveyNotAvailable", new { message = $"This survey ended on {survey.EndDate.Value:MMMM dd, yyyy}." });

            if (!string.IsNullOrEmpty(survey.PasswordHash))
                return RedirectToPage("PasswordPrompt", new { id = Id });

            Survey = _mapper.Map<TakeSurveyViewModel>(survey);
            Submission.SurveyId = Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.SurveyId == Submission.SurveyId);

            if (survey == null)
                return NotFound();

            // Validate required questions
            var requiredIds = survey.Questions.Where(q => q.IsRequired).Select(q => q.QuestionId).ToList();
            foreach (var reqId in requiredIds)
            {
                var answer = Submission.Answers.FirstOrDefault(a => a.QuestionId == reqId);
                if (answer == null || !answer.HasAnswer())
                {
                    ModelState.AddModelError("", "Please answer all required questions.");
                    await OnGetAsync();
                    return Page();
                }
            }

            // Save response
            var response = new Response
            {
                SurveyId = Submission.SurveyId,
                SubmittedAt = DateTime.UtcNow,
                IsAnonymous = Submission.IsAnonymous,
                RespondentUserId = Submission.IsAnonymous ? "anonymous" : User.Identity?.Name ?? "anonymous",
                Answers = Submission.Answers.SelectMany(a =>
                {
                    var question = survey.Questions.First(q => q.QuestionId == a.QuestionId);
                    return question.QuestionType switch
                    {
                        QuestionType.SingleChoice => a.SelectedOptionId.HasValue
                            ? new[] { new Answer { QuestionId = a.QuestionId, SelectedOptionId = a.SelectedOptionId } }
                            : Array.Empty<Answer>(),
                        QuestionType.MultipleChoice => a.SelectedOptionIds.Select(optId => new Answer { QuestionId = a.QuestionId, SelectedOptionId = optId }),
                        QuestionType.Text => new[] { new Answer { QuestionId = a.QuestionId, AnswerText = a.TextAnswer ?? "" } },
                        QuestionType.Rating => a.Rating.HasValue
                            ? new[] { new Answer { QuestionId = a.QuestionId, RatingValue = a.Rating } }
                            : Array.Empty<Answer>(),
                        _ => Array.Empty<Answer>()
                    };
                }).ToList()
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            return RedirectToPage("ThankYou", new { id = response.ResponseId });
        }
    }
}
