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

        public List<OptionResultViewModel> Options { get; set; }

        public List<string> TextResponses { get; set; }

        public double? AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; }

        public ChartDataViewModel ChartData { get; set; }
    }
}
