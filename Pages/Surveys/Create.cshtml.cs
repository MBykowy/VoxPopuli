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

                // Add debugging for questions and options
                if (Survey.Questions != null)
                {
                    foreach (var q in Survey.Questions)
                    {
                        _logger.LogInformation("Question: {QuestionText}, Type: {QuestionType}, Options Count: {OptionsCount}",
                            q.QuestionText, q.QuestionType, q.Options?.Count ?? 0);

                        if (q.Options != null)
                        {
                            foreach (var opt in q.Options)
                            {
                                _logger.LogInformation("Option: {OptionText}", opt?.OptionText ?? "NULL");
                            }
                        }
                    }
                }
                // Create the survey directly without using AutoMapper
                var surveyEntity = new Survey
                {
                    Title = Survey.Title,
                    Description = Survey.Description ?? string.Empty,
                    CreatorUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    StartDate = Survey.StartDate,
                    EndDate = Survey.EndDate,
                    IsActive = Survey.IsActive,
                    AllowAnonymous = Survey.AllowAnonymous,
                    // Hash password if provided
                    PasswordHash = !string.IsNullOrEmpty(Survey.Password) ?
                        BCrypt.Net.BCrypt.HashPassword(Survey.Password) : null
                };

                // Save the survey first to get its ID
                _context.Surveys.Add(surveyEntity);
                await _context.SaveChangesAsync();

                // Now add questions and their options
                if (Survey.Questions != null && Survey.Questions.Any())
                {
                    int questionOrder = 0;
                    foreach (var questionVM in Survey.Questions)
                    {
                        var question = new Question
                        {
                            SurveyId = surveyEntity.SurveyId,
                            QuestionText = questionVM.QuestionText,
                            QuestionType = questionVM.QuestionType,
                            IsRequired = questionVM.IsRequired,
                            Order = questionOrder++
                        };

                        _context.Questions.Add(question);
                        // Save to get the QuestionId
                        await _context.SaveChangesAsync();

                        // Add options for choice-based questions
                        if ((questionVM.QuestionType == QuestionType.SingleChoice ||
                             questionVM.QuestionType == QuestionType.MultipleChoice) &&
                            questionVM.Options != null && questionVM.Options.Any())
                        {
                            int optionOrder = 0;
                            foreach (var optionVM in questionVM.Options)
                            {
                                var option = new AnswerOption
                                {
                                    QuestionId = question.QuestionId,
                                    OptionText = optionVM.OptionText, // Change from optionVM.Text to optionVM.OptionText
                                    Order = optionOrder++
                                };

                                _context.AnswerOptions.Add(option);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Add success message to TempData
                TempData["SuccessMessage"] = $"Survey '{surveyEntity.Title}' has been created successfully!";
                TempData["NewSurveyId"] = surveyEntity.SurveyId;

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