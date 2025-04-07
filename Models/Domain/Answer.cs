using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxPopuli.Models.Domain
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int ResponseId { get; set; }

        [ForeignKey("ResponseId")]
        public Response Response { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; }

        public int? SelectedOptionId { get; set; }

        [ForeignKey("SelectedOptionId")]
        public AnswerOption SelectedOption { get; set; }

        [StringLength(2000)]
        public string AnswerText { get; set; }

        public int? RatingValue { get; set; }
    }
}
