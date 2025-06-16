using System.Collections.Generic;
using VoxPopuli.Models.Domain;

namespace VoxPopuli.Models.ViewModels.Responses
{
    public class TakeSurveyViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPasswordProtected { get; set; }
        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }
        public bool IsAnonymous { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }
        public bool IsRequired { get; set; }
        public List<AnswerOptionViewModel> Options { get; set; } = new List<AnswerOptionViewModel>();
    }

    public class AnswerOptionViewModel
    {
        public int AnswerOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }
}