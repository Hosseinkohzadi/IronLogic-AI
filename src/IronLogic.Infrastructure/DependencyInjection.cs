using IronLogic.Application.Interfaces;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Repositories;
using IronLogic.Infrastructure.Services;
using IronLogic.Infrastructure.Services.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IronLogic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration)
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
                .LogTo(Console.WriteLine, LogLevel.Information) // نمایش کوئری‌ها در کنسول
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutImportService, WorkoutImportService>();
        services.AddScoped<IWorkoutParserService, WorkoutParserService>();
        services.AddScoped<IWorkoutService, WorkoutService>();

        return services;
    }

    private static void AddExternalProviders(this IServiceCollection services)
    {
    }
}