using IronLogic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext()
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseSession> ExerciseSessions { get; set; }
    public DbSet<DailyWeight> DailyWeights { get; set; }
    public DbSet<Muscle> Muscles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ironlogic.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var defaultUserId = new Guid("00000000-0000-0000-0000-000000000001");
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Static historical date for seed data

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = defaultUserId,
            Username = "kohzadi90",
            Email = "kohzadi90@gmail.com",
            DateCreated = seedDate,
            DateModified = seedDate
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).DateModified = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
                ((BaseEntity)entityEntry.Entity).DateCreated = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}