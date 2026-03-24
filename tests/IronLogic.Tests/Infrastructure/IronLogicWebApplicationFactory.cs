using IronLogic.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IronLogic.Tests.Infrastructure;

/// <summary>
///     Custom WebApplicationFactory that replaces the real SQLite database
///     with an EF Core InMemory database for isolated integration tests.
/// </summary>
public class IronLogicWebApplicationFactory : WebApplicationFactory<Program>
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
                    (d.ImplementationType != null &&
                     d.ImplementationType.FullName != null &&
                     d.ImplementationType.FullName.Contains(nameof(AppDbContext))) ||
                    (d.ServiceType.FullName != null &&
                     d.ServiceType.FullName.Contains(nameof(AppDbContext))))
                .ToList();

            foreach (var d in toRemove) services.Remove(d);

            // Re-register with InMemory provider (non-pooled)
            var dbName = "IronLogicTestDb_" + Guid.NewGuid();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}