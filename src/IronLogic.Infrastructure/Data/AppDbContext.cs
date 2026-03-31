using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IronLogic.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext()
    {
    }

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

        var defaultUserId = "00000000-0000-0000-0000-000000000001";
        var hasher = new PasswordHasher<User>();

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<DailyWeight>(entity =>
        {
            entity.HasOne(dw => dw.User)
                .WithMany(u => u.DailyWeights)
                .HasForeignKey(dw => dw.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = defaultUserId,
            Email = "kohzadi90@gmail.com",
            UserName = "kohzadi90@gmail.com",
            NormalizedUserName = "KOHZADI90@GMAIL.COM",
            NormalizedEmail = "KOHZADI90@GMAIL.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEA7IkppTOn/SpmrmnXTCMdPLqEonDkuYkMjRDc6IXd+rrZ5BbdPP0st7JtFTBjPOig==",
            SecurityStamp = "a1b2c3d4-e5f6-7890-1234-567890abcdef",
            ConcurrencyStamp = "fedcba98-7654-3210-fedc-ba9876543210",
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