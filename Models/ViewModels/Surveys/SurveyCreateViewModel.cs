using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VoxPopuli.Models.ViewModels.Questions;

namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class SurveyCreateViewModel
    {
        public SurveyCreateViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
            CreatorUserId = string.Empty;
            Questions = new List<QuestionViewModel>();
        }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public string CreatorUserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AllowAnonymous { get; set; }

        public string? Password { get; set; }

        public List<QuestionViewModel> Questions { get; set; }
    }

    public class SurveyEditViewModel
    {
        public SurveyEditViewModel()
        {
            Title = string.Empty;
            Description = string.Empty;
            Questions = new List<QuestionViewModel>();
        }

        public int SurveyId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public bool AllowAnonymous { get; set; }

        public string? Password { get; set; }

        public List<QuestionViewModel> Questions { get; set; }
    }
}
