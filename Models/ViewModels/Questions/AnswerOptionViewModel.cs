using System.ComponentModel.DataAnnotations;

namespace VoxPopuli.Models.ViewModels.Questions
{
    public class AnswerOptionViewModel
    {
        public AnswerOptionViewModel()
        {
            OptionText = string.Empty;
        }

        public int AnswerOptionId { get; set; }

        [Required(ErrorMessage = "Option text is required")]
        [StringLength(500, ErrorMessage = "Option text cannot exceed 500 characters")]
        [Display(Name = "Option")]
        public string OptionText { get; set; }

        [Required]
        [Display(Name = "Order")]
        public int Order { get; set; }
    }

}
