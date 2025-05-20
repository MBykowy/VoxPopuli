namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class SurveyResultViewModel
    {
        public SurveyResultViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
            Questions = new List<QuestionResultViewModel>();
        }

        public int SurveyId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int ResponseCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public List<QuestionResultViewModel> Questions { get; set; }
    }
}
