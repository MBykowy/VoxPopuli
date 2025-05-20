namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class SurveyListItemViewModel
        {
            public SurveyListItemViewModel()
            {
                Title = string.Empty;
                Status = string.Empty;
                Description = string.Empty;
            }

            public int SurveyId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Status { get; set; }
            public bool IsActive { get; set; }
            public int ResponseCount { get; set; }
            public int QuestionCount { get; set; }
        }
    }


