using IronLogic.Application.Interfaces;
using IronLogic.Application.Mappers;
using IronLogic.Infrastructure;
using IronLogic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register parser and mapper
services.AddSingleton<IHevyParserService, HevyCsvParserService>();
services.AddSingleton<IHevyDataMapper, HevyDataMapper>();

// Register DbContext with pooling to improve connection/instance reuse for high-throughput scenarios
services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlite("Data Source=ironlogic.db")
        // Optional: tune batch size to improve multiple-statement performance
        .EnableSensitiveDataLogging(false)
);

// Build provider and ensure disposal
await using var provider = services.BuildServiceProvider();

Console.WriteLine("--- IronLogic AI: CSV to Hierarchy Test ---");

var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workout_data.csv");

// Resolve services
var parser = provider.GetRequiredService<IHevyParserService>();
var mapper = provider.GetRequiredService<IHevyDataMapper>();

try
{
    // 2. Parse CSV and map to domain hierarchy
    var flatRecords = parser.Parse(filePath);
    var sessions = mapper.MapToSessions(flatRecords);

    Console.WriteLine($"Mapped into {sessions.Count} Workout Sessions. Ready to save...");
    Console.WriteLine(new string('-', 40));

    // 3. Connect to the database safely using DI
    await using var dbContext = provider.GetRequiredService<AppDbContext>();

    // Apply pending migrations (creates DB + all tables if missing)
    dbContext.Database.Migrate();

    // 4. Insert data if the database is currently empty
    if (!dbContext.Sessions.Any())
    {
        Console.WriteLine("Inserting data into SQLite. This might take a few seconds...");

        dbContext.Sessions.AddRange(sessions);
        dbContext.SaveChanges();

        Console.WriteLine("✅ Successfully saved all workout history to the database!");
    }
    else
    {
        var existingCount = dbContext.Sessions.Count();
        Console.WriteLine($"⏩ Database already contains {existingCount} sessions. Skipping insert.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}