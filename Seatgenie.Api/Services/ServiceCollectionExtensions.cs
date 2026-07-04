using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

/// <summary>Registers the service layer, its current-user accessor and analytics repository.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        // Analytics has its own read-only repository (not part of the generic CRUD set).
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IOfficeSettingsService, OfficeSettingsService>();
        services.AddScoped<IFloorService, FloorService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IDeskService, DeskService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChatbotService, ChatbotService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IUserSeatPreferenceService, UserSeatPreferenceService>();
        services.AddScoped<IDeskQualityService, DeskQualityService>();

        return services;
    }
}
