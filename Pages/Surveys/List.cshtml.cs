﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoxPopuli.Data;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Pages.Surveys
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ListModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SurveyListItemViewModel> Surveys { get; set; } = new List<SurveyListItemViewModel>();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = User.IsInRole("Admin")
                ? _context.Surveys
                : _context.Surveys.Where(s => s.CreatorUserId == userId);

            Surveys = await query
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SurveyListItemViewModel
                {
                    SurveyId = s.SurveyId,
                    Title = s.Title,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    ResponseCount = s.Responses.Count,
                    QuestionCount = s.Questions.Count
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var survey = await _context.Surveys.FindAsync(id);

            if (survey == null)
            {
                return NotFound();
            }

            if (survey.CreatorUserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            StatusMessage = $"Survey '{survey.Title}' has been deleted.";
            return RedirectToPage();
        }
    }
}
