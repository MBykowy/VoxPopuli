using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxPopuli.Models.Domain
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int SurveyId { get; set; }

        [ForeignKey("SurveyId")]
        public Survey Survey { get; set; }

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; }

        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        public int Order { get; set; }

        public bool IsRequired { get; set; } = false;

        // Navigation properties
        public virtual ICollection<AnswerOption> AnswerOptions { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }
}
