using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VoxPopuli.Models.Domain
{
    public class Response
    {
        [Key]
        public int ResponseId { get; set; }

        [Required]
        public int SurveyId { get; set; }

        [ForeignKey("SurveyId")]
        public Survey Survey { get; set; }

        public string RespondentUserId { get; set; }

        [ForeignKey("RespondentUserId")]
        public IdentityUser Respondent { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public bool IsAnonymous { get; set; }

        // Navigation properties
        public virtual ICollection<Answer> Answers { get; set; }
    }
}
