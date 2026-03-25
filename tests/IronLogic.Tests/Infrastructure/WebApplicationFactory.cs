using IronLogic.Application.Interfaces;
using IronLogic.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IronLogic.Tests.Infrastructure;

/// <summary>
///     Custom WebApplicationFactory that replaces the real SQLite database
///     with an EF Core InMemory database for isolated integration tests.
///     Also replaces MockHevyWorkoutProvider with a database-backed provider
///     so that seeded data is reflected in the /stats endpoint.
/// </summary>
public class WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove every registration that touches AppDbContext or its options.
            // AddDbContextPool registers several internal types - nuke them all.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ImplementationType is { FullName: not null } &&
                     d.ImplementationType.FullName.Contains(nameof(AppDbContext))) ||
                    (d.ServiceType is { FullName: not null } &&
                     d.ServiceType.FullName.Contains(nameof(AppDbContext))))
                .ToList();

            foreach (var d in toRemove) services.Remove(d);

            // Re-register with InMemory provider (non-pooled)
            var dbName = "IronLogicTestDb_" + Guid.NewGuid();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Replace MockHevyWorkoutProvider with a database-backed provider
            // so integration tests that seed the DB get consistent /stats results.
            var workoutProviderDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(IWorkoutProvider));

            if (workoutProviderDescriptor is not null)
                services.Remove(workoutProviderDescriptor);

            services.AddScoped<IWorkoutProvider, DatabaseWorkoutProvider>();
        });
    }
}