using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Responses;
using QuestionViewModels = VoxPopuli.Models.ViewModels.Questions;
using ResponseViewModels = VoxPopuli.Models.ViewModels.Responses;

namespace VoxPopuli.Pages.Responses
{
    public class TakeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TakeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public TakeSurveyViewModel Survey { get; set; } = new TakeSurveyViewModel();

        [BindProperty]
        public SurveySubmissionViewModel Submission { get; set; } = new SurveySubmissionViewModel();

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

            Survey = new TakeSurveyViewModel
            {
                SurveyId = survey.SurveyId,
                Title = survey.Title,
                Description = survey.Description,
                Questions = survey.Questions.OrderBy(q => q.Order).Select(q => new ResponseViewModels.QuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.AnswerOptions.OrderBy(o => o.Order).Select(o => new ResponseViewModels.AnswerOptionViewModel
                    {
                        AnswerOptionId = o.AnswerOptionId,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

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

            try
            {
                string? respondentId = null;
                if (!Submission.IsAnonymous && User.Identity?.IsAuthenticated == true)
                {
                    respondentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                var response = new Response
                {
                    SurveyId = Submission.SurveyId,
                    SubmittedAt = DateTime.UtcNow,
                    IsAnonymous = Submission.IsAnonymous,
                    RespondentUserId = respondentId
                };

                response.Answers = new List<Answer>();

                foreach (var answerSubmission in Submission.Answers)
                {
                    var question = survey.Questions.FirstOrDefault(q => q.QuestionId == answerSubmission.QuestionId);
                    if (question == null) continue;

                    switch (question.QuestionType)
                    {
                        case QuestionType.SingleChoice:
                            if (answerSubmission.SelectedOptionId.HasValue)
                            {
                                response.Answers.Add(new Answer
                                {
                                    QuestionId = answerSubmission.QuestionId,
                                    SelectedOptionId = answerSubmission.SelectedOptionId
                                });
                            }
                            break;

                        case QuestionType.MultipleChoice:
                            foreach (var optionId in answerSubmission.SelectedOptionIds)
                            {
                                response.Answers.Add(new Answer
                                {
                                    QuestionId = answerSubmission.QuestionId,
                                    SelectedOptionId = optionId
                                });
                            }
                            break;

                        case QuestionType.Text:
                            response.Answers.Add(new Answer
                            {
                                QuestionId = answerSubmission.QuestionId,
                                AnswerText = answerSubmission.TextAnswer ?? ""
                            });
                            break;

                        case QuestionType.Rating:
                            if (answerSubmission.Rating.HasValue)
                            {
                                response.Answers.Add(new Answer
                                {
                                    QuestionId = answerSubmission.QuestionId,
                                    RatingValue = answerSubmission.Rating
                                });
                            }
                            break;
                    }
                }

                _context.Responses.Add(response);
                await _context.SaveChangesAsync();

                return RedirectToPage("ThankYou", new { id = response.ResponseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}