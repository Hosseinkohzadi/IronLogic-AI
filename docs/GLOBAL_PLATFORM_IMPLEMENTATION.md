# Global Platform Architecture Implementation

## Overview
IronLogic AI has been architected as a truly global fitness platform supporting international users across Canada, USA, Europe, Australia, and beyond. This document outlines the comprehensive backend implementation following international standards.

---

## 1. Global Standards & Localization ?

### Timezone Support (UTC-First Architecture)
**Implementation Status:** ? Complete

All DateTime fields across entities now use UTC exclusively:
- `BaseEntity.DateCreated`: `DateTimeOffset` in UTC
- `BaseEntity.DateModified`: `DateTimeOffset` in UTC
- `Session.Date`: `DateTime` in UTC with documentation
- `UserSubscription.StartDate/EndDate`: UTC
- `PaymentTransaction.ProcessedAt/RefundedAt`: UTC

**User Entity Enhancement:**
```csharp
public string TimeZone { get; set; } = "UTC"; // IANA format: "America/Toronto", "Europe/London"
```

**Frontend Display Strategy:**
- Store all dates in UTC in the database
- Convert to user's local timezone only in the presentation layer
- Use `User.TimeZone` for accurate localization

---

### Unit System Support (Metric/Imperial)
**Implementation Status:** ? Complete

**New Enum:**
```csharp
public enum UnitSystem
{
    Metric = 0,   // kg, cm, km (Global standard)
    Imperial = 1  // lbs, inches, miles (USA, Canada, UK)
}
```

**User Entity:**
```csharp
public UnitSystem UnitSystem { get; set; } = UnitSystem.Metric;
```

**Normalization Strategy:**
- All weights stored in **kilograms (kg)** in the database
- `WorkoutParserService` automatically converts lbs ? kg during import
- Frontend converts kg ? lbs for Imperial users at display time
- `IronAiConstants.KgToLbsFactor = 2.20462262` for accurate conversion

**Example Workflow:**
1. User in Canada logs "225 lbs Bench Press"
2. Parser converts: `225 * 0.45359237 = 102.06 kg` (stored)
3. Frontend displays: `102.06 kg` (Metric) or `225 lbs` (Imperial)

---

### Multi-Currency Support (CAD, USD, EUR, GBP, AUD)
**Implementation Status:** ? Complete

**New Enum:**
```csharp
public enum Currency
{
    USD = 0, CAD = 1, EUR = 2, GBP = 3, AUD = 4
}
```

**Updated Entities:**
- `User.PreferredCurrency`: User's default currency
- `SubscriptionPlan.Currency`: Plan pricing currency
- `PaymentTransaction.Currency`: Transaction currency

**Database Configuration:**
- Stored as `string` via enum conversion (3-char codes: "USD", "CAD")
- `decimal(18,2)` precision for all financial amounts
- Indexed by `(CountryCode, Currency)` for tax lookups

**Multi-Currency Pricing Strategy:**
```csharp
// Example: Elite Plan Pricing
- USD: $149/month
- CAD: $189/month
- EUR: €139/month
- GBP: £119/month
- AUD: $209/month
```

**Frontend Integration:**
- `FinancialSettings.baseCurrency`: Admin sets base currency
- Display prices using `Intl.NumberFormat` with user's `PreferredCurrency`
- Tax calculation respects `CountryCode` and `RegionCode`

---

## 2. Financial Module (Stripe-Ready) ?

### Enhanced Entities

#### **SubscriptionPlan**
```csharp
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; }              // "Basic", "Pro", "Elite"
    public decimal Price { get; set; }             // decimal(18,2)
    public Currency Currency { get; set; }         // USD, CAD, EUR, GBP, AUD
    public int DurationDays { get; set; }          // 30, 365
    public bool IsActive { get; set; }             // Active plans only
    public string? FeaturesJson { get; set; }      // JSON feature list
}
```

**Database Indexes:**
- `(Currency, IsActive)` - Fast lookup of active plans by currency

---

#### **UserSubscription**
```csharp
public class UserSubscription : BaseEntity
{
    public string UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }        // UTC
    public DateTime EndDate { get; set; }          // UTC
    public bool IsActive { get; set; }
    public bool AutoRenew { get; set; }
    
    // ? Stripe Integration Fields
    public string? StripeSubscriptionId { get; set; }  // "sub_..."
    public string? StripeCustomerId { get; set; }      // "cus_..."
    
    // Cancellation Tracking
    public DateTime? CancelledAt { get; set; }     // UTC
    public string? CancellationReason { get; set; }
}
```

**Stripe Webhook Integration:**
- `StripeSubscriptionId`: Links to Stripe subscription object
- `StripeCustomerId`: Links to Stripe customer for payment methods
- Handle events: `customer.subscription.created`, `deleted`, `updated`

---

#### **PaymentTransaction**
```csharp
public class PaymentTransaction : BaseEntity
{
    public string UserId { get; set; }
    public decimal Amount { get; set; }            // decimal(18,2)
    public Currency Currency { get; set; }
    
    // ? Tax Calculation (Canadian GST/HST + Global)
    public decimal TaxAmount { get; set; }         // decimal(18,2)
    public string CountryCode { get; set; }        // "CA", "US", "GB"
    public string? RegionCode { get; set; }        // "ON", "BC", "CA", "NY"
    
    // ? Stripe Integration
    public string GatewayTransactionId { get; set; }  // "pi_..." or "ch_..."
    public string? StripeSubscriptionId { get; set; } // "sub_..."
    public string? StripeInvoiceId { get; set; }      // "in_..."
    
    // Payment Details
    public PaymentStatus Status { get; set; }
    public string PaymentMethod { get; set; }      // "card", "bank_transfer"
    public string? PaymentMethodLast4 { get; set; }
    public DateTime? ProcessedAt { get; set; }     // UTC
    
    // Refund Support
    public decimal RefundAmount { get; set; }      // decimal(18,2)
    public DateTime? RefundedAt { get; set; }      // UTC
    public string? ErrorMessage { get; set; }
}
```

**Canadian Tax Compliance (GST/HST):**
```csharp
// Example Tax Rates by Province
ON (Ontario):    13% HST
BC (British Columbia): 5% GST + 7% PST = 12%
AB (Alberta):    5% GST
QC (Quebec):     5% GST + 9.975% QST = 14.975%
```

**Database Indexes:**
- `GatewayTransactionId` (UNIQUE) - Prevent duplicate transactions
- `(CountryCode, Currency)` - Tax lookup optimization
- `StripeSubscriptionId` - Link payments to subscriptions

---

### PaymentStatus Enum
```csharp
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3,
    Cancelled = 4
}
```

---

## 3. Exercise Approval Flow (Logic Gate) ?

### Updated Exercise Entity
```csharp
public class Exercise : BaseEntity
{
    public string Name { get; set; }
    public ExerciseType Type { get; set; }
    public string? ImageUrl { get; set; }          // ? Azure Blob URL, not DB storage
    
    // ? Approval Workflow
    public string CreatorUserId { get; set; }      // User who created this exercise
    public ExerciseStatus Status { get; set; }     // Private, PendingApproval, Approved, Rejected
    public bool IsGlobal { get; set; }             // True if approved by admin
}
```

### ExerciseStatus Enum
```csharp
public enum ExerciseStatus
{
    Private = 0,          // Visible only to creator
    PendingApproval = 1,  // Awaiting admin review
    Approved = 2,         // Globally visible
    Rejected = 3          // Rejected by admin
}
```

---

### Critical Logic: Repository Implementation
**File:** `ExerciseRepository.cs`

```csharp
public async Task<IReadOnlyList<Exercise>> GetAvailableExercisesAsync(string userId)
{
    return await _context.Exercises
        .Where(e => e.Status == ExerciseStatus.Approved || e.CreatorUserId == userId)
        .Include(e => e.PrimaryMuscle)
        .Include(e => e.Equipment)
        .Include(e => e.SecondaryMuscles)
        .ToListAsync();
}
```

**Logic Gate Explanation:**
1. **Approved Exercises:** Visible to ALL users globally
2. **Private Exercises:** Visible ONLY to the creator (`CreatorUserId == userId`)
3. **Security:** Users NEVER see other users' private exercises

---

### Global Query Filter (Template)
**File:** `AppDbContext.OnModelCreating()`

```csharp
// Global Query Filter: Users only see exercises where Status == Approved OR CreatorUserId == currentUserId
// Note: Requires IHttpContextAccessor to inject current userId dynamically
// entity.HasQueryFilter(e => e.Status == ExerciseStatus.Approved || e.CreatorUserId == currentUserId);
```

**Implementation Strategy:**
1. Create `ICurrentUserService` to get authenticated user's ID
2. Inject service into `AppDbContext` constructor
3. Apply query filter using `currentUserService.GetUserId()`
4. Filter applies automatically to ALL queries (can be bypassed with `.IgnoreQueryFilters()`)

---

### Admin Approval Service
**File:** `ExerciseService.cs`

```csharp
public async Task<bool> ApproveExerciseAsync(Guid exerciseId)
{
    var exercise = await exerciseRepository.GetByIdAsync(exerciseId);
    if (exercise == null) return false;

    exercise.Status = ExerciseStatus.Approved;
    exercise.IsGlobal = true;
    
    exerciseRepository.Update(exercise);
    return await exerciseRepository.SaveChangesAsync();
}
```

**Authorization:**
- **Controller:** `[Authorize(Roles = "Admin")]` on `ApproveExercise` endpoint
- **Result:** Only admins can make user-generated exercises globally visible

---

## 4. Technical Specifications ?

### .NET 10 & C# 13 Features
**Primary Constructors:**
```csharp
public class ExerciseService(IExerciseRepository exerciseRepository) : IExerciseService
{
    // No explicit constructor needed - repository injected via primary constructor
}
```

**File-Scoped Namespaces:**
```csharp
namespace IronLogic.Domain.Entities;

public class Exercise : BaseEntity { }
```

**Global Usings:**
- Configure in `.csproj` or `GlobalUsings.cs`
- Reduces repetitive `using` statements

---

### Clean Architecture Compliance
```
IronLogic.Domain         ? Entities, Enums, Interfaces (Core Business Logic)
IronLogic.Application    ? Services, DTOs, Use Cases
IronLogic.Infrastructure ? DbContext, Repositories, External Services
IronLogic.Api            ? Controllers, Middleware, Startup
```

**Dependency Flow:**
```
Api ? Application ? Domain
        ?
Infrastructure ? Domain
```

---

### API DTOs (camelCase for Angular Signals)
**Example DTO:**
```csharp
public class ExerciseDto
{
    public Guid id { get; set; }              // camelCase
    public string name { get; set; }
    public string? imageUrl { get; set; }
    public string status { get; set; }        // "approved", "private"
    public string creatorUserId { get; set; }
}
```

**Serialization:**
- Use `JsonNamingPolicy.CamelCase` in API configuration
- Matches Angular `UserRow`, `UserDetail` interfaces
- TypeScript: `exerciseId`, `imageUrl`, `creatorUserId`

---

## 5. Media Handling (Azure Blob Storage Pattern) ?

### Image Storage Strategy
**NOT Stored in Database:**
- ? `Exercise.Image` (byte[]) - Removed from active use
- ? `Exercise.ImageUrl` (string) - URL to external storage

**Recommended Implementation:**
```csharp
public interface IFileStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
    Task<bool> DeleteImageAsync(string imageUrl);
}

public class AzureBlobStorageService : IFileStorageService
{
    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        // Upload to Azure Blob Storage
        // Return public URL: "https://ironlogic.blob.core.windows.net/exercises/squat-123.jpg"
    }
}
```

**Exercise Creation Workflow:**
1. User uploads exercise image via API
2. `FileStorageService.UploadImageAsync()` ? Azure Blob
3. Save returned URL to `Exercise.ImageUrl`
4. Frontend displays image from `ImageUrl`

**Benefits:**
- ? Database stays small and fast
- ? CDN-ready for global performance
- ? Supports Azure, AWS S3, Cloudinary
- ? Easy to integrate with image optimization services

---

## 6. Database Migration Checklist

### Required Migrations
```bash
# Add new enums and User fields
dotnet ef migrations add GlobalPlatformSupport --project src/IronLogic.Infrastructure

# Update SubscriptionPlan with Currency
dotnet ef migrations add MultiCurrencyPlans --project src/IronLogic.Infrastructure

# Update PaymentTransaction with tax fields
dotnet ef migrations add StripeTaxIntegration --project src/IronLogic.Infrastructure

# Update UserSubscription with Stripe fields
dotnet ef migrations add StripeSubscriptionFields --project src/IronLogic.Infrastructure

# Apply all migrations
dotnet ef database update --project src/IronLogic.Infrastructure
```

---

## 7. API Endpoints (camelCase JSON)

### Exercise Endpoints
```http
GET /api/v1/exercises/available?userId={userId}
? Returns: Exercise[] with Status == Approved OR CreatorUserId == userId

GET /api/v1/exercises/my-exercises?userId={userId}
? Returns: Exercise[] created by user (all statuses)

POST /api/v1/admin/exercise-approvals/{exerciseId}/approve
? [Authorize(Roles = "Admin")]
? Sets Status = Approved, IsGlobal = true
```

### Financial Endpoints (Future)
```http
POST /api/v1/subscriptions/create
Body: { userId, planId, paymentMethodId }
? Creates Stripe subscription, saves StripeSubscriptionId

POST /api/v1/payments/webhook
? Stripe webhook handler
? Updates UserSubscription and PaymentTransaction based on events
```

---

## 8. Frontend Integration (Angular Signals)

### User Model (TypeScript)
```typescript
export interface UserRow {
  id: string;
  userName: string;
  email: string;
  unitSystem: 'Metric' | 'Imperial';        // ? NEW
  preferredCurrency: 'USD' | 'CAD' | 'EUR'; // ? NEW
  timeZone: string;                          // ? NEW "America/Toronto"
  countryCode: string;                       // ? NEW "CA"
  tier: 'Free' | 'Basic' | 'Pro' | 'Elite';
  lastSeen: string;                          // ISO 8601 UTC
}
```

### Exercise Display Logic
```typescript
export class ExerciseListComponent {
  exercises = signal<Exercise[]>([]);
  
  ngOnInit() {
    const userId = this.authService.currentUserId();
    this.api.getAvailableExercises(userId).subscribe(data => {
      this.exercises.set(data); // Only approved + user's private exercises
    });
  }
}
```

### Currency Display
```typescript
formatMoney(amount: number): string {
  const settings = this.configService.financialSettings();
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: settings.baseCurrency, // USD, CAD, EUR
    currencyDisplay: settings.currencyDisplay // 'symbol' or 'code'
  }).format(amount);
}
```

---

## 9. Security & Compliance

### Data Privacy
- ? Never store full credit card numbers (use `PaymentMethodLast4`)
- ? Stripe handles PCI compliance
- ? `StripeCustomerId` and `StripeSubscriptionId` are safe to store

### Tax Compliance
- ? `CountryCode` + `RegionCode` enable accurate tax calculation
- ? `TaxAmount` stored separately for auditing
- ? Canadian GST/HST varies by province (handled by `RegionCode`)

### Authorization
- ? Exercise approval: `[Authorize(Roles = "Admin")]`
- ? Global query filter prevents unauthorized data access
- ? User can only see their own private exercises

---

## 10. Testing Recommendations

### Unit Tests
```csharp
[Fact]
public async Task GetAvailableExercises_ReturnsApprovedAndUserExercises()
{
    // Arrange: Seed approved exercise + user's private exercise
    // Act: Call repository.GetAvailableExercisesAsync(userId)
    // Assert: Count == 2 (approved + user's private)
}

[Fact]
public async Task ApproveExercise_SetsStatusAndIsGlobal()
{
    // Arrange: Create private exercise
    // Act: service.ApproveExerciseAsync(exerciseId)
    // Assert: Status == Approved, IsGlobal == true
}
```

### Integration Tests
```csharp
[Fact]
public async Task StripeWebhook_UpdatesSubscriptionStatus()
{
    // Simulate Stripe webhook event: subscription.updated
    // Verify UserSubscription.IsActive updated correctly
}
```

---

## 11. Performance Optimizations

### Database Indexes
```csharp
// Exercise
entity.HasIndex(e => e.Status);
entity.HasIndex(e => e.CreatorUserId);

// PaymentTransaction
entity.HasIndex(pt => new { pt.CountryCode, pt.Currency });
entity.HasIndex(pt => pt.StripeSubscriptionId);

// User
entity.HasIndex(u => u.CountryCode);
```

### Caching Strategy
```csharp
// Cache approved exercises globally (rarely change)
services.AddMemoryCache();

public class ExerciseCacheService
{
    public async Task<List<Exercise>> GetApprovedExercisesAsync()
    {
        return await cache.GetOrCreateAsync("approved-exercises", async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await repository.GetApprovedExercisesAsync();
        });
    }
}
```

---

## 12. Deployment Checklist

### Environment Variables
```bash
# Stripe Configuration
STRIPE_API_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...

# Azure Blob Storage
AZURE_STORAGE_CONNECTION_STRING=...
AZURE_STORAGE_CONTAINER_NAME=exercise-images

# Database
ConnectionStrings__DefaultConnection=...
```

### Stripe Webhook Setup
```bash
# Development
stripe listen --forward-to localhost:5000/api/v1/payments/webhook

# Production
# Configure webhook endpoint in Stripe Dashboard:
# https://api.ironlogic.ai/api/v1/payments/webhook
# Events: customer.subscription.*, invoice.*, payment_intent.*
```

---

## Summary

? **Global Standards:** UTC timestamps, multi-currency, unit system preference  
? **Financial Module:** Stripe-ready with tax calculation (Canadian GST/HST + global)  
? **Exercise Approval:** Logic gate ensures users only see approved OR own exercises  
? **Clean Architecture:** .NET 10, C# 13 primary constructors, camelCase DTOs  
? **Media Handling:** ImageUrl pattern for Azure Blob Storage integration  

**Next Steps:**
1. Run migrations to update database schema
2. Implement `IFileStorageService` for image uploads
3. Add Stripe SDK and webhook handlers
4. Create `ICurrentUserService` for global query filter
5. Add authorization policies for admin-only endpoints
