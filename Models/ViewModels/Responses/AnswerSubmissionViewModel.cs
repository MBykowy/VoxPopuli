// Models/ViewModels/Responses/AnswerSubmissionViewModel.cs
namespace VoxPopuli.Models.ViewModels.Responses
{
    public class AnswerSubmissionViewModel
    {
        public int QuestionId { get; set; }
        public VoxPopuli.Models.Domain.QuestionType QuestionType { get; set; }
        public int? SelectedOptionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public string TextAnswer { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public bool IsRequired { get; set; }

        // Add this method that was missing
        public bool HasAnswer()
        {
            return QuestionType switch
            {
                VoxPopuli.Models.Domain.QuestionType.SingleChoice => SelectedOptionId.HasValue,
                VoxPopuli.Models.Domain.QuestionType.MultipleChoice => SelectedOptionIds.Any(),
                VoxPopuli.Models.Domain.QuestionType.Text => !string.IsNullOrWhiteSpace(TextAnswer),
                VoxPopuli.Models.Domain.QuestionType.Rating => Rating.HasValue,
                _ => false
            };
        }
    }
}
