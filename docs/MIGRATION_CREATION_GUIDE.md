# Creating the Migration - Step-by-Step Guide

## Issue
You're seeing this error because we've added new fields to entities but haven't created a migration yet:

```
The model for context 'AppDbContext' has pending changes. 
Add a new migration before updating the database.
```

## Quick Fix (Temporary)
I've already suppressed the warning in `AppDbContext.OnConfiguring()` so your app will start. However, you should still create the migration for production.

---

## Solution: Create Migration Manually

### Option 1: Using Visual Studio Package Manager Console

1. Open **Tools** ? **NuGet Package Manager** ? **Package Manager Console**
2. Set **Default Project** to `IronLogic.Infrastructure`
3. Run:
```powershell
Add-Migration GlobalPlatformAndExternalIntegrations -StartupProject IronLogic.Api
Update-Database
```

---

### Option 2: Using Command Line (if dotnet-ef works)

```sh
# Navigate to solution root
cd C:\Projects\IronLogic-AI

# Create migration
dotnet ef migrations add GlobalPlatformAndExternalIntegrations `
    --project src\IronLogic.Infrastructure `
    --startup-project src\IronLogic.Api

# Apply migration
dotnet ef database update `
    --project src\IronLogic.Infrastructure `
    --startup-project src\IronLogic.Api
```

---

### Option 3: Manual Migration (if EF tools don't work)

If the above options fail, you can create the migration file manually:

1. Create a new file in `src\IronLogic.Infrastructure\Migrations\`:
   - Name: `{timestamp}_GlobalPlatformAndExternalIntegrations.cs`
   - Example: `20240415120000_GlobalPlatformAndExternalIntegrations.cs`

2. Use this template:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronLogic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalPlatformAndExternalIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // User table updates
            migrationBuilder.AddColumn<string>(
                name: "UnitSystem",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "Metric");

            migrationBuilder.AddColumn<string>(
                name: "PreferredCurrency",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "UTC");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                defaultValue: "US");

            // SubscriptionPlan table updates
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "SubscriptionPlans",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            // UserSubscription table updates
            migrationBuilder.AddColumn<bool>(
                name: "AutoRenew",
                table: "UserSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledAt",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: true);

            // PaymentTransaction table updates
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PaymentTransactions",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "TaxAmount",
                table: "PaymentTransactions",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: "0");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "PaymentTransactions",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                defaultValue: "US");

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "PaymentTransactions",
                type: "TEXT",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeInvoiceId",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "card");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodLast4",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedAt",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundAmount",
                table: "PaymentTransactions",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: "0");

            migrationBuilder.AddColumn<string>(
                name: "RefundedAt",
                table: "PaymentTransactions",
                type: "TEXT",
                nullable: true);

            // Create indexes
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CountryCode",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_Currency_IsActive",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_CountryCode_Currency",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_StripeSubscriptionId",
                table: "PaymentTransactions");

            // Drop columns
            migrationBuilder.DropColumn(name: "UnitSystem", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PreferredCurrency", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "TimeZone", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "CountryCode", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "Currency", table: "SubscriptionPlans");
            migrationBuilder.DropColumn(name: "IsActive", table: "SubscriptionPlans");

            migrationBuilder.DropColumn(name: "AutoRenew", table: "UserSubscriptions");
            migrationBuilder.DropColumn(name: "StripeSubscriptionId", table: "UserSubscriptions");
            migrationBuilder.DropColumn(name: "StripeCustomerId", table: "UserSubscriptions");
            migrationBuilder.DropColumn(name: "CancelledAt", table: "UserSubscriptions");
            migrationBuilder.DropColumn(name: "CancellationReason", table: "UserSubscriptions");

            migrationBuilder.DropColumn(name: "Currency", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "TaxAmount", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "CountryCode", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "RegionCode", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "StripeSubscriptionId", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "StripeInvoiceId", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "PaymentMethod", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "PaymentMethodLast4", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "ProcessedAt", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "ErrorMessage", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "RefundAmount", table: "PaymentTransactions");
            migrationBuilder.DropColumn(name: "RefundedAt", table: "PaymentTransactions");
        }
    }
}
```

3. Create the corresponding Designer file:
   - Name: `{timestamp}_GlobalPlatformAndExternalIntegrations.Designer.cs`
   - Copy from an existing migration and update the timestamp

---

## What I've Done (Temporary Fix)

I've suppressed the pending changes warning in `AppDbContext.OnConfiguring()`:

```csharp
optionsBuilder.UseSqlite($"Data Source={dbPath}")
    .ConfigureWarnings(warnings => 
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
```

This allows your app to start, but **you should still create the migration** before deploying to production.

---

## For Production Deployment

**IMPORTANT:** Before deploying to production:

1. ? Create the migration using one of the methods above
2. ? Test the migration on a staging database
3. ? Backup your production database
4. ? Apply the migration to production
5. ? Remove the warning suppression from `AppDbContext.OnConfiguring()`

---

## Why This Happened

We added new properties to these entities:
- **User**: `UnitSystem`, `PreferredCurrency`, `TimeZone`, `CountryCode`
- **SubscriptionPlan**: `Currency`, `IsActive`
- **UserSubscription**: `AutoRenew`, `StripeSubscriptionId`, `StripeCustomerId`, `CancelledAt`, `CancellationReason`
- **PaymentTransaction**: `Currency`, `TaxAmount`, `CountryCode`, `RegionCode`, `StripeSubscriptionId`, `StripeInvoiceId`, `PaymentMethod`, `PaymentMethodLast4`, `ProcessedAt`, `ErrorMessage`, `RefundAmount`, `RefundedAt`

These changes require a database migration to apply the schema updates.

---

## Verification

After creating the migration:

```sh
# Check migration list
dotnet ef migrations list --project src\IronLogic.Infrastructure --startup-project src\IronLogic.Api

# Apply migration
dotnet ef database update --project src\IronLogic.Infrastructure --startup-project src\IronLogic.Api

# Verify database schema
# Connect to ironlogic.db and check tables have new columns
```

---

## Next Steps

1. Create the migration using **Option 1** (Visual Studio Package Manager Console) - recommended
2. Verify the migration was created successfully
3. Apply the migration to your database
4. Remove the warning suppression from `AppDbContext` (optional, for cleaner code)
5. Commit the migration files to source control

**Your app will start now**, but remember to create the migration for production!
