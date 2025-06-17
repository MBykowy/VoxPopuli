using System;
using System.Collections.Generic;
using VoxPopuli.Models.ViewModels.Responses;

namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class SurveyResponsesViewModel
    {
        public int SurveyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ResponseCount { get; set; }

        public int CompletionRate { get; set; } = 100;   
        public string AvgCompletionTime { get; set; } = "2.5";   
        public List<ResponseDateCount> ResponseDateCounts { get; set; } = new List<ResponseDateCount>();


        public List<ResponseDetailViewModel> Responses { get; set; } = new List<ResponseDetailViewModel>();
    }
    public class ResponseDateCount
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }
    public class ResponseDetailViewModel
    {
        public int ResponseId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string RespondentName { get; set; }
        public bool IsAnonymous { get; set; }
        public List<ResponseAnswerViewModel> Answers { get; set; } = new List<ResponseAnswerViewModel>();
    }

    public class ResponseAnswerViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public int? Rating { get; set; }
        public string SelectedOptionText { get; set; }
    }
}