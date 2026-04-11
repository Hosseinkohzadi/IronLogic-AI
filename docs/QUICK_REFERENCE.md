# IronLogic AI - Quick Reference Card

## ?? Global Platform Standards

### Timezone
- **Storage:** Always UTC in database
- **Display:** Convert using `User.TimeZone` (IANA format)
- **Example:** `"America/Toronto"`, `"Europe/London"`, `"UTC"`

### Unit System
- **Enum:** `Metric | Imperial`
- **Storage:** All weights in **kilograms (kg)**
- **Display:** Convert to lbs for Imperial users (multiply kg by 2.20462262)

### Currency
- **Supported:** `USD | CAD | EUR | GBP | AUD`
- **User Preference:** `User.PreferredCurrency`
- **Display:** Use `Intl.NumberFormat` with user's currency

---

## ?? Financial Entities

### SubscriptionPlan
```csharp
public decimal Price { get; set; }      // decimal(18,2)
public Currency Currency { get; set; }  // USD, CAD, EUR, etc.
public int DurationDays { get; set; }   // 30, 365
public bool IsActive { get; set; }
```

### UserSubscription
```csharp
public string? StripeSubscriptionId { get; set; }  // "sub_..."
public string? StripeCustomerId { get; set; }      // "cus_..."
public DateTime? CancelledAt { get; set; }         // UTC
```

### PaymentTransaction
```csharp
public decimal Amount { get; set; }         // decimal(18,2)
public decimal TaxAmount { get; set; }      // decimal(18,2)
public Currency Currency { get; set; }
public string CountryCode { get; set; }     // "CA", "US", "GB"
public string? RegionCode { get; set; }      // "ON", "BC", "CA"
public PaymentStatus Status { get; set; }   // Pending, Completed, Failed, Refunded
```

---

## ??? Exercise Approval Workflow

### Exercise Entity
```csharp
public string CreatorUserId { get; set; }
public ExerciseStatus Status { get; set; }  // Private, PendingApproval, Approved, Rejected
public bool IsGlobal { get; set; }
public string? ImageUrl { get; set; }       // Azure Blob URL
```

### Logic Gate
```csharp
// Users see: Approved exercises OR their own private exercises
.Where(e => e.Status == ExerciseStatus.Approved || e.CreatorUserId == userId)
```

### Admin Approval
```csharp
exercise.Status = ExerciseStatus.Approved;
exercise.IsGlobal = true;
```

---

## ?? Key Services

### ExerciseService
```csharp
Task<IReadOnlyList<Exercise>> GetAvailableExercisesAsync(string userId)
Task<bool> ApproveExerciseAsync(Guid exerciseId)  // Admin only
Task<IReadOnlyList<Exercise>> GetPendingApprovalsAsync()
```

### IFileStorageService (Azure Blob)
```csharp
Task<string> UploadImageAsync(Stream imageStream, string fileName)
Task<bool> DeleteImageAsync(string imageUrl)
```

---

## ?? Database Indexes

```sql
-- User
CREATE INDEX IX_AspNetUsers_CountryCode ON AspNetUsers(CountryCode);

-- SubscriptionPlan
CREATE INDEX IX_SubscriptionPlans_Currency_IsActive;

-- PaymentTransaction
CREATE INDEX IX_PaymentTransactions_CountryCode_Currency;
CREATE INDEX IX_PaymentTransactions_StripeSubscriptionId;
CREATE UNIQUE INDEX IX_PaymentTransactions_GatewayTransactionId;
```

---

## ?? API Endpoints

### Exercises
```http
GET /api/v1/exercises/available?userId={userId}
GET /api/v1/exercises/my-exercises?userId={userId}
```

### Admin (Requires: `[Authorize(Roles = "Admin")]`)
```http
GET /api/v1/admin/exercise-approvals/pending
POST /api/v1/admin/exercise-approvals/{exerciseId}/approve
POST /api/v1/admin/exercise-approvals/{exerciseId}/reject
```

---

## ?? Angular Integration (camelCase)

### TypeScript Interface
```typescript
interface UserRow {
  id: string;
  userName: string;
  unitSystem: 'Metric' | 'Imperial';
  preferredCurrency: 'USD' | 'CAD' | 'EUR' | 'GBP' | 'AUD';
  timeZone: string;
  countryCode: string;
}
```

### Currency Formatting
```typescript
formatMoney(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: this.user.preferredCurrency,
    currencyDisplay: 'symbol'
  }).format(amount);
}
```

---

## ?? Security Checklist

- ? Never store full credit card numbers (use `PaymentMethodLast4`)
- ? All sensitive operations require `[Authorize]`
- ? Admin operations require `[Authorize(Roles = "Admin")]`
- ? Exercise logic gate prevents unauthorized access
- ? Stripe handles PCI compliance

---

## ?? Migration Commands

```bash
# Create migration
dotnet ef migrations add MigrationName --startup-project src/IronLogic.Api

# Review SQL
dotnet ef migrations script --startup-project src/IronLogic.Api

# Apply migration
dotnet ef database update --startup-project src/IronLogic.Api
```

---

## ?? Deployment Checklist

- [ ] Backup production database
- [ ] Test migration on staging
- [ ] Configure Stripe webhook
- [ ] Set up Azure Blob Storage
- [ ] Update connection strings
- [ ] Run database migration
- [ ] Verify API health checks
- [ ] Monitor payment transactions

---

## ?? Documentation Files

- `GLOBAL_PLATFORM_IMPLEMENTATION.md` - Full architecture guide
- `DATABASE_MIGRATION_GUIDE.md` - Step-by-step migration
- `EXERCISE_SERVICE_IMPLEMENTATION.md` - ExerciseService details
- `IMPLEMENTATION_SUMMARY.md` - Executive summary

---

## ?? Best Practices

### Always Use UTC
```csharp
// ? Bad
DateTime.Now

// ? Good
DateTime.UtcNow
```

### Decimal Precision for Money
```csharp
// ? Bad
public float Amount { get; set; }

// ? Good
public decimal Amount { get; set; }  // Configured as decimal(18,2)
```

### Image Storage
```csharp
// ? Bad
public byte[] Image { get; set; }  // Bloats database

// ? Good
public string? ImageUrl { get; set; }  // Azure Blob URL
```

### API JSON Casing
```csharp
// C# Property
public string CreatorUserId { get; set; }

// JSON Output (camelCase)
{ "creatorUserId": "user-123" }
```

---

## ??? Troubleshooting

### Migration Timeout
```bash
dotnet ef database update --command-timeout 300
```

### Enum Conversion Error
```csharp
// Ensure in AppDbContext:
entity.Property(e => e.Status).HasConversion<string>();
```

### Build Error: Nullable Reference
```csharp
// Add default value or mark nullable
public string? OptionalField { get; set; }
public string RequiredField { get; set; } = string.Empty;
```

---

## ?? Support

- **Build Issues:** Check build logs for detailed errors
- **Migration Issues:** Review generated SQL script
- **Stripe Issues:** Check Stripe Dashboard ? Developers ? Webhooks
- **Azure Issues:** Check Azure Portal ? Storage Account ? Monitoring

---

**Version:** 1.0  
**Last Updated:** 2024  
**Build Status:** ? Successful
