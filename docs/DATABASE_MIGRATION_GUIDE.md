# Database Migration Guide - Global Platform Implementation

## Overview
This guide provides step-by-step instructions for applying the global platform enhancements to your IronLogic AI database.

---

## Prerequisites
- .NET 10 SDK installed
- Entity Framework Core CLI tools installed
- Access to your database connection string

---

## Step 1: Create Migration

```bash
cd src/IronLogic.Infrastructure

# Create a comprehensive migration for all global platform changes
dotnet ef migrations add GlobalPlatformImplementation --startup-project ../IronLogic.Api
```

**This migration includes:**
- ? User entity: UnitSystem, PreferredCurrency, TimeZone, CountryCode
- ? SubscriptionPlan: Currency field with enum conversion
- ? UserSubscription: StripeSubscriptionId, StripeCustomerId, cancellation tracking
- ? PaymentTransaction: TaxAmount, CountryCode, RegionCode, Stripe fields, PaymentStatus enum
- ? Exercise: Maintained existing approval workflow (already implemented)
- ? Database indexes for performance optimization

---

## Step 2: Review Generated Migration

Open the generated migration file in `src/IronLogic.Infrastructure/Migrations/`:

```csharp
public partial class GlobalPlatformImplementation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ?? User Table Updates ??
        migrationBuilder.AddColumn<string>(
            name: "UnitSystem",
            table: "AspNetUsers",
            nullable: false,
            defaultValue: "Metric");

        migrationBuilder.AddColumn<string>(
            name: "PreferredCurrency",
            table: "AspNetUsers",
            maxLength: 3,
            nullable: false,
            defaultValue: "USD");

        migrationBuilder.AddColumn<string>(
            name: "TimeZone",
            table: "AspNetUsers",
            maxLength: 50,
            nullable: false,
            defaultValue: "UTC");

        migrationBuilder.AddColumn<string>(
            name: "CountryCode",
            table: "AspNetUsers",
            maxLength: 2,
            nullable: false,
            defaultValue: "US");

        // ?? SubscriptionPlan Table Updates ??
        migrationBuilder.AddColumn<string>(
            name: "Currency",
            table: "SubscriptionPlans",
            maxLength: 3,
            nullable: false,
            defaultValue: "USD");

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "SubscriptionPlans",
            nullable: false,
            defaultValue: true);

        // ?? UserSubscription Table Updates ??
        migrationBuilder.AddColumn<bool>(
            name: "AutoRenew",
            table: "UserSubscriptions",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<string>(
            name: "StripeSubscriptionId",
            table: "UserSubscriptions",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StripeCustomerId",
            table: "UserSubscriptions",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CancelledAt",
            table: "UserSubscriptions",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CancellationReason",
            table: "UserSubscriptions",
            nullable: true);

        // ?? PaymentTransaction Table Updates ??
        migrationBuilder.AddColumn<string>(
            name: "Currency",
            table: "PaymentTransactions",
            maxLength: 3,
            nullable: false,
            defaultValue: "USD");

        migrationBuilder.AddColumn<decimal>(
            name: "TaxAmount",
            table: "PaymentTransactions",
            type: "decimal(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "CountryCode",
            table: "PaymentTransactions",
            maxLength: 2,
            nullable: false,
            defaultValue: "US");

        migrationBuilder.AddColumn<string>(
            name: "RegionCode",
            table: "PaymentTransactions",
            maxLength: 3,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StripeSubscriptionId",
            table: "PaymentTransactions",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StripeInvoiceId",
            table: "PaymentTransactions",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PaymentMethod",
            table: "PaymentTransactions",
            nullable: false,
            defaultValue: "card");

        migrationBuilder.AddColumn<string>(
            name: "PaymentMethodLast4",
            table: "PaymentTransactions",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ProcessedAt",
            table: "PaymentTransactions",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ErrorMessage",
            table: "PaymentTransactions",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "RefundAmount",
            table: "PaymentTransactions",
            type: "decimal(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefundedAt",
            table: "PaymentTransactions",
            nullable: true);

        // ?? Indexes ??
        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_CountryCode",
            table: "AspNetUsers",
            column: "CountryCode");

        migrationBuilder.CreateIndex(
            name: "IX_SubscriptionPlans_Currency_IsActive",
            table: "SubscriptionPlans",
            columns: new[] { "Currency", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PaymentTransactions_CountryCode_Currency",
            table: "PaymentTransactions",
            columns: new[] { "CountryCode", "Currency" });

        migrationBuilder.CreateIndex(
            name: "IX_PaymentTransactions_StripeSubscriptionId",
            table: "PaymentTransactions",
            column: "StripeSubscriptionId");
    }
}
```

---

## Step 3: Test Migration Locally (Development)

```bash
# Apply migration to local development database
dotnet ef database update --startup-project ../IronLogic.Api

# Verify migration success
dotnet ef migrations list --startup-project ../IronLogic.Api
```

**Expected Output:**
```
20240408123456_InitialCreate
20240410095432_FinancialAndApproval
20240415142033_GlobalPlatformImplementation (Pending)
```

---

## Step 4: Verify Database Schema

Connect to your database and verify the changes:

### **AspNetUsers Table**
```sql
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
AND COLUMN_NAME IN ('UnitSystem', 'PreferredCurrency', 'TimeZone', 'CountryCode');
```

**Expected Result:**
| COLUMN_NAME | DATA_TYPE | MAX_LENGTH | DEFAULT |
|-------------|-----------|------------|---------|
| UnitSystem | nvarchar | - | 'Metric' |
| PreferredCurrency | nvarchar | 3 | 'USD' |
| TimeZone | nvarchar | 50 | 'UTC' |
| CountryCode | nvarchar | 2 | 'US' |

---

### **PaymentTransactions Table**
```sql
SELECT COLUMN_NAME, DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PaymentTransactions'
AND COLUMN_NAME IN ('TaxAmount', 'RefundAmount', 'Currency');
```

**Expected Result:**
| COLUMN_NAME | DATA_TYPE | PRECISION | SCALE |
|-------------|-----------|-----------|-------|
| TaxAmount | decimal | 18 | 2 |
| RefundAmount | decimal | 18 | 2 |
| Currency | nvarchar | - | - |

---

## Step 5: Data Migration (Existing Records)

If you have existing data, update it to match the new schema:

```sql
-- Update existing users to default values (if not already set)
UPDATE AspNetUsers
SET 
    UnitSystem = 'Metric',
    PreferredCurrency = 'USD',
    TimeZone = 'UTC',
    CountryCode = 'US'
WHERE UnitSystem IS NULL OR TimeZone IS NULL;

-- Update existing subscription plans with currency
UPDATE SubscriptionPlans
SET 
    Currency = 'USD',
    IsActive = 1
WHERE Currency IS NULL;

-- Update existing payment transactions with default country
UPDATE PaymentTransactions
SET 
    CountryCode = 'US',
    TaxAmount = 0,
    RefundAmount = 0,
    Currency = 'USD'
WHERE CountryCode IS NULL;
```

---

## Step 6: Production Deployment

### Pre-Deployment Checklist
- [ ] Backup production database
- [ ] Test migration on staging environment
- [ ] Notify users of maintenance window (if downtime required)
- [ ] Prepare rollback script

### Deployment Steps

```bash
# 1. Backup database
# Use your database provider's backup tool (Azure SQL, AWS RDS, etc.)

# 2. Generate SQL script (for review)
dotnet ef migrations script --startup-project ../IronLogic.Api --output migration.sql

# 3. Review SQL script for any potential issues

# 4. Apply migration to production
dotnet ef database update --startup-project ../IronLogic.Api --connection "YOUR_PRODUCTION_CONNECTION_STRING"
```

---

## Step 7: Rollback (If Needed)

If you encounter issues, rollback to the previous migration:

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName --startup-project ../IronLogic.Api

# Example:
dotnet ef database update FinancialAndApproval --startup-project ../IronLogic.Api
```

Or manually restore from backup:
```sql
-- Restore from backup (SQL Server example)
RESTORE DATABASE IronLogicDB FROM DISK = 'C:\Backups\IronLogicDB_PreMigration.bak';
```

---

## Step 8: Post-Migration Verification

Run these queries to ensure data integrity:

```sql
-- Check user preferences
SELECT TOP 10 
    Id, UserName, UnitSystem, PreferredCurrency, TimeZone, CountryCode
FROM AspNetUsers;

-- Check subscription plans with currency
SELECT Id, Name, Price, Currency, IsActive
FROM SubscriptionPlans;

-- Check payment transactions with tax
SELECT TOP 10
    Id, Amount, Currency, TaxAmount, CountryCode, Status
FROM PaymentTransactions
ORDER BY DateCreated DESC;

-- Check exercise approval status
SELECT Status, COUNT(*) as Count
FROM Exercises
GROUP BY Status;
```

---

## Step 9: Update Application Settings

### **appsettings.Production.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_PRODUCTION_CONNECTION_STRING"
  },
  "Stripe": {
    "ApiKey": "sk_live_...",
    "WebhookSecret": "whsec_...",
    "PublishableKey": "pk_live_..."
  },
  "Azure": {
    "BlobStorageConnectionString": "...",
    "BlobContainerName": "exercise-images"
  }
}
```

---

## Step 10: Monitor Application

After deployment, monitor:
- ? API health checks: `/health`
- ? Database connection pool metrics
- ? Payment transaction creation (test with Stripe test mode first)
- ? Exercise approval workflow
- ? User registration with new fields

---

## Common Issues & Solutions

### Issue 1: Migration Timeout
**Symptom:** Migration hangs or times out  
**Solution:**
```bash
# Increase command timeout
dotnet ef database update --startup-project ../IronLogic.Api --command-timeout 300
```

### Issue 2: Nullable Reference Warnings
**Symptom:** Compiler warnings about nullable properties  
**Solution:** Ensure all new properties have default values or are marked as nullable (`?`)

### Issue 3: Enum Conversion Errors
**Symptom:** `Cannot convert string to PaymentStatus`  
**Solution:** Verify enum conversion configuration in `AppDbContext`:
```csharp
entity.Property(pt => pt.Status).HasConversion<string>();
```

### Issue 4: Index Creation Failures
**Symptom:** Duplicate index or foreign key errors  
**Solution:**
```sql
-- Drop conflicting index manually before migration
DROP INDEX IF EXISTS IX_PaymentTransactions_CountryCode_Currency;
```

---

## Performance Tuning

After migration, update statistics and rebuild indexes:

```sql
-- Update statistics for better query performance
UPDATE STATISTICS AspNetUsers;
UPDATE STATISTICS SubscriptionPlans;
UPDATE STATISTICS UserSubscriptions;
UPDATE STATISTICS PaymentTransactions;

-- Rebuild fragmented indexes
ALTER INDEX ALL ON AspNetUsers REBUILD;
ALTER INDEX ALL ON PaymentTransactions REBUILD;
```

---

## Monitoring Queries

### Track Migration Impact
```sql
-- Check database size growth
EXEC sp_spaceused;

-- Check index usage
SELECT 
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) IN ('AspNetUsers', 'PaymentTransactions', 'SubscriptionPlans')
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;
```

---

## Next Steps

After successful migration:

1. **Configure Stripe Integration**
   - Set up webhook endpoints
   - Test subscription creation flow
   - Verify tax calculation accuracy

2. **Implement Azure Blob Storage**
   - Configure storage account
   - Create container for exercise images
   - Update `IFileStorageService` implementation

3. **Add Data Seeding**
   - Create default subscription plans (Basic, Pro, Elite) in multiple currencies
   - Seed approved exercises for all users

4. **Update Frontend**
   - Add unit system toggle (Metric/Imperial)
   - Display currency based on user preference
   - Show timezone-aware timestamps

---

## Support

For migration issues:
- Check EF Core logs: `LogLevel.Information` in `appsettings.Development.json`
- Review migration SQL script: `dotnet ef migrations script`
- Contact database administrator for production assistance

**Migration Complete!** ??
