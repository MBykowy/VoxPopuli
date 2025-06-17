using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Surveys;
using VoxPopuli.Models.ViewModels.Questions;      
using AutoMapper;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace VoxPopuli.Pages.Surveys
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext context, IMapper mapper, ILogger<EditModel> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [BindProperty]
        public SurveyCreateViewModel Survey { get; set; } = new SurveyCreateViewModel();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized access attempt to edit survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }
            Survey = new SurveyCreateViewModel
            {
                Title = survey.Title,
                Description = survey.Description,
                CreatorUserId = survey.CreatorUserId,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                IsActive = survey.IsActive,
                AllowAnonymous = survey.AllowAnonymous,
                Questions = new List<VoxPopuli.Models.ViewModels.Questions.QuestionViewModel>()
            };


            foreach (var question in survey.Questions.OrderBy(q => q.Order))
            {
                var questionVM = new VoxPopuli.Models.ViewModels.Questions.QuestionViewModel
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    IsRequired = question.IsRequired,
                    Order = question.Order,
                    Options = new List<VoxPopuli.Models.ViewModels.Questions.AnswerOptionViewModel>()
                };

                if (question.AnswerOptions != null)
                {
                    foreach (var option in question.AnswerOptions.OrderBy(o => o.Order))
                    {
                        questionVM.Options.Add(new VoxPopuli.Models.ViewModels.Questions.AnswerOptionViewModel
                        {
                            AnswerOptionId = option.AnswerOptionId,
                            OptionText = option.OptionText,
                            Order = option.Order
                        });
                    }
                }

                Survey.Questions.Add(questionVM);
            }

            return Page();
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

            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized update attempt for survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }

            try
            {
                survey.Title = Survey.Title;
                survey.Description = Survey.Description ?? string.Empty;
                survey.StartDate = Survey.StartDate;
                survey.EndDate = Survey.EndDate;
                survey.IsActive = Survey.IsActive;
                survey.AllowAnonymous = Survey.AllowAnonymous;

                if (!string.IsNullOrEmpty(Survey.Password))
                {
                    survey.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Survey.Password);
                }

                if (Survey.Questions != null)
                {
                    var existingQuestionIds = survey.Questions.Select(q => q.QuestionId).ToList();
                    var submittedQuestionIds = Survey.Questions
                        .Where(q => q.QuestionId > 0)
                        .Select(q => q.QuestionId)
                        .ToList();

                    var questionsToDelete = existingQuestionIds.Except(submittedQuestionIds).ToList();
                    foreach (var questionId in questionsToDelete)
                    {
                        var questionToDelete = await _context.Questions
                            .Include(q => q.AnswerOptions)
                            .FirstOrDefaultAsync(q => q.QuestionId == questionId);

                        if (questionToDelete != null)
                        {
                            _context.AnswerOptions.RemoveRange(questionToDelete.AnswerOptions);
                            _context.Questions.Remove(questionToDelete);
                        }
                    }

                    int questionOrder = 0;
                    foreach (var questionVM in Survey.Questions)
                    {
                        Question question;

                        if (questionVM.QuestionId > 0)
                        {
                            question = survey.Questions.FirstOrDefault(q => q.QuestionId == questionVM.QuestionId);
                            if (question != null)
                            {
                                question.QuestionText = questionVM.QuestionText;
                                question.QuestionType = questionVM.QuestionType;
                                question.IsRequired = questionVM.IsRequired;
                                question.Order = questionOrder++;

                                if ((question.QuestionType == QuestionType.SingleChoice ||
                                     question.QuestionType == QuestionType.MultipleChoice) &&
                                     questionVM.Options != null)
                                {
                                    var existingOptionIds = question.AnswerOptions.Select(o => o.AnswerOptionId).ToList();
                                    var submittedOptionIds = questionVM.Options
                                        .Where(o => o.AnswerOptionId > 0)
                                        .Select(o => o.AnswerOptionId)
                                        .ToList();

                                    var optionsToDelete = existingOptionIds.Except(submittedOptionIds).ToList();
                                    foreach (var optionId in optionsToDelete)
                                    {
                                        var optionToDelete = question.AnswerOptions.FirstOrDefault(o => o.AnswerOptionId == optionId);
                                        if (optionToDelete != null)
                                        {
                                            _context.AnswerOptions.Remove(optionToDelete);
                                        }
                                    }

                                    int optionOrder = 0;
                                    foreach (var optionVM in questionVM.Options)
                                    {
                                        if (optionVM.AnswerOptionId > 0)
                                        {
                                            var existingOption = question.AnswerOptions.FirstOrDefault(o => o.AnswerOptionId == optionVM.AnswerOptionId);
                                            if (existingOption != null)
                                            {
                                                existingOption.OptionText = optionVM.OptionText;
                                                existingOption.Order = optionOrder++;
                                            }
                                        }
                                        else
                                        {
                                            var newOption = new AnswerOption
                                            {
                                                QuestionId = question.QuestionId,
                                                OptionText = optionVM.OptionText,
                                                Order = optionOrder++
                                            };
                                            _context.AnswerOptions.Add(newOption);
                                        }
                                    }
                                }
                                else
                                {
                                    if (question.AnswerOptions.Any())
                                    {
                                        _context.AnswerOptions.RemoveRange(question.AnswerOptions);
                                    }
                                }
                            }
                        }
                        else
                        {
                            question = new Question
                            {
                                SurveyId = survey.SurveyId,
                                QuestionText = questionVM.QuestionText,
                                QuestionType = questionVM.QuestionType,
                                IsRequired = questionVM.IsRequired,
                                Order = questionOrder++
                            };

                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync();       

                            if ((question.QuestionType == QuestionType.SingleChoice ||
                                 question.QuestionType == QuestionType.MultipleChoice) &&
                                 questionVM.Options != null)
                            {
                                int optionOrder = 0;
                                foreach (var optionVM in questionVM.Options)
                                {
                                    var option = new AnswerOption
                                    {
                                        QuestionId = question.QuestionId,
                                        OptionText = optionVM.OptionText,
                                        Order = optionOrder++
                                    };
                                    _context.AnswerOptions.Add(option);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Survey '{survey.Title}' has been updated successfully!";

                return RedirectToPage("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating survey");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the survey. Please try again.");
                return Page();
            }
        }
    }
}