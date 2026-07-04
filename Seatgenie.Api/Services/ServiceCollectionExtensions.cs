using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Services;

/// <summary>Registers the service layer, its current-user accessor and analytics repository.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        // Register HttpClient for booking notifications
        services.AddHttpClient("BookingNotification", client =>
        {
            // Configure base address (should be set from configuration in production)
            // client.BaseAddress = new Uri("https://api.notification-service.com");
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "SeatGenie-API");
            client.DefaultRequestHeaders.Add("seatGenie-Api-Key", "seatgenie1234");
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Recommended: connection pooling
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            MaxConnectionsPerServer = 10
        });

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
