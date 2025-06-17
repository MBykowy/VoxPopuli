using System;

namespace VoxPopuli.Models.ViewModels.Dashboard
{
    public class TopSurveyViewModel
    {
        public string Title { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public int CompletionRate { get; set; }
    }
}