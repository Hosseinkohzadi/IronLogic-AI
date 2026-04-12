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

    public DbSet<IronLogic.Domain.Entities.Exercise> Exercises { get; set; }

    public DbSet<ExerciseSession> ExerciseSessions { get; set; }

    public DbSet<DailyWeight> DailyWeights { get; set; }

    public DbSet<Muscle> Muscles { get; set; }

    public DbSet<Equipment> Equipments { get; set; }

    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ironlogic.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}")
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var defaultUserId = "00000000-0000-0000-0000-000000000001";
        var hasher = new PasswordHasher<User>();

        modelBuilder.Entity<ExerciseSession>()
            .HasIndex(es => new { es.ExerciseId, es.Weight })
            .HasDatabaseName("IX_Exercise_Weight");

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

        // Exercise configuration with approval workflow
        modelBuilder.Entity<IronLogic.Domain.Entities.Exercise>(entity =>
        {
            entity.HasOne(e => e.CreatorUser)
                .WithMany()
                .HasForeignKey(e => e.CreatorUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .HasDefaultValue(Domain.Enums.ExerciseStatus.Private);

            entity.Property(e => e.IsGlobal)
                .HasDefaultValue(false);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatorUserId);

            // Global Query Filter: Users only see exercises where Status == Approved OR CreatorUserId == currentUserId
            // Note: This is a template - actual userId will be injected at runtime via IHttpContextAccessor
            // For now, this demonstrates the pattern. Implement CurrentUserService to get userId dynamically.
            // entity.HasQueryFilter(e => e.Status == Domain.Enums.ExerciseStatus.Approved || e.CreatorUserId == currentUserId);
        });

        // SubscriptionPlan configuration with multi-currency support
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(sp => sp.Price)
                .HasPrecision(18, 2);

            entity.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(sp => sp.Currency)
                .HasConversion<string>()
                .HasMaxLength(3);

            entity.HasIndex(sp => new { sp.Currency, sp.IsActive });
        });

        // UserSubscription configuration
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasOne(us => us.User)
                .WithMany(u => u.UserSubscriptions)
                .HasForeignKey(us => us.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(us => us.Plan)
                .WithMany(sp => sp.UserSubscriptions)
                .HasForeignKey(us => us.PlanId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(us => new { us.UserId, us.IsActive });
        });

        // PaymentTransaction configuration with decimal precision and multi-currency support
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.Property(pt => pt.Amount)
                .HasPrecision(18, 2);

            entity.Property(pt => pt.TaxAmount)
                .HasPrecision(18, 2);

            entity.Property(pt => pt.RefundAmount)
                .HasPrecision(18, 2);

            entity.HasOne(pt => pt.User)
                .WithMany(u => u.PaymentTransactions)
                .HasForeignKey(pt => pt.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(pt => pt.GatewayTransactionId)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(pt => pt.Status)
                .HasConversion<string>();

            entity.Property(pt => pt.Currency)
                .HasConversion<string>()
                .HasMaxLength(3);

            entity.Property(pt => pt.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            entity.Property(pt => pt.RegionCode)
                .HasMaxLength(3);

            entity.HasIndex(pt => pt.GatewayTransactionId)
                .IsUnique();

            entity.HasIndex(pt => pt.UserId);

            entity.HasIndex(pt => new { pt.CountryCode, pt.Currency });

            entity.HasIndex(pt => pt.StripeSubscriptionId);
        });

        // User configuration for international support
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.UnitSystem)
                .HasConversion<string>();

            entity.Property(u => u.PreferredCurrency)
                .HasConversion<string>()
                .HasMaxLength(3);

            entity.Property(u => u.TimeZone)
                .HasMaxLength(50)
                .HasDefaultValue("UTC");

            entity.Property(u => u.CountryCode)
                .HasMaxLength(2)
                .HasDefaultValue("US");

            entity.HasIndex(u => u.CountryCode);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.Property(up => up.Bio)
                .HasMaxLength(1000);

            entity.Property(up => up.Gender)
                .HasConversion<string>();

            entity.Property(up => up.Height)
                .HasPrecision(5, 2);

            entity.Property(up => up.CurrentWeight)
                .HasPrecision(5, 2);

            entity.Property(up => up.TargetWeight)
                .HasPrecision(5, 2);

            entity.Property(up => up.ActivityLevel)
                .HasConversion<string>();

            entity.HasOne(up => up.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(up => up.UserId)
                .IsUnique();
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