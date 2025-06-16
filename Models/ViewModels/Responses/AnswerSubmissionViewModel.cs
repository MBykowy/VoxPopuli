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

        /// <summary>
        /// Determines if this answer has a valid response based on its question type
        /// </summary>
        public bool HasAnswer()
        {
            return QuestionType switch
            {
                QuestionType.SingleChoice => SelectedOptionId.HasValue,
                QuestionType.MultipleChoice => SelectedOptionIds != null && SelectedOptionIds.Any(),
                QuestionType.Text => !string.IsNullOrWhiteSpace(TextAnswer),
                QuestionType.Rating => Rating.HasValue,
                // Remove the non-existent enum values and use default for any future additions
                _ => false
            };
        }
    }
}