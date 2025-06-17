using AutoMapper;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Questions;
using VoxPopuli.Models.ViewModels.Responses;
using VoxPopuli.Models.ViewModels.Surveys;
using System.Linq;

namespace VoxPopuli.Mappings
{
    public class ResponseMappingProfile : Profile
    {
        public ResponseMappingProfile()
        {
            CreateMap<Survey, TakeSurveyViewModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions.OrderBy(q => q.Order)))
                .ForMember(dest => dest.IsPasswordProtected, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PasswordHash)))
                .ForMember(dest => dest.HasStarted, opt => opt.MapFrom(src => !src.StartDate.HasValue || src.StartDate.Value <= DateTime.UtcNow))
                .ForMember(dest => dest.HasEnded, opt => opt.MapFrom(src => src.EndDate.HasValue && src.EndDate.Value < DateTime.UtcNow));

            CreateMap<Survey, SurveyResultViewModel>()
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses != null ? src.Responses.Count : 0))
                .ForMember(dest => dest.Questions, opt => opt.Ignore());         

            CreateMap<Question, QuestionResultViewModel>()
                .ForMember(dest => dest.Options, opt => opt.Ignore())
                .ForMember(dest => dest.TextResponses, opt => opt.Ignore())
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                .ForMember(dest => dest.RatingDistribution, opt => opt.Ignore())
                .ForMember(dest => dest.ChartData, opt => opt.Ignore());

            CreateMap<Response, ResponseViewModel>()
                .ForMember(dest => dest.SurveyTitle, opt => opt.MapFrom(src => src.Survey.Title))
                .ForMember(dest => dest.RespondentName, opt => opt.MapFrom(src =>
                    src.IsAnonymous ? "Anonymous" :
                    (src.Respondent != null ? src.Respondent.UserName : "Unknown")));
        }
    }

    public class ResponseViewModel
    {
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = string.Empty;
        public string RespondentName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}
