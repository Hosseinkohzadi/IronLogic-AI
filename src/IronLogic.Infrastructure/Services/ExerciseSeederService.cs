using System.Text.Json;
using IronLogic.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Exercise = IronLogic.Domain.Entities.Exercise;

namespace IronLogic.Infrastructure.Services;

/// <summary>
/// Service responsible for seeding exercise data from JSON files into the database.
/// Ensures proper order of operations: Database creation ? Admin user ? Base data ? Exercises.
/// All operations are wrapped in transactions for data integrity.
/// </summary>
public static class ExerciseSeederService
{
    private const string DefaultAdminUserId = "00000000-0000-0000-0000-000000000001";
    private const string DefaultAdminEmail = "admin@ironlogic.ai";
    private const string DefaultAdminUserName = "admin@ironlogic.ai";

    /// <summary>
    /// Seeds exercise data from a JSON file into the database with proper transaction handling.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="userManager">The user manager for creating admin user.</param>
    /// <param name="loggerFactory">Logger factory for creating logger instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<User> userManager,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("ExerciseSeeder");
        
        try
        {
            logger.LogInformation("Starting database seeding process at {Time}", DateTime.UtcNow);

            // Step 1: Ensure database is created/migrated
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database ensured created/migrated");

            // Step 2: Ensure default Admin user exists
            await EnsureAdminUserExistsAsync(context, userManager, logger);

            // Step 3: Check if exercises already exist
            if (await context.Exercises.AnyAsync())
            {
                logger.LogInformation("Exercises already exist in database. Skipping seeding.");
                return;
            }

            // Step 4: Load exercise data from JSON
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "exercises_final.json");
            if (!File.Exists(filePath))
            {
                logger.LogWarning("Exercise JSON file not found at {FilePath}. Skipping exercise seeding.", filePath);
                return;
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var rawData = JsonSerializer.Deserialize<List<ExerciseJsonModel>>(jsonData, options);

            if (rawData == null || rawData.Count == 0)
            {
                logger.LogWarning("No exercise data found in JSON file. Skipping seeding.");
                return;
            }

            logger.LogInformation("Loaded {Count} exercises from JSON file", rawData.Count);

            // Step 5: Seed data within a transaction
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Ensure base data exists (muscles and equipment)
                await EnsureBaseDataExistsAsync(context, rawData, logger);

                // Reload maps after saving base data
                var muscleMap = await context.Muscles.ToDictionaryAsync(m => m.Name.ToLower(), m => m);
                var equipmentMap = await context.Equipments.ToDictionaryAsync(e => e.Name.ToLower(), e => e.Id);

                // Get default IDs for fallback
                var defaultMuscleId = muscleMap.Values.First(x => x.Name.Equals("General", StringComparison.OrdinalIgnoreCase)).Id;
                var defaultEquipId = equipmentMap.ContainsKey("none") ? equipmentMap["none"] : equipmentMap.Values.First();

                logger.LogInformation("Base data prepared. Starting exercise seeding...");

                // Add exercises with proper CreatorUserId
                foreach (var item in rawData)
                {
                    var exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = item.Name,
                        Url = item.Url,
                        ImagePath = item.Image_Path,
                        Mechanics = item.Mechanics ?? "None",
                        Instructions = item.Instructions != null ? string.Join("\n", item.Instructions) : string.Empty,
                        SecondaryMuscles = new List<Muscle>(),
                        
                        // CRITICAL: Set CreatorUserId to Admin user to satisfy foreign key constraint
                        CreatorUserId = DefaultAdminUserId,
                        
                        // Set approval status for seeded exercises
                        Status = ExerciseStatus.Approved,
                        IsGlobal = true
                    };

                    // Safe assignment of primary muscle
                    var pMuscleName = (item.Primary_Muscle ?? "General").ToLower();
                    exercise.PrimaryMuscleId = muscleMap.TryGetValue(pMuscleName, out var pm) ? pm.Id : defaultMuscleId;

                    // Safe assignment of equipment
                    var equipName = (item.Equipment ?? "None").ToLower();
                    exercise.EquipmentId = equipmentMap.TryGetValue(equipName, out var eid) ? eid : defaultEquipId;

                    // Assign secondary muscles
                    if (!string.IsNullOrWhiteSpace(item.Secondary_Muscle) && item.Secondary_Muscle != "None")
                    {
                        var secondaryNames = item.Secondary_Muscle.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var sName in secondaryNames)
                        {
                            if (muscleMap.TryGetValue(sName.Trim().ToLower(), out var secMuscle))
                            {
                                exercise.SecondaryMuscles.Add(secMuscle);
                            }
                        }
                    }

                    context.Exercises.Add(exercise);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("Successfully seeded {Count} exercises into database", rawData.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error during exercise seeding. Transaction rolled back.");
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error during database seeding process");
            throw;
        }
    }

    /// <summary>
    /// Ensures that the default Admin user exists in the database.
    /// Creates the admin user if it doesn't exist with proper Identity configuration.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="userManager">The user manager for creating admin user.</param>
    /// <param name="logger">Logger for tracking operations.</param>
    private static async Task EnsureAdminUserExistsAsync(
        AppDbContext context,
        UserManager<User> userManager,
        ILogger logger)
    {
        // Check if admin user already exists
        var adminUser = await context.Users.FindAsync(DefaultAdminUserId);

        if (adminUser != null)
        {
            logger.LogInformation("Admin user already exists with ID: {AdminUserId}", DefaultAdminUserId);
            return;
        }

        logger.LogInformation("Creating default Admin user...");

        // Create admin user with explicit ID
        adminUser = new User
        {
            Id = DefaultAdminUserId,
            UserName = DefaultAdminUserName,
            Email = DefaultAdminEmail,
            NormalizedUserName = DefaultAdminUserName.ToUpperInvariant(),
            NormalizedEmail = DefaultAdminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            ConcurrencyStamp = Guid.NewGuid().ToString("D"),
            
            // Global platform defaults
            UnitSystem = UnitSystem.Metric,
            PreferredCurrency = Currency.USD,
            TimeZone = "UTC",
            CountryCode = "US"
        };

        // Hash password for admin user
        var passwordHasher = new PasswordHasher<User>();
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123456");

        // Add user directly to context to preserve the specific ID
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin user created successfully with ID: {AdminUserId}", DefaultAdminUserId);
    }

    /// <summary>
    /// Ensures that base data (muscles and equipment) exists in the database before seeding exercises.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="data">The raw exercise data from JSON.</param>
    /// <param name="logger">Logger for tracking operations.</param>
    private static async Task EnsureBaseDataExistsAsync(
        AppDbContext context,
        List<ExerciseJsonModel> data,
        ILogger logger)
    {
        logger.LogInformation("Ensuring base data (muscles and equipment) exists...");

        // Extract names and ensure "General" entity exists
        var muscleNames = data.Select(x => x.Primary_Muscle)
            .Concat(data.SelectMany(x => (x.Secondary_Muscle ?? string.Empty).Split(',').Select(s => s.Trim())))
            .Append("General") // Ensure default entity exists
            .Where(x => !string.IsNullOrEmpty(x) && x != "None")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var equipNames = data.Select(x => x.Equipment)
            .Append("None") // Ensure default equipment exists
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingMuscles = await context.Muscles.Select(m => m.Name.ToLower()).ToListAsync();
        var musclesToAdd = muscleNames.Where(n => !existingMuscles.Contains(n.ToLower())).ToList();

        foreach (var name in musclesToAdd)
        {
            context.Muscles.Add(new Muscle { Id = Guid.NewGuid(), Name = name });
        }

        var existingEquip = await context.Equipments.Select(e => e.Name.ToLower()).ToListAsync();
        var equipmentToAdd = equipNames.Where(n => !existingEquip.Contains(n.ToLower())).ToList();

        foreach (var name in equipmentToAdd)
        {
            context.Equipments.Add(new Equipment { Id = Guid.NewGuid(), Name = name });
        }

        if (musclesToAdd.Count > 0 || equipmentToAdd.Count > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Added {MuscleCount} muscles and {EquipmentCount} equipment to database",
                musclesToAdd.Count, equipmentToAdd.Count);
        }
        else
        {
            logger.LogInformation("All base data already exists");
        }
    }

    /// <summary>
    /// Model representing exercise data from JSON file.
    /// </summary>
    private class ExerciseJsonModel
    {
        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL reference for the exercise.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image path for the exercise.
        /// </summary>
        public string Image_Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary muscle targeted by the exercise.
        /// </summary>
        public string Primary_Muscle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the secondary muscles targeted by the exercise.
        /// </summary>
        public string? Secondary_Muscle { get; set; }

        /// <summary>
        /// Gets or sets the equipment required for the exercise.
        /// </summary>
        public string Equipment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mechanics type of the exercise.
        /// </summary>
        public string? Mechanics { get; set; }

        /// <summary>
        /// Gets or sets the list of instructions for performing the exercise.
        /// </summary>
        public List<string>? Instructions { get; set; }
    }
}
