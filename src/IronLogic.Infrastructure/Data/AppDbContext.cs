namespace IronLogic.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // 1. Constructor for Dependency Injection (The standard way)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 2. Parameterless constructor (Needed by EF Core Migrations Tooling)
    public AppDbContext()
    {
    }

    public DbSet<WorkoutSession> Sessions { get; set; }
    public DbSet<WorkoutExercise> Exercises { get; set; }
    public DbSet<ExerciseSet> Sets { get; set; }
    public DbSet<DailyWeight> DailyWeights { get; set; }
    public DbSet<MuscleMeasurement> MuscleMeasurements { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only configure here if it wasn't already configured in Program.cs
        // This prevents the "pooling" and configuration conflict errors!
        if (optionsBuilder.IsConfigured)
            return;

        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ironlogic.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutSession>()
            .HasMany(s => s.Exercises)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkoutExercise>()
            .HasMany(e => e.Sets)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyWeight>(entity =>
        {
            entity.Property(d => d.Weight).IsRequired();
            entity.Property(d => d.Note).HasMaxLength(200);
        });

        modelBuilder.Entity<MuscleMeasurement>(entity =>
        {
            entity.Property(m => m.Neck).IsRequired();
            entity.Property(m => m.Chest).IsRequired();
            entity.Property(m => m.Waist).IsRequired();
        });
    }
}