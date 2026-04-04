using IronLogic.Infrastructure.Repositories;
using IronLogic.Infrastructure.Services;
using IronLogic.Infrastructure.Services.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; 
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure;

public static class DependencyInjection
{
    // IHostEnvironment parameter added to detect the environment
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment)
            .AddRepositories()
            .AddDomainServices()
            .AddExternalProviders();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string is missing!");

        services.AddDbContextPool<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);

            // Heavy logging is only enabled in Development environment
            if (environment.IsDevelopment())
            {
                options.LogTo(Console.WriteLine, LogLevel.Information)
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors();
            }
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutImportService, WorkoutImportService>();
        services.AddScoped<IWorkoutService, WorkoutService>();

        services.AddSingleton<IWorkoutParserService, WorkoutParserService>();

        return services;
    }

    private static void AddExternalProviders(this IServiceCollection services)
    {
        services.AddSingleton<IMuscleMapperService, MuscleMapperService>();
        services.AddScoped<IExerciseCacheService, ExerciseCacheService>();
        services.AddScoped<IPersonalRecordService, PersonalRecordService>();
        services.AddScoped<IWorkoutPersistenceService, WorkoutPersistenceService>();
    }
}