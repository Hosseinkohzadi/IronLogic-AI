using IronLogic.Application.Interfaces;
using IronLogic.Application.Mappers;
using IronLogic.Application.Services;
using IronLogic.Infrastructure;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Services;
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

    // Ensure database and tables exist (Creates ironlogic.db if missing)
    dbContext.Database.EnsureCreated();

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

    // --- Phase 4: The Analyzer (Testing the Database) ---
    Console.WriteLine("\n--- IronLogic AI: Data Analysis ---");
    var analyzer = new WorkoutAnalysisService(dbContext);

    var maxBench = analyzer.GetMaxWeightForExercise("Bench Press");
    var topExercisesList = analyzer.GetTopExercises();
    var last30DaysVolume = analyzer.GetTotalVolumeInDateRange(DateTime.Now.AddDays(-30), DateTime.Now);

    Console.WriteLine($"🏆 Max Bench Press: {maxBench} lbs");
    Console.WriteLine($"📈 Total Volume (30 Days): {last30DaysVolume} lbs");

    // --- Phase 5: The Simulated AI Coach ---
    Console.WriteLine("\n--- 🤖 IronLogic AI Coach is thinking... ---");

    // Initialize our rule-based mock service
    var aiCoach = new IronLogicCoachService();

    // Convert the list of top exercises into a single formatted string
    var exercisesText = string.Join("\n", topExercisesList.Select(e => $"  - {e}"));

    // Get the dynamic advice
    var aiAdvice = await aiCoach.AnalyzeWorkoutStatsAsync(maxBench, last30DaysVolume, exercisesText);

    Console.WriteLine(aiAdvice);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}