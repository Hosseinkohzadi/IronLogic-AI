using IronLogic.Application.Interfaces;
using IronLogic.Application.Mappers;
using IronLogic.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<IHevyParserService, HevyCsvParserService>();
services.AddSingleton<IHevyDataMapper, HevyDataMapper>();

var provider = services.BuildServiceProvider();

Console.WriteLine("--- IronLogic AI: CSV to Hierarchy Test ---");

// Insert your actual CSV path here
var filePath = @"C:\Path\To\Your\hevy_export.csv";
var parser = provider.GetRequiredService<IHevyParserService>();
var mapper = provider.GetRequiredService<IHevyDataMapper>();

try
{
    // 1. Get flat records from CSV
    var flatRecords = parser.Parse(filePath);
    Console.WriteLine($"Loaded {flatRecords.Count} flat rows from CSV.");

    // 2. Map to Domain Hierarchy
    var sessions = mapper.MapToSessions(flatRecords);
    Console.WriteLine($"Mapped into {sessions.Count} Workout Sessions.");
    Console.WriteLine(new string('-', 40));

    // 3. Print a quick summary of the first 2 sessions
    foreach (var session in sessions.Take(2))
    {
        Console.WriteLine($"Workout: {session.Name} | Date: {session.Date:yyyy-MM-dd}");
        Console.WriteLine($"Total Volume: {session.TotalSessionVolume} kg");

        foreach (var exercise in session.Exercises)
            Console.WriteLine($"  -> {exercise.Name} ({exercise.Sets.Count} sets)");
        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}