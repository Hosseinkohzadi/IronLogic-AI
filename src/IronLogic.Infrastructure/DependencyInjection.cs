using System.Text.Json.Serialization;
using IronLogic.Application.Interfaces;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Repositories;
using IronLogic.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IWorkoutImportService, WorkoutImportService>();

        return services;
    }

    private static void AddExternalProviders(this IServiceCollection services)
    {
    }
}