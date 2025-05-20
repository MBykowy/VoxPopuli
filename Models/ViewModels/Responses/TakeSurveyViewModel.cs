// Models/ViewModels/Responses/TakeSurveyViewModel.cs
namespace VoxPopuli.Models.ViewModels.Responses
{
    public class TakeSurveyViewModel
    {
        public TakeSurveyViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
            Questions = new List<VoxPopuli.Models.ViewModels.Questions.QuestionViewModel>();
        }

        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        // Change init to set
        public List<VoxPopuli.Models.ViewModels.Questions.QuestionViewModel> Questions { get; set; }
        public bool IsAnonymous { get; set; }
        public string? Password { get; set; }
        public bool IsPasswordProtected { get; set; }
        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
