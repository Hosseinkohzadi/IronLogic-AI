public static class MuscleSeeder
{
    public static void SeedMuscles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Muscle>().HasData(
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000001"), Name = "Unknown" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000002"), Name = "Chest" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000003"), Name = "Back" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000004"), Name = "Quads" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000005"), Name = "Hamstrings" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000006"), Name = "Glutes" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000007"), Name = "Shoulders" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000008"), Name = "Biceps" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000009"), Name = "Triceps" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000010"), Name = "Abs" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000011"), Name = "Calves" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000012"), Name = "Lower Back" },
            new Muscle { Id = new Guid("00000000-0000-0000-0000-000000000013"), Name = "Forearms" }
        );
    }
}