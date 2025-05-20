using System;

namespace VoxPopuli.Models.ViewModels.Dashboard
{
    public class RecentActivityViewModel
    {
        public string SurveyTitle { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ActionLink { get; set; } = string.Empty;
    }
}
