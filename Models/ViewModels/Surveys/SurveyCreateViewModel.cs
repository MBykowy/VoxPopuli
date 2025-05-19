// These are placeholder implementations to make the mapping profiles work.
// You should implement these properly according to your requirements.

namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class SurveyCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatorUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool AllowAnonymous { get; set; }
        public string Password { get; set; } // Plain text password that will be hashed
        public List<ViewModels.Questions.QuestionViewModel> Questions { get; set; } = new List<ViewModels.Questions.QuestionViewModel>();
    }

    public class SurveyEditViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool AllowAnonymous { get; set; }
        public string Password { get; set; } // Optional - only set if changing password
        public List<ViewModels.Questions.QuestionViewModel> Questions { get; set; } = new List<ViewModels.Questions.QuestionViewModel>();
    }

    public class SurveyListItemViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int ResponseCount { get; set; }
        public int QuestionCount { get; set; }
    }

    public class SurveyDetailsViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool AllowAnonymous { get; set; }
        public bool IsPasswordProtected { get; set; }
        public int ResponseCount { get; set; }
        public List<ViewModels.Questions.QuestionViewModel> Questions { get; set; } = new List<ViewModels.Questions.QuestionViewModel>();
    }

    public class SurveyResultViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ResponseCount { get; set; }
        public List<QuestionResultViewModel> Questions { get; set; } = new List<QuestionResultViewModel>();
    }

    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public VoxPopuli.Models.Domain.QuestionType QuestionType { get; set; }
        public List<OptionResultViewModel> Options { get; set; } = new List<OptionResultViewModel>();
        public List<string> TextResponses { get; set; } = new List<string>();
        public double? AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>();
    }

    public class OptionResultViewModel
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}

namespace VoxPopuli.Models.ViewModels.Responses
{
    public class TakeSurveyViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsAnonymous { get; set; }
        public List<ViewModels.Questions.QuestionViewModel> Questions { get; set; } = new List<ViewModels.Questions.QuestionViewModel>();
        public string Password { get; set; } // Used for password-protected surveys
    }

    public class ResponseViewModel
    {
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public string RespondentEmail { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class AnswerSubmissionViewModel
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; } // For single choice
        public List<int> SelectedOptionIds { get; set; } = new List<int>(); // For multiple choice
        public string TextAnswer { get; set; } // For text questions
        public int? Rating { get; set; } // For rating questions
    }
}
