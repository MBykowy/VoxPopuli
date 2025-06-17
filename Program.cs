using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;    
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxPopuli.Data;
using VoxPopuli.Models.Domain;
using VoxPopuli.Models.ViewModels.Questions;
using VoxPopuli.Models.ViewModels.Responses;
using VoxPopuli.Models.ViewModels.Surveys;
using VoxPopuli.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;



Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/voxpopuli-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<VoxPopuli.Services.PDF.PdfExportService>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ISurveyMapper, SurveyMapper>();
builder.Services.AddSingleton<IResponseMapper, ResponseMapper>();

builder.Services.AddSingleton<AutoMapper.IMapper>(sp =>
    new AutoMapperCompatibilityAdapter(
        sp.GetRequiredService<ISurveyMapper>(),
        sp.GetRequiredService<IResponseMapper>()
    )
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


if (app.Environment.IsDevelopment())
{
    await RoleInitializer.InitializeRoles(app.Services);

    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

public interface ISurveyMapper
{
    TakeSurveyViewModel MapToTakeSurveyViewModel(Survey survey);
    SurveyResultViewModel MapToSurveyResultViewModel(Survey survey);
    SurveyDetailsViewModel MapToDetailsViewModel(Survey survey);
    SurveyListItemViewModel MapToListItemViewModel(Survey survey);
    VoxPopuli.Models.ViewModels.Questions.QuestionViewModel MapToViewModel(Question question);
    Survey MapToEntity(SurveyCreateViewModel viewModel);
    Survey MapToEntity(SurveyEditViewModel viewModel);
    Question MapToEntity(VoxPopuli.Models.ViewModels.Questions.QuestionViewModel viewModel);
}
public interface IResponseMapper
{
    VoxPopuli.Mappings.ResponseViewModel MapToViewModel(Response response);
}

public class SurveyMapper : ISurveyMapper
{
    public TakeSurveyViewModel MapToTakeSurveyViewModel(Survey survey)
    {
        if (survey == null) return null;

        var viewModel = new TakeSurveyViewModel
        {
            SurveyId = survey.SurveyId,
            Title = survey.Title,
            Description = survey.Description,
            IsPasswordProtected = !string.IsNullOrEmpty(survey.PasswordHash),
            HasStarted = !survey.StartDate.HasValue || survey.StartDate.Value <= DateTime.UtcNow,
            HasEnded = survey.EndDate.HasValue && survey.EndDate.Value < DateTime.UtcNow
        };

        if (survey.Questions != null)
        {
            viewModel.Questions = survey.Questions
                .OrderBy(q => q.Order)
                .Select(q => new VoxPopuli.Models.ViewModels.Responses.QuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.AnswerOptions
                        .OrderBy(o => o.Order)
                        .Select(ao => new VoxPopuli.Models.ViewModels.Responses.AnswerOptionViewModel
                        {
                            AnswerOptionId = ao.AnswerOptionId,
                            OptionText = ao.OptionText
                        })
                        .ToList()
                })
                .ToList();
        }

        return viewModel;
    }

public SurveyResultViewModel MapToSurveyResultViewModel(Survey survey)
    {
        if (survey == null) return null;

        return new SurveyResultViewModel
        {
            SurveyId = survey.SurveyId,
            Title = survey.Title,
            Description = survey.Description,
            ResponseCount = survey.Responses?.Count ?? 0,
            Questions = new List<QuestionResultViewModel>()
        };
    }

    public SurveyDetailsViewModel MapToDetailsViewModel(Survey survey)
    {
        if (survey == null) return null;

        return new SurveyDetailsViewModel
        {
            SurveyId = survey.SurveyId,
            Title = survey.Title,
            Description = survey.Description,
            IsActive = survey.IsActive,
            StartDate = survey.StartDate,
            EndDate = survey.EndDate,
            IsPasswordProtected = !string.IsNullOrEmpty(survey.PasswordHash),
            ResponseCount = survey.Responses?.Count ?? 0,
            Questions = survey.Questions?.Select(q => this.MapToViewModel(q)).ToList() ??
                new List<VoxPopuli.Models.ViewModels.Questions.QuestionViewModel>()
        };
    }

    public SurveyListItemViewModel MapToListItemViewModel(Survey survey)
    {
        if (survey == null) return null;

        return new SurveyListItemViewModel
        {
            SurveyId = survey.SurveyId,
            Title = survey.Title,
            CreatedAt = survey.CreatedAt,
            Status = GetSurveyStatus(survey),
            ResponseCount = survey.Responses?.Count ?? 0,
            QuestionCount = survey.Questions?.Count ?? 0
        };
    }

    public VoxPopuli.Models.ViewModels.Questions.QuestionViewModel MapToViewModel(Question question)
    {
        if (question == null) return null;

        var viewModel = new VoxPopuli.Models.ViewModels.Questions.QuestionViewModel
        {
            QuestionId = question.QuestionId,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType,
            IsRequired = question.IsRequired,
            Order = question.Order
        };

        if (question.AnswerOptions != null)
        {
            viewModel.Options = question.AnswerOptions
                .OrderBy(o => o.Order)
                .Select(ao => new VoxPopuli.Models.ViewModels.Questions.AnswerOptionViewModel
                {
                    AnswerOptionId = ao.AnswerOptionId,
                    OptionText = ao.OptionText,
                    Order = ao.Order
                })
                .ToList();
        }

        return viewModel;
    }

    public Survey MapToEntity(SurveyCreateViewModel viewModel)
    {
        if (viewModel == null) return null;

        var survey = new Survey
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            IsActive = viewModel.IsActive,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            AllowAnonymous = viewModel.AllowAnonymous
        };

        if (viewModel.Questions != null)
        {
            survey.Questions = viewModel.Questions
                .Select(q => this.MapToEntity(q))
                .ToList();
        }

        return survey;
    }

    public Survey MapToEntity(SurveyEditViewModel viewModel)
    {
        if (viewModel == null) return null;

        var survey = new Survey
        {
            SurveyId = viewModel.SurveyId,
            Title = viewModel.Title,
            Description = viewModel.Description,
            IsActive = viewModel.IsActive,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            AllowAnonymous = viewModel.AllowAnonymous
        };

        if (viewModel.Questions != null)
        {
            survey.Questions = viewModel.Questions
                .Select(q => this.MapToEntity(q))
                .ToList();
        }

        return survey;
    }

    public Question MapToEntity(VoxPopuli.Models.ViewModels.Questions.QuestionViewModel viewModel)
    {
        if (viewModel == null) return null;

        var question = new Question
        {
            QuestionId = viewModel.QuestionId,
            QuestionText = viewModel.QuestionText,
            QuestionType = viewModel.QuestionType,
            IsRequired = viewModel.IsRequired,
            Order = viewModel.Order
        };

        if (viewModel.Options != null)
        {
            question.AnswerOptions = viewModel.Options
                .Select(o => new AnswerOption
                {
                    AnswerOptionId = o.AnswerOptionId,
                    OptionText = o.OptionText,
                    Order = o.Order,
                    QuestionId = question.QuestionId
                })
                .ToList();
        }

        return question;
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

public class ResponseMapper : IResponseMapper
{
    public VoxPopuli.Mappings.ResponseViewModel MapToViewModel(Response response)
    {
        if (response == null) return null;

        return new VoxPopuli.Mappings.ResponseViewModel
        {
            ResponseId = response.ResponseId,
            SurveyId = response.SurveyId,
            SurveyTitle = response.Survey?.Title ?? string.Empty,
            RespondentName = response.IsAnonymous ? "Anonymous" :
                             (response.Respondent?.UserName ?? "Unknown"),
            SubmittedAt = response.SubmittedAt
        };
    }
}

public class AutoMapperCompatibilityAdapter : AutoMapper.IMapper
{
    private readonly ISurveyMapper _surveyMapper;
    private readonly IResponseMapper _responseMapper;

    public AutoMapperCompatibilityAdapter(ISurveyMapper surveyMapper, IResponseMapper responseMapper)
    {
        _surveyMapper = surveyMapper;
        _responseMapper = responseMapper;
    }

    public TDestination Map<TDestination>(object source)
    {
        return Map<object, TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null)
            return default;

        if (source is Survey survey && typeof(TDestination) == typeof(TakeSurveyViewModel))
            return (TDestination)(object)_surveyMapper.MapToTakeSurveyViewModel(survey);

        if (source is Survey surveySR && typeof(TDestination) == typeof(SurveyResultViewModel))
            return (TDestination)(object)_surveyMapper.MapToSurveyResultViewModel(surveySR);

        if (source is Survey surveySD && typeof(TDestination) == typeof(SurveyDetailsViewModel))
            return (TDestination)(object)_surveyMapper.MapToDetailsViewModel(surveySD);

        if (source is Survey surveySL && typeof(TDestination) == typeof(SurveyListItemViewModel))
            return (TDestination)(object)_surveyMapper.MapToListItemViewModel(surveySL);
       
        if (source is Question question && typeof(TDestination) == typeof(VoxPopuli.Models.ViewModels.Questions.QuestionViewModel))
            return (TDestination)(object)_surveyMapper.MapToViewModel(question);

        if (source is Response response && typeof(TDestination) == typeof(VoxPopuli.Mappings.ResponseViewModel))
            return (TDestination)(object)_responseMapper.MapToViewModel(response);

        if (source is SurveyCreateViewModel createVM && typeof(TDestination) == typeof(Survey))
            return (TDestination)(object)_surveyMapper.MapToEntity(createVM);

        if (source is SurveyEditViewModel editVM && typeof(TDestination) == typeof(Survey))
            return (TDestination)(object)_surveyMapper.MapToEntity(editVM);


        if (source is VoxPopuli.Models.ViewModels.Questions.QuestionViewModel questionVM && typeof(TDestination) == typeof(Question))
            return (TDestination)(object)_surveyMapper.MapToEntity(questionVM);

        try
        {
            return Activator.CreateInstance<TDestination>();
        }
        catch
        {
            return default;
        }
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return destination;
    }

    public object Map(object source, Type sourceType, Type destinationType)
    {
        try
        {
            return Activator.CreateInstance(destinationType);
        }
        catch
        {
            return null;
        }
    }

    public object Map(object source, object destination, Type sourceType, Type destinationType)
    {
        return destination;
    }

    public AutoMapper.IConfigurationProvider ConfigurationProvider => null;

    public Func<Type, object> ServiceCtor => type => null;

    public TDestination Map<TDestination>(object source, Action<AutoMapper.IMappingOperationOptions> opts)
    {
        return Map<TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, Action<AutoMapper.IMappingOperationOptions<TSource, TDestination>> opts)
    {
        return Map<TSource, TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<AutoMapper.IMappingOperationOptions<TSource, TDestination>> opts)
    {
        return Map(source, destination);
    }

    public object Map(object source, Type sourceType, Type destinationType, Action<AutoMapper.IMappingOperationOptions> opts)
    {
        return Map(source, sourceType, destinationType);
    }

    public object Map(object source, object destination, Type sourceType, Type destinationType, Action<AutoMapper.IMappingOperationOptions> opts)
    {
        return Map(source, destination, sourceType, destinationType);
    }

    public System.Linq.IQueryable<TDestination> ProjectTo<TDestination>(System.Linq.IQueryable source, object parameters = null, params System.Linq.Expressions.Expression<Func<TDestination, object>>[] membersToExpand)
    {
        throw new NotImplementedException("ProjectTo is not supported in this implementation");
    }

    public System.Linq.IQueryable ProjectTo(System.Linq.IQueryable source, Type destinationType, IDictionary<string, object> parameters = null, params string[] membersToExpand)
    {
        throw new NotImplementedException("ProjectTo is not supported in this implementation");
    }
}