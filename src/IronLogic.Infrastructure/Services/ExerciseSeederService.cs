using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Exercise = IronLogic.Domain.Entities.Exercise;

namespace IronLogic.Infrastructure.Services;

/// <summary>
///     Service responsible for seeding exercise data from JSON files into the database.
/// </summary>
public static class ExerciseSeederService
{
    /// <summary>
    ///     Seeds exercise data from a JSON file into the database and ensures admin user exists.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="userManager">The user manager for creating admin users.</param>
    /// <param name="roleManager">The role manager for creating roles.</param>
    /// <param name="loggerFactory">The logger factory for logging operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(ExerciseSeederService));

        // Step 1: Ensure roles exist first
        await EnsureRolesExistAsync(roleManager, logger);

        // Step 2: Ensure admin user exists
        await EnsureAdminUserExistsAsync(userManager, logger);

        // Step 3: Seed exercises from JSON
        await SeedExercisesAsync(context, logger);
    }

    /// <summary>
    ///     Ensures that Admin and User roles exist in the database.
    /// </summary>
    /// <param name="roleManager">The role manager for creating roles.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task EnsureRolesExistAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "User"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }
    }

    /// <summary>
    ///     Ensures that an admin user exists in the database.
    /// </summary>
    /// <param name="userManager">The user manager for creating users.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task EnsureAdminUserExistsAsync(UserManager<User> userManager, ILogger logger)
    {
        const string adminEmail = "admin@ironlogic.ai";
        const string adminPassword = "Admin@123456";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            logger.LogInformation("Creating admin user: {Email}", adminEmail);

            adminUser = new User
            {
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created successfully");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        // Ensure admin has Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Admin role assigned to {Email}", adminEmail);
            }
            else
            {
                logger.LogWarning("Failed to assign Admin role to {Email}", adminEmail);
            }
        }
    }

    /// <summary>
    ///     Seeds exercise data from a JSON file into the database.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task SeedExercisesAsync(AppDbContext context, ILogger logger)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "exercises_final.json");
        if (!File.Exists(filePath))
        {
            logger.LogWarning("Exercise data file not found at: {FilePath}", filePath);
            return;
        }

        logger.LogInformation("Loading exercise data from: {FilePath}", filePath);

        var jsonData = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawData = JsonSerializer.Deserialize<List<ExerciseJsonModel>>(jsonData, options);

        if (rawData == null || !rawData.Any())
        {
            logger.LogWarning("No exercise data found in JSON file");
            return;
        }

        logger.LogInformation("Loaded {Count} exercises from JSON", rawData.Count);

        // Step 1: Ensure base data exists (muscles and equipment)
        await EnsureBaseDataExistsAsync(context, rawData, logger);

        // If exercises already exist, stop
        if (await context.Exercises.AnyAsync())
        {
            logger.LogInformation("Exercises already exist in database, skipping seed");
            return;
        }

        logger.LogInformation("Seeding exercises into database...");

        // Step 2: Reload maps after saving the first stage
        var muscleMap = await context.Muscles.ToDictionaryAsync(m => m.Name.ToLower(), m => m);
        var equipmentMap = await context.Equipments.ToDictionaryAsync(e => e.Name.ToLower(), e => e.Id);

        // Get default IDs for fallback
        var defaultMuscleId = muscleMap.Values.First(x => x.Name.Equals("General", StringComparison.OrdinalIgnoreCase)).Id;
        var defaultEquipId = equipmentMap.Keys.Contains("none") ? equipmentMap["none"] : equipmentMap.Values.First();

        // Get admin user ID for CreatorUserId
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@ironlogic.ai");
        var creatorUserId = adminUser?.Id ?? "00000000-0000-0000-0000-000000000001";

        // Step 3: Add exercises with handling for missing values
        foreach (var item in rawData)
        {
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Url = item.Url,
                ImagePath = item.Image_Path,
                Mechanics = item.Mechanics ?? "None",
                Instructions = item.Instructions != null ? string.Join("\n", item.Instructions) : "",
                SecondaryMuscles = new List<Muscle>(),
                CreatorUserId = creatorUserId,
                IsGlobal = true,
                Status = Domain.Enums.ExerciseStatus.Approved
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
        logger.LogInformation("Successfully seeded {Count} exercises", rawData.Count);
    }

    /// <summary>
    ///     Ensures that base data (muscles and equipment) exists in the database before seeding exercises.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="data">The raw exercise data from JSON.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task EnsureBaseDataExistsAsync(AppDbContext context, List<ExerciseJsonModel> data, ILogger logger)
    {
        logger.LogInformation("Ensuring base data (muscles and equipment) exists...");

        // Extract names and ensure "General" entity exists
        var muscleNames = data.Select(x => x.Primary_Muscle)
            .Concat(data.SelectMany(x => (x.Secondary_Muscle ?? "").Split(',').Select(s => s.Trim())))
            .Append("General") // Ensure default entity exists
            .Where(x => !string.IsNullOrEmpty(x) && x != "None")
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var equipNames = data.Select(x => x.Equipment)
            .Append("None") // Ensure default equipment exists
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var existingMuscles = await context.Muscles.Select(m => m.Name.ToLower()).ToListAsync();
        var newMusclesCount = 0;
        foreach (var name in muscleNames.Where(n => !existingMuscles.Contains(n.ToLower())))
        {
            context.Muscles.Add(new Muscle { Id = Guid.NewGuid(), Name = name });
            newMusclesCount++;
        }

        var existingEquip = await context.Equipments.Select(e => e.Name.ToLower()).ToListAsync();
        var newEquipmentCount = 0;
        foreach (var name in equipNames.Where(n => !existingEquip.Contains(n.ToLower())))
        {
            context.Equipments.Add(new Equipment { Id = Guid.NewGuid(), Name = name });
            newEquipmentCount++;
        }

        if (newMusclesCount > 0 || newEquipmentCount > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Added {MuscleCount} new muscles and {EquipmentCount} new equipment",
                newMusclesCount, newEquipmentCount);
        }
        else
        {
            logger.LogInformation("Base data already exists, no new records added");
        }
    }

    /// <summary>
    ///     Model representing exercise data from JSON file.
    /// </summary>
    private class ExerciseJsonModel
    {
        /// <summary>
        ///     Gets or sets the name of the exercise.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the URL reference for the exercise.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Gets or sets the image path for the exercise.
        /// </summary>
        public string Image_Path { get; set; }

        /// <summary>
        ///     Gets or sets the primary muscle targeted by the exercise.
        /// </summary>
        public string Primary_Muscle { get; set; }

        /// <summary>
        ///     Gets or sets the secondary muscles targeted by the exercise.
        /// </summary>
        public string Secondary_Muscle { get; set; }

        /// <summary>
        ///     Gets or sets the equipment required for the exercise.
        /// </summary>
        public string Equipment { get; set; }

        /// <summary>
        ///     Gets or sets the mechanics type of the exercise.
        /// </summary>
        public string Mechanics { get; set; }

        /// <summary>
        ///     Gets or sets the list of instructions for performing the exercise.
        /// </summary>
        public List<string> Instructions { get; set; }
    }
}