using System.ComponentModel.DataAnnotations;
using VoxPopuli.Models.Domain;

namespace VoxPopuli.Models.ViewModels.Questions
{
    public class QuestionViewModel
    {
        public QuestionViewModel()
        {
            QuestionText = string.Empty;
            Options = new List<AnswerOptionViewModel>();
        }

        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        [StringLength(1000, ErrorMessage = "Question text cannot exceed 1000 characters")]
        [Display(Name = "Question")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        [Display(Name = "Question Type")]
        public QuestionType QuestionType { get; set; }

        [Required]
        [Display(Name = "Order")]
        public int Order { get; set; }

        [Display(Name = "Required")]
        public bool IsRequired { get; set; }

        // For SingleChoice and MultipleChoice question types
        public List<AnswerOptionViewModel> Options { get; set; }
    }

}
