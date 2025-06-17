using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Dashboard;

namespace VoxPopuli.Pages.Surveys
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Summary metrics
        public int TotalSurveys { get; set; }
        public int TotalResponses { get; set; }
        public int ActiveSurveys { get; set; }
        public decimal AvgResponseRate { get; set; }

        // Chart data
        public List<string> TrendLabels { get; set; } = new List<string>();
        public List<int> TrendData { get; set; } = new List<int>();
        public List<string> TopSurveyLabels { get; set; } = new List<string>();
        public List<int> TopSurveyData { get; set; } = new List<int>();
        public List<string> DistributionLabels { get; set; } = new List<string>();
        public List<int> DistributionData { get; set; } = new List<int>();
        public List<double> SentimentCurrentData { get; set; } = new List<double>();
        public List<double> SentimentPreviousData { get; set; } = new List<double>();

        // Recent activity
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
        // Add this method to your DashboardModel class

        public async Task<IActionResult> OnPostExportDashboardPdfAsync()
        {
            // First load all the data we need
            await OnGetAsync();

            var pdfService = HttpContext.RequestServices.GetRequiredService<VoxPopuli.Services.PDF.PdfExportService>();

            // Create the list of top surveys with correct type (Dashboard ViewModel)
            var topSurveys = TopSurveyLabels.Select((title, index) => new TopSurveyViewModel
            {
                Title = title,
                ResponseCount = TopSurveyData[index],
                CompletionRate = 80 // Example value, adjust as needed or calculate from your data
            }).ToList();

            // Convert RecentActivities to the required Dashboard namespace type
            var recentActivitiesForPdf = RecentActivities.Select(a => new VoxPopuli.Models.ViewModels.Dashboard.RecentActivityViewModel
            {
                SurveyTitle = a.SurveyTitle,
                ActivityType = a.ActivityType,
                Timestamp = a.Timestamp,
                Username = a.Username,
                ActionLink = a.ActionLink
            }).ToList();

            // Generate the PDF
            byte[] pdfBytes = pdfService.GenerateAnalyticsDashboardPdf(
                TotalSurveys,
                TotalResponses,
                ActiveSurveys,
                AvgResponseRate,
                topSurveys,
                recentActivitiesForPdf); // Using our converted list

            // Return as a downloadable file
            return File(
                pdfBytes,
                "application/pdf",
                $"Survey-Analytics-Dashboard-{DateTime.Now:yyyyMMdd}.pdf");
        }
        public async Task<IActionResult> OnGetAsync(string period = "monthly")
        {
            // Calculate date ranges based on period
            DateTime startDate;
            DateTime endDate = DateTime.Today;
            List<DateTime> datePoints = new List<DateTime>();

            switch (period.ToLower())
            {
                case "weekly":
                    startDate = DateTime.Today.AddDays(-7);
                    for (int i = 0; i < 7; i++)
                    {
                        datePoints.Add(startDate.AddDays(i));
                        TrendLabels.Add(startDate.AddDays(i).ToString("dd MMM"));
                    }
                    break;
                case "yearly":
                    startDate = DateTime.Today.AddMonths(-12);
                    for (int i = 0; i < 12; i++)
                    {
                        datePoints.Add(startDate.AddMonths(i));
                        TrendLabels.Add(startDate.AddMonths(i).ToString("MMM yy"));
                    }
                    break;
                case "monthly":
                default:
                    startDate = DateTime.Today.AddDays(-30);
                    // Create 10 data points over the period
                    for (int i = 0; i < 10; i++)
                    {
                        var point = startDate.AddDays(i * 3);
                        datePoints.Add(point);
                        TrendLabels.Add(point.ToString("dd MMM"));
                    }
                    break;
            }

            // Get summary metrics
            TotalSurveys = await _context.Surveys.CountAsync();
            TotalResponses = await _context.Responses.CountAsync();
            ActiveSurveys = await _context.Surveys
                .Where(s => s.IsActive && (s.EndDate == null || s.EndDate >= DateTime.Today))
                .CountAsync();

            // Calculate average response rate
            var surveyWithResponses = await _context.Surveys
                .Include(s => s.Responses)
                .ToListAsync();

            if (surveyWithResponses.Any())
            {
                AvgResponseRate = (decimal)surveyWithResponses.Average(s => s.Responses.Count);
                AvgResponseRate = Math.Round(AvgResponseRate, 1);
            }

            // Trend data - responses over time
            foreach (var date in datePoints)
            {
                var nextDate = period == "yearly" ? date.AddMonths(1) : date.AddDays(1);
                var count = await _context.Responses
                    .Where(r => r.SubmittedAt >= date && r.SubmittedAt < nextDate)
                    .CountAsync();

                TrendData.Add(count);
            }

            // Top performing surveys
            var topSurveys = await _context.Surveys
                .Include(s => s.Responses)
                .OrderByDescending(s => s.Responses.Count)
                .Take(5)
                .Select(s => new { s.Title, Count = s.Responses.Count })
                .ToListAsync();

            TopSurveyLabels = topSurveys.Select(s => s.Title.Length > 25 ? s.Title.Substring(0, 22) + "..." : s.Title).ToList();
            TopSurveyData = topSurveys.Select(s => s.Count).ToList();

            // Response distribution by question type
            var allQuestions = await _context.Questions.ToListAsync();
            var singleChoiceCount = allQuestions.Count(q => q.QuestionType == QuestionType.SingleChoice);
            var multiChoiceCount = allQuestions.Count(q => q.QuestionType == QuestionType.MultipleChoice);
            var textCount = allQuestions.Count(q => q.QuestionType == QuestionType.Text);
            var ratingCount = allQuestions.Count(q => q.QuestionType == QuestionType.Rating);

            DistributionLabels = new List<string> { "Single Choice", "Multiple Choice", "Text", "Rating" };
            DistributionData = new List<int> { singleChoiceCount, multiChoiceCount, textCount, ratingCount };

            // Sentiment analysis mock data (would require NLP services for real implementation)
            // Current month data
            SentimentCurrentData = new List<double> { 65, 45, 32, 18, 12 };
            // Previous month data
            SentimentPreviousData = new List<double> { 54, 48, 38, 22, 10 };

            // Recent activities
            RecentActivities = await _context.Responses
                .Include(r => r.Survey)
                .OrderByDescending(r => r.SubmittedAt)
                .Take(10)
                .Select(r => new RecentActivityViewModel
                {
                    SurveyTitle = r.Survey.Title,
                    ActivityType = "Survey Response",
                    Timestamp = r.SubmittedAt,
                    Username = r.IsAnonymous ? "Anonymous" : r.RespondentUserId,
                    ActionLink = "/Surveys/Results?id=" + r.SurveyId
                })
                .ToListAsync();

            return Page();
        }
    }

    public class RecentActivityViewModel
    {
        public string SurveyTitle { get; set; }
        public string ActivityType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string ActionLink { get; set; }
    }
}
