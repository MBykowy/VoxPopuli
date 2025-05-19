using AutoMapper;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Questions;
using VoxPopuli.Models.ViewModels.Responses;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Mappings
{
    public class ResponseMappingProfile : Profile
    {
        public ResponseMappingProfile()
        {
            // For taking a survey
            CreateMap<Survey, TakeSurveyViewModel>()
                .ForMember(dest => dest.SurveyId, opt => opt.MapFrom(src => src.SurveyId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.IsAnonymous, opt => opt.MapFrom(src => src.AllowAnonymous));

            // For survey responses
            CreateMap<Response, ResponseViewModel>()
                .ForMember(dest => dest.SurveyTitle, opt => opt.MapFrom(src => src.Survey.Title))
                .ForMember(dest => dest.RespondentEmail, opt => opt.MapFrom(src => src.IsAnonymous ? "Anonymous" : src.Respondent.Email));

            // For survey results
            CreateMap<Survey, SurveyResultViewModel>()
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses != null ? src.Responses.Count : 0))
                .ForMember(dest => dest.Questions, opt => opt.Ignore()); // Questions will be mapped separately with aggregated data
        }
    }
}
