using System.ComponentModel.DataAnnotations;

namespace VoxPopuli.Models.ViewModels.Responses
{
    public class SurveySubmissionViewModel
    {
        public int SurveyId { get; set; }
        public List<AnswerSubmissionViewModel> Answers { get; set; } = new();
        public bool IsAnonymous { get; set; }
        public string? Password { get; set; }
    }
}
