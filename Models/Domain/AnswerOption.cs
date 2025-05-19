using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoxPopuli.Models.Domain
{
    public class AnswerOption
    {
        public AnswerOption()
        {
            OptionText = string.Empty;
            Answers = new List<Answer>();
        }

        [Key]
        public int AnswerOptionId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }

        [Required]
        [StringLength(500)]
        public string OptionText { get; set; }

        [Required]
        public int Order { get; set; }

        // Navigation properties
        public virtual ICollection<Answer> Answers { get; set; }
    }

}
