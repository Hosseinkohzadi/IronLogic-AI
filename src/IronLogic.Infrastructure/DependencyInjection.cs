using IronLogic.Application.Mappers;
using IronLogic.Application.Services;
using IronLogic.Domain.Interfaces;
using IronLogic.Infrastructure.ExternalServices;
using IronLogic.Infrastructure.Repositories;
using IronLogic.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IronLogic.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    ///     Registers all Infrastructure-layer services: EF Core, repositories,
    ///     domain services, mappers, and external providers.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddRepositories()
            .AddDomainServices()
            .AddExternalProviders();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=ironlogic.db";

        services.AddDbContextPool<AppDbContext>(options =>
            options.UseSqlite(connectionString)
                .EnableSensitiveDataLogging(false));

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutAnalysisService, WorkoutAnalysisService>();
        services.AddScoped<ICoachService, CoachService>();
        services.AddScoped<IDailyWeightService, DailyWeightService>();
        services.AddScoped<IMuscleMeasurementService, MuscleMeasurementService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IWorkoutAnalyticsService, WorkoutAnalyticsService>();

        services.AddScoped<BodybuildingCoachPlugin>();
        services.AddSingleton<IHevyParserService, HevyCsvParserService>();
        services.AddSingleton<IHevyDataMapper, HevyDataMapper>();

        return services;
    }

    private static IServiceCollection AddExternalProviders(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutProvider, MockHevyWorkoutProvider>();
        services.AddScoped<IBodyMetricsProvider, BodyMetricsProvider>();

        return services;
    }
}