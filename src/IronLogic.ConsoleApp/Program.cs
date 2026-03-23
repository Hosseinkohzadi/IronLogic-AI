using IronLogic.Application.Interfaces;
using IronLogic.Application.Mappers;
using IronLogic.Infrastructure;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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
    // 1. Parse CSV and map to domain hierarchy
    var flatRecords = parser.Parse(filePath);
    var sessions = mapper.MapToSessions(flatRecords);

    Console.WriteLine($"Mapped into {sessions.Count} Workout Sessions. Ready to save...");
    Console.WriteLine(new string('-', 40));

    // 2. Connect to database
    using var dbContext = provider.GetRequiredService<AppDbContext>();

    // Ensure database and tables exist (Creates ironlogic.db if missing)
    dbContext.Database.EnsureCreated();

    // 3. Insert data if the database is currently empty
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


    Console.WriteLine("\n--- IronLogic AI: Data Analysis ---");
    
    var analyzer = new WorkoutAnalysisService(dbContext);

    var maxBench = analyzer.GetMaxWeightForExercise("Bench Press");
    Console.WriteLine($"🏆 Max Bench Press (Any Variation): {maxBench} lbs");

    Console.WriteLine("\n🔥 Top 5 Most Performed Exercises:");
    var topExercises = analyzer.GetTopExercises();
    foreach (var ex in topExercises)
    {
        Console.WriteLine($"  -> {ex}");
    }

    var last30DaysVolume = analyzer.GetTotalVolumeInDateRange(DateTime.Now.AddDays(-30), DateTime.Now);
    Console.WriteLine($"\n📈 Total Volume Lifted (Last 30 Days): {last30DaysVolume} lbs");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}