using Microsoft.EntityFrameworkCore;
using Seatgenie.Api.Repositories;

namespace Seatgenie.Api.Data;

/// <summary>Wires up the EF Core context (Azure PostgreSQL) and the repository layer.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SeatGenieDb");

        services.AddDbContext<SeatGenieDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<IFloorRepository, FloorRepository>();
        services.AddScoped<IMeetingRoomRepository, MeetingRoomRepository>();
        services.AddScoped<IOfficeRoomRepository, OfficeRoomRepository>();
        services.AddScoped<IDeskRepository, DeskRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IRecommendationRepository, RecommendationRepository>();
        services.AddScoped<IOnboardingRepository, OnboardingRepository>();

        return services;
    }
}
