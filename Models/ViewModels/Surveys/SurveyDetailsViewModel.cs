using VoxPopuli.Models.ViewModels.Questions;

namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class SurveyDetailsViewModel
    {
        public SurveyDetailsViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
            Questions = new List<QuestionViewModel>();
        }

        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool AllowAnonymous { get; set; }
        public bool IsPasswordProtected { get; set; }
        public int ResponseCount { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
    }
}
