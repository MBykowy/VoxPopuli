﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Azure;
using Microsoft.AspNetCore.Identity;

namespace VoxPopuli.Models.Domain
{
    public class Survey
    {
        [Key]
        public int SurveyId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public string CreatorUserId { get; set; }

        [ForeignKey("CreatorUserId")]
        public IdentityUser Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AllowAnonymous { get; set; } = false;

        public string PasswordHash { get; set; }

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Response> Responses { get; set; }
    }
}
