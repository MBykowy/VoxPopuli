using VoxPopuli.Models.Domain;

namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class QuestionResultViewModel
    {
        public QuestionResultViewModel()
        {
            QuestionText = string.Empty;
            Options = new List<OptionResultViewModel>();
            TextResponses = new List<string>();
            RatingDistribution = new Dictionary<int, int>();
        }

        public int QuestionId { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public int Order { get; set; }

        public int ResponseCount { get; set; }

        // For choice-based questions (SingleChoice and MultipleChoice)
        public List<OptionResultViewModel> Options { get; set; }

        // For text questions
        public List<string> TextResponses { get; set; }

        // For rating questions
        public double? AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; }

        // For chart data
        public ChartDataViewModel ChartData { get; set; }
    }
}
