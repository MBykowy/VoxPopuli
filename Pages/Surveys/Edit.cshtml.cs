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
using VoxPopuli.Models.ViewModels.Questions; // Added missing namespace for QuestionViewModel
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
            // Retrieve the survey with all related data
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                return NotFound();
            }

            // Security check - only allow the survey creator or admin to edit
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized access attempt to edit survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }
            // In the Survey property initialization
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


            // Map each question and its options
            foreach (var question in survey.Questions.OrderBy(q => q.Order))
            {
                // When creating a new QuestionViewModel
                var questionVM = new VoxPopuli.Models.ViewModels.Questions.QuestionViewModel
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    IsRequired = question.IsRequired,
                    Order = question.Order,
                    Options = new List<VoxPopuli.Models.ViewModels.Questions.AnswerOptionViewModel>()
                };

                // Map options for choice questions
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

            // Retrieve the original survey
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                return NotFound();
            }

            // Security check - only allow the survey creator or admin to edit
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized update attempt for survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }

            try
            {
                // Update basic survey properties
                survey.Title = Survey.Title;
                survey.Description = Survey.Description ?? string.Empty;
                survey.StartDate = Survey.StartDate;
                survey.EndDate = Survey.EndDate;
                survey.IsActive = Survey.IsActive;
                survey.AllowAnonymous = Survey.AllowAnonymous;

                // Update password if provided
                if (!string.IsNullOrEmpty(Survey.Password))
                {
                    survey.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Survey.Password);
                }

                // Process questions
                if (Survey.Questions != null)
                {
                    var existingQuestionIds = survey.Questions.Select(q => q.QuestionId).ToList();
                    var submittedQuestionIds = Survey.Questions
                        .Where(q => q.QuestionId > 0)
                        .Select(q => q.QuestionId)
                        .ToList();

                    // Find questions to delete (in existing but not in submitted)
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

                    // Process each question in the submitted form
                    int questionOrder = 0;
                    foreach (var questionVM in Survey.Questions)
                    {
                        Question question;

                        if (questionVM.QuestionId > 0)
                        {
                            // Update existing question
                            question = survey.Questions.FirstOrDefault(q => q.QuestionId == questionVM.QuestionId);
                            if (question != null)
                            {
                                question.QuestionText = questionVM.QuestionText;
                                question.QuestionType = questionVM.QuestionType;
                                question.IsRequired = questionVM.IsRequired;
                                question.Order = questionOrder++;

                                // Update options for choice questions
                                if ((question.QuestionType == QuestionType.SingleChoice ||
                                     question.QuestionType == QuestionType.MultipleChoice) &&
                                     questionVM.Options != null)
                                {
                                    // Remove options not in the updated set
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

                                    // Add or update options
                                    int optionOrder = 0;
                                    foreach (var optionVM in questionVM.Options)
                                    {
                                        if (optionVM.AnswerOptionId > 0)
                                        {
                                            // Update existing option
                                            var existingOption = question.AnswerOptions.FirstOrDefault(o => o.AnswerOptionId == optionVM.AnswerOptionId);
                                            if (existingOption != null)
                                            {
                                                existingOption.OptionText = optionVM.OptionText;
                                                existingOption.Order = optionOrder++;
                                            }
                                        }
                                        else
                                        {
                                            // Add new option
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
                                    // If question type changed from choice to non-choice, remove all options
                                    if (question.AnswerOptions.Any())
                                    {
                                        _context.AnswerOptions.RemoveRange(question.AnswerOptions);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Add new question
                            question = new Question
                            {
                                SurveyId = survey.SurveyId,
                                QuestionText = questionVM.QuestionText,
                                QuestionType = questionVM.QuestionType,
                                IsRequired = questionVM.IsRequired,
                                Order = questionOrder++
                            };

                            _context.Questions.Add(question);
                            await _context.SaveChangesAsync(); // Save to get ID for options

                            // Add options for choice questions
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

                // Add success message to TempData
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