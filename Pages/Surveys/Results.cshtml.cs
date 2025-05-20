// Pages/Surveys/Results.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Questions;
using VoxPopuli.Models.ViewModels.Responses;
using VoxPopuli.Models.ViewModels.Surveys;


namespace VoxPopuli.Pages.Surveys
{
    public class ResultsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ResultsModel> _logger;

        public ResultsModel(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ResultsModel> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SurveyResultViewModel? SurveyResult { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Accessing results for survey ID: {SurveyId}", Id);

            var survey = await _context.Surveys
                .Include(s => s.Questions).ThenInclude(q => q.AnswerOptions)
                .Include(s => s.Responses).ThenInclude(r => r.Answers)
                .FirstOrDefaultAsync(s => s.SurveyId == Id);

            if (survey == null)
            {
                _logger.LogWarning("Survey with ID {SurveyId} not found", Id);
                return NotFound();
            }

            // Check if the current user is allowed to view results
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (survey.CreatorUserId != currentUserId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("Unauthorized access attempt to results of survey {SurveyId} by user {UserId}", Id, currentUserId);
                return Forbid();
            }

            // In your Results.cshtml.cs OnGetAsync method
            SurveyResult = _mapper.Map<SurveyResultViewModel>(survey);

            if (SurveyResult == null)
            {
                SurveyResult = new SurveyResultViewModel
                {
                    Title = survey.Title,
                    Description = survey.Description,
                    ResponseCount = 0,
                    Questions = new List<QuestionResultViewModel>()
                };
            }

            // Make sure your ModelState is valid
            if (!ModelState.IsValid)
            {
                // Log any validation errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError(error.ErrorMessage);
                    }
                }
            }

            // Process and aggregate answers for each question
            foreach (var question in survey.Questions.OrderBy(q => q.Order))
            {
                // Logic to process different question types and aggregate results
                // (Move the processing logic from your controller here)
            }

            return Page();
        }
    }
}
