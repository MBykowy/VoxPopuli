using VoxPopuli.Models.Domain;

namespace VoxPopuli.Models.ViewModels.Responses
{
    public class AnswerSubmissionViewModel
    {
        public int QuestionId { get; set; }
        public QuestionType QuestionType { get; set; }
        public int? SelectedOptionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public string? TextAnswer { get; set; }
        public int? Rating { get; set; }
        public bool IsRequired { get; set; }

        public bool HasAnswer()
        {
            return QuestionType switch
            {
                QuestionType.SingleChoice => SelectedOptionId.HasValue,
                QuestionType.MultipleChoice => SelectedOptionIds != null && SelectedOptionIds.Any(),
                QuestionType.Text => !string.IsNullOrWhiteSpace(TextAnswer),
                QuestionType.Rating => Rating.HasValue,
                _ => false
            };
        }
    }
}