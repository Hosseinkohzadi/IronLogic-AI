# Database Migration Guide - Financial & Exercise Approval System

## Prerequisites
- .NET 10 SDK installed
- EF Core tools installed globally:
  ```bash
  dotnet tool install --global dotnet-ef
  # or update if already installed
  dotnet tool update --global dotnet-ef
  ```

## Step 1: Create Migration

From the solution root directory:

```bash
cd src\IronLogic.Infrastructure
dotnet ef migrations add AddFinancialAndExerciseApproval --startup-project ..\IronLogic.Api --context AppDbContext
```

Expected output:
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

## Step 2: Review Migration

The migration file will be created in `src\IronLogic.Infrastructure\Migrations\` with timestamp prefix.

### Expected Changes:

#### New Tables:
1. **SubscriptionPlans**
   - Id (TEXT, PK)
   - Name (TEXT, NOT NULL, MaxLength: 100)
   - Price (TEXT with DECIMAL precision 18,2)
   - DurationDays (INTEGER)
   - FeaturesJson (TEXT, nullable)
   - DateCreated (TEXT)
   - DateModified (TEXT)

2. **UserSubscriptions**
   - Id (TEXT, PK)
   - UserId (TEXT, FK to AspNetUsers)
   - PlanId (TEXT, FK to SubscriptionPlans)
   - StartDate (TEXT)
   - EndDate (TEXT)
   - IsActive (INTEGER/BOOLEAN)
   - DateCreated (TEXT)
   - DateModified (TEXT)
   - INDEX: IX_UserSubscriptions_UserId_IsActive

3. **PaymentTransactions**
   - Id (TEXT, PK)
   - UserId (TEXT, FK to AspNetUsers)
   - Amount (TEXT with DECIMAL precision 18,2)
   - GatewayTransactionId (TEXT, NOT NULL, MaxLength: 255, UNIQUE)
   - Status (TEXT, NOT NULL, MaxLength: 50)
   - DateCreated (TEXT)
   - DateModified (TEXT)
   - INDEX: IX_PaymentTransactions_GatewayTransactionId (UNIQUE)
   - INDEX: IX_PaymentTransactions_UserId

#### Updated Table:
**Exercises**
- Added: ImageUrl (TEXT, nullable)
- Added: CreatorUserId (TEXT, NOT NULL, FK to AspNetUsers)
- Added: Status (INTEGER, DEFAULT 0 = Private)
- Added: IsGlobal (INTEGER/BOOLEAN, DEFAULT 0 = false)
- INDEX: IX_Exercises_Status
- INDEX: IX_Exercises_CreatorUserId
- FK: FK_Exercises_AspNetUsers_CreatorUserId (ON DELETE RESTRICT)

## Step 3: Verify Migration Script

Review the generated migration Up() and Down() methods in:
`src\IronLogic.Infrastructure\Migrations\YYYYMMDDHHMMSS_AddFinancialAndExerciseApproval.cs`

### Critical Checks:
- ? Decimal precision set to (18,2) for Price and Amount
- ? Foreign key relationships properly defined
- ? Delete behaviors configured (Cascade vs Restrict)
- ? Indexes created on Status, CreatorUserId, GatewayTransactionId
- ? Default values set for Status and IsGlobal
- ? Unique constraint on GatewayTransactionId

## Step 4: Apply Migration

### Development Environment:
```bash
dotnet ef database update --startup-project ..\IronLogic.Api --context AppDbContext
```

Expected output:
```
Build started...
Build succeeded.
Applying migration '20260329XXXXXX_AddFinancialAndExerciseApproval'.
Done.
```

### Production Environment:
```bash
# Generate SQL script for review/deployment
dotnet ef migrations script --startup-project ..\IronLogic.Api --context AppDbContext --output migration.sql

# Review migration.sql before executing on production database
```

## Step 5: Verify Database

### SQLite (Development):
```bash
sqlite3 ironlogic.db

# Check tables
.tables

# Expected output should include:
# SubscriptionPlans
# UserSubscriptions
# PaymentTransactions

# Check Exercises table schema
.schema Exercises

# Verify new columns: ImageUrl, CreatorUserId, Status, IsGlobal
```

### Verify Indexes:
```sql
-- SQLite
.indexes Exercises
.indexes UserSubscriptions
.indexes PaymentTransactions

-- Expected:
-- IX_Exercises_Status
-- IX_Exercises_CreatorUserId
-- IX_UserSubscriptions_UserId_IsActive
-- IX_PaymentTransactions_GatewayTransactionId
-- IX_PaymentTransactions_UserId
```

## Step 6: Seed Initial Data (Optional)

Create a new migration or add to existing one:

```bash
dotnet ef migrations add SeedSubscriptionPlans --startup-project ..\IronLogic.Api
```

In the migration Up() method:

```csharp
migrationBuilder.InsertData(
    table: "SubscriptionPlans",
    columns: new[] { "Id", "Name", "Price", "DurationDays", "FeaturesJson", "DateCreated", "DateModified" },
    values: new object[,]
    {
        { 
            Guid.NewGuid().ToString(), 
            "Basic", 
            9.99m, 
            30, 
            "{\"maxWorkouts\":50,\"aiCoach\":false,\"analytics\":\"basic\"}", 
            DateTimeOffset.UtcNow, 
            DateTimeOffset.UtcNow 
        },
        { 
            Guid.NewGuid().ToString(), 
            "Premium", 
            19.99m, 
            30, 
            "{\"maxWorkouts\":\"unlimited\",\"aiCoach\":true,\"analytics\":\"advanced\"}", 
            DateTimeOffset.UtcNow, 
            DateTimeOffset.UtcNow 
        },
        { 
            Guid.NewGuid().ToString(), 
            "Pro", 
            99.99m, 
            365, 
            "{\"maxWorkouts\":\"unlimited\",\"aiCoach\":true,\"analytics\":\"premium\",\"personalTrainer\":true}", 
            DateTimeOffset.UtcNow, 
            DateTimeOffset.UtcNow 
        }
    });
```

## Troubleshooting

### Error: "Build failed"
**Solution**: Ensure all dependencies are restored
```bash
dotnet restore
dotnet build
```

### Error: "No DbContext named 'AppDbContext' was found"
**Solution**: Verify you're in the Infrastructure project and specifying startup project
```bash
cd src\IronLogic.Infrastructure
dotnet ef migrations add ... --startup-project ..\IronLogic.Api
```

### Error: "Unable to create an object of type 'AppDbContext'"
**Solution**: Ensure AppDbContext has a parameterless constructor or OnConfiguring method
- ? Already implemented in `AppDbContext.cs`

### Error: "A migration with this name already exists"
**Solution**: Remove the existing migration first
```bash
dotnet ef migrations remove --startup-project ..\IronLogic.Api
```

### Error: Foreign key constraint violation
**Cause**: Existing Exercise records don't have CreatorUserId
**Solution**: Two approaches:

**Approach 1**: Make CreatorUserId nullable temporarily
```csharp
public string? CreatorUserId { get; set; }
```

**Approach 2**: Set default value in migration
```csharp
migrationBuilder.AddColumn<string>(
    name: "CreatorUserId",
    table: "Exercises",
    type: "TEXT",
    nullable: false,
    defaultValue: "00000000-0000-0000-0000-000000000001"); // Default user
```

Then make it required in a follow-up migration.

## Rollback Instructions

### Rollback Last Migration:
```bash
dotnet ef database update <PreviousMigrationName> --startup-project ..\IronLogic.Api
```

### Remove Last Migration (if not applied):
```bash
dotnet ef migrations remove --startup-project ..\IronLogic.Api
```

### Complete Rollback:
```bash
# List all migrations
dotnet ef migrations list --startup-project ..\IronLogic.Api

# Rollback to specific migration
dotnet ef database update InitialCreate --startup-project ..\IronLogic.Api

# Remove the migration file
dotnet ef migrations remove --startup-project ..\IronLogic.Api
```

## Post-Migration Validation

### Test Queries:

```sql
-- Verify SubscriptionPlans table
SELECT COUNT(*) FROM SubscriptionPlans;

-- Verify UserSubscriptions table
SELECT COUNT(*) FROM UserSubscriptions;

-- Verify PaymentTransactions table
SELECT COUNT(*) FROM PaymentTransactions;

-- Verify Exercises table has new columns
PRAGMA table_info(Exercises);
-- Should show: ImageUrl, CreatorUserId, Status, IsGlobal

-- Verify indexes
SELECT name FROM sqlite_master WHERE type='index' AND tbl_name='Exercises';
-- Should include: IX_Exercises_Status, IX_Exercises_CreatorUserId
```

### API Validation:

```bash
# Start the API
cd src\IronLogic.Api
dotnet run

# Test endpoints (use Postman/curl/browser)
curl https://localhost:5001/api/v1/exercises/available?userId=test-user
curl https://localhost:5001/api/v1/admin/exercise-approvals/pending
```

## Production Deployment Checklist

- [ ] Generate migration script: `dotnet ef migrations script`
- [ ] Review SQL script for any issues
- [ ] Backup production database
- [ ] Test migration on staging environment first
- [ ] Schedule maintenance window (if needed)
- [ ] Execute migration script on production
- [ ] Verify all tables and indexes created
- [ ] Run smoke tests on production API
- [ ] Monitor for errors
- [ ] Document migration date and version

## Migration Best Practices

1. **Always backup** before running migrations
2. **Test on dev/staging** before production
3. **Use migration scripts** for production (not `dotnet ef database update`)
4. **Review generated SQL** before applying
5. **Document breaking changes** in release notes
6. **Plan rollback strategy** before migrating
7. **Monitor performance** after migration (new indexes may affect query plans)
8. **Seed reference data** in migrations, not in code

## Next Steps After Migration

1. ? Verify database schema
2. ? Run integration tests
3. ? Test API endpoints
4. ? Update API documentation
5. ? Deploy to staging
6. ? Perform UAT (User Acceptance Testing)
7. ? Deploy to production
8. ? Monitor logs and metrics
