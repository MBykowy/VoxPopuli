using AutoMapper;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Questions;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Mappings
{
    public class SurveyMappingProfile : Profile
    {
        public SurveyMappingProfile()
        {
            CreateMap<Question, QuestionViewModel>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.AnswerOptions));

            CreateMap<QuestionViewModel, Question>()
                .ForMember(dest => dest.SurveyId, opt => opt.Ignore())
                .ForMember(dest => dest.Survey, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.Ignore());

            CreateMap<AnswerOption, AnswerOptionViewModel>();

            CreateMap<AnswerOptionViewModel, AnswerOption>()
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.Ignore());

            CreateMap<Survey, SurveyCreateViewModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.Password, opt => opt.Ignore());        

            CreateMap<SurveyCreateViewModel, Survey>()
                .ForMember(dest => dest.SurveyId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))     
                .ForMember(dest => dest.Responses, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());       

            CreateMap<Survey, SurveyEditViewModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.Password, opt => opt.Ignore());        

            CreateMap<SurveyEditViewModel, Survey>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))        
                .ForMember(dest => dest.Responses, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());       

            CreateMap<Survey, SurveyListItemViewModel>()
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses != null ? src.Responses.Count : 0))
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetSurveyStatus(src)));

            CreateMap<Survey, SurveyDetailsViewModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses != null ? src.Responses.Count : 0))
                .ForMember(dest => dest.IsPasswordProtected, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PasswordHash)));
        }

        private string GetSurveyStatus(Survey survey)
        {
            if (!survey.IsActive)
                return "Inactive";

            var now = DateTime.UtcNow;

            if (survey.StartDate.HasValue && survey.StartDate > now)
                return "Scheduled";

            if (survey.EndDate.HasValue && survey.EndDate < now)
                return "Expired";

            return "Active";
        }
    }
}
