using FluentAssertions;
using IronLogic.Infrastructure.Data;
using IronLogic.Infrastructure.Services;
using IronLogic.Infrastructure.Services.Parsing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // 🚀 اضافه شد
using Microsoft.Extensions.Logging.Abstractions; // 🚀 اضافه شد
using Xunit;

namespace IronLogic.Tests.Services.Parsing;

public class WorkoutServiceTests
{
    [Fact]
    public async Task Service_NewExercise_AssignsDefaults()
    {
        // Arrange
        var rawText = """
                      First Time Leg Day
                      Thursday, Mar 26, 2026 at 6:00pm

                      A Brand New Exercise
                      Set 1: 100 lbs x 10
                      """;

        // Use SQLite in-memory database for transaction support.
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Ensure the database schema is created
        await using (var dbContext = new AppDbContext(options))
        {
            await dbContext.Database.EnsureCreatedAsync(CancellationToken.None);
        }

        var parser = new WorkoutParserService();
        
        // 🚀 ساخت نمونه‌های فیک/تستی برای Logger و Cache
        var logger = NullLogger<WorkoutService>.Instance;
        var cache = new MemoryCache(new MemoryCacheOptions());

        // 🚀 پاس دادن هر 4 پارامتر به سازنده
        var service = new WorkoutService(parser, new AppDbContext(options), logger, cache);
        var userId = "test-user";

        // Act
        var result = await service.CreateFromRawTextAsync(rawText, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        using (var dbContext = new AppDbContext(options))
        {
            var newExercise =
                await dbContext.Exercises.FirstOrDefaultAsync(e => e.Name == "A Brand New Exercise",
                    CancellationToken.None);
            newExercise.Should().NotBeNull();

            // These should match the hardcoded default GUIDs in the service
            newExercise.EquipmentId.Should().NotBe(Guid.Empty);
            newExercise.PrimaryMuscleId.Should().NotBe(Guid.Empty);

            // در اینجا result.Value اکنون یک WorkoutImportResult است، پس برای پیدا کردن سشن باید از result.Value.SessionId استفاده کنید
            var session = await dbContext.Sessions.FindAsync(result.Value.SessionId); 
            session.Should().NotBeNull();
            session.Title.Should().Be("First Time Leg Day");
        }

        // Clean up the connection
        connection.Close();
    }
}