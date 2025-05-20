using System.ComponentModel.DataAnnotations;

// Models/ViewModels/Responses/SurveySubmissionViewModel.cs
namespace VoxPopuli.Models.ViewModels.Responses
{
    public class SurveySubmissionViewModel
    {
        // Change init to set
        public int SurveyId { get; set; }
        public List<AnswerSubmissionViewModel> Answers { get; set; } = new();
        public bool IsAnonymous { get; set; }
        public string? Password { get; set; }
    }
}
