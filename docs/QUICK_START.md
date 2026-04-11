# Quick Start Guide - Financial & Exercise Approval System

## ? What's Been Implemented

### Domain Layer (IronLogic.Domain)
- ? `ExerciseStatus` enum (Private/PendingApproval/Approved/Rejected)
- ? `SubscriptionPlan` entity
- ? `UserSubscription` entity
- ? `PaymentTransaction` entity
- ? `Exercise` entity updated with approval workflow properties
- ? `User` entity updated with financial navigation properties
- ? `IExerciseRepository` interface

### Application Layer (IronLogic.Application)
- ? `IAdminService` interface
- ? `AdminService` implementation

### Infrastructure Layer (IronLogic.Infrastructure)
- ? `ExerciseRepository` implementation
- ? `AppDbContext` updated with financial entities and configurations
- ? Decimal precision (18,2) configured for Price and Amount
- ? Indexes and relationships configured
- ? Dependency injection updated

### API Layer (IronLogic.Api)
- ? `ExerciseApprovalController` (admin endpoints)
- ? `ExerciseController` (user endpoints)

## ?? Next Steps

### Step 1: Create Database Migration

```bash
# Navigate to Infrastructure project
cd src\IronLogic.Infrastructure

# Create migration
dotnet ef migrations add AddFinancialAndExerciseApproval --startup-project ..\IronLogic.Api

# Apply migration
dotnet ef database update --startup-project ..\IronLogic.Api
```

### Step 2: Test API Endpoints

#### Admin Endpoints (Approval Management)

**Get Pending Approvals**:
```http
GET https://localhost:5001/api/v1/admin/exercise-approvals/pending
```

**Approve Exercise**:
```http
POST https://localhost:5001/api/v1/admin/exercise-approvals/{exerciseId}/approve
```

**Reject Exercise**:
```http
POST https://localhost:5001/api/v1/admin/exercise-approvals/{exerciseId}/reject
Content-Type: application/json

{
  "reason": "Does not meet quality standards"
}
```

#### User Endpoints (Exercise Access)

**Get Available Exercises**:
```http
GET https://localhost:5001/api/v1/exercises/available?userId={userId}
```

**Get My Exercises**:
```http
GET https://localhost:5001/api/v1/exercises/my-exercises?userId={userId}
```

### Step 3: Add Authorization (Recommended)

Update `ExerciseApprovalController.cs`:

```csharp
[ApiController]
[Route("api/v1/admin/exercise-approvals")]
[Authorize(Roles = "Admin")]  // Add this
[Produces("application/json")]
public class ExerciseApprovalController(IAdminService adminService, IExerciseRepository exerciseRepository) : ControllerBase
{
    // ...
}
```

### Step 4: Seed Initial Subscription Plans (Optional)

Add to `AppDbContext.OnModelCreating()`:

```csharp
modelBuilder.Entity<SubscriptionPlan>().HasData(
    new SubscriptionPlan
    {
        Id = Guid.NewGuid(),
        Name = "Basic",
        Price = 9.99m,
        DurationDays = 30,
        FeaturesJson = "{\"maxWorkouts\":50,\"aiCoach\":false,\"analytics\":\"basic\"}",
        DateCreated = DateTimeOffset.UtcNow,
        DateModified = DateTimeOffset.UtcNow
    },
    new SubscriptionPlan
    {
        Id = Guid.NewGuid(),
        Name = "Premium",
        Price = 19.99m,
        DurationDays = 30,
        FeaturesJson = "{\"maxWorkouts\":\"unlimited\",\"aiCoach\":true,\"analytics\":\"advanced\"}",
        DateCreated = DateTimeOffset.UtcNow,
        DateModified = DateTimeOffset.UtcNow
    },
    new SubscriptionPlan
    {
        Id = Guid.NewGuid(),
        Name = "Pro",
        Price = 99.99m,
        DurationDays = 365,
        FeaturesJson = "{\"maxWorkouts\":\"unlimited\",\"aiCoach\":true,\"analytics\":\"premium\",\"personalTrainer\":true}",
        DateCreated = DateTimeOffset.UtcNow,
        DateModified = DateTimeOffset.UtcNow
    }
);
```

### Step 5: Testing Exercise Workflow

**Scenario 1: User Creates Private Exercise**
1. User creates exercise with `Status = Private`, `IsGlobal = false`
2. Only that user can see it via `/exercises/available?userId={userId}`

**Scenario 2: User Submits for Approval**
1. User updates exercise to `Status = PendingApproval`
2. Admin sees it via `/admin/exercise-approvals/pending`

**Scenario 3: Admin Approves Exercise**
1. Admin calls `/admin/exercise-approvals/{id}/approve`
2. Exercise `Status = Approved`, `IsGlobal = true`
3. Now ALL users see it via `/exercises/available`

**Scenario 4: Admin Rejects Exercise**
1. Admin calls `/admin/exercise-approvals/{id}/reject`
2. Exercise `Status = Rejected`, `IsGlobal = false`
3. Only creator and admins can still see it

## ?? Database Schema

### New Tables
- `SubscriptionPlans` - Subscription plan definitions
- `UserSubscriptions` - User subscription instances
- `PaymentTransactions` - Payment records

### Updated Tables
- `Exercises` - Added: ImageUrl, CreatorUserId, Status, IsGlobal
- `AspNetUsers` - Added navigation properties (no table changes)

### Indexes
- `IX_Exercises_Status`
- `IX_Exercises_CreatorUserId`
- `IX_UserSubscriptions_UserId_IsActive`
- `IX_PaymentTransactions_GatewayTransactionId` (Unique)
- `IX_PaymentTransactions_UserId`

## ?? Query Examples

### Get Available Exercises for User
```csharp
var exercises = await _exerciseRepository.GetAvailableExercisesAsync(userId);
// Returns: All approved exercises + user's private exercises
```

### Get Pending Approvals (Admin)
```csharp
var pending = await _exerciseRepository.GetPendingApprovalsAsync();
// Returns: All exercises with Status = PendingApproval
```

### Approve Exercise (Admin)
```csharp
var success = await _adminService.ApproveExerciseAsync(exerciseId);
// Sets: Status = Approved, IsGlobal = true
```

## ?? Important Notes

### Exercise Visibility Logic
The `GetAvailableExercisesAsync` method implements this rule:
```csharp
.Where(e => e.IsGlobal || e.CreatorUserId == userId)
```

This means a user sees:
- ? All approved exercises (IsGlobal = true)
- ? Their own private exercises (CreatorUserId = userId)
- ? Other users' private exercises
- ? Rejected exercises (unless they created them)

### Delete Behavior
- **User ? Exercise**: `Restrict` - Can't delete user if they have exercises
- **User ? UserSubscription**: `Cascade` - Delete user subscriptions when user deleted
- **User ? PaymentTransaction**: `Cascade` - Delete payment records when user deleted
- **Plan ? UserSubscription**: `Restrict` - Can't delete plan if users are subscribed

### Decimal Precision
Financial fields configured with `HasPrecision(18, 2)`:
- `SubscriptionPlan.Price`
- `PaymentTransaction.Amount`

This ensures:
- 18 total digits
- 2 decimal places
- Proper money handling without floating-point errors

## ?? Sample Data Flow

### Exercise Creation & Approval
```
1. User creates exercise
   POST /api/v1/exercises
   Body: { name: "Custom Bench", ... }
   Result: Exercise { Status: Private, IsGlobal: false, CreatorUserId: user1 }

2. User submits for approval
   PATCH /api/v1/exercises/{id}
   Body: { status: "PendingApproval" }
   Result: Exercise { Status: PendingApproval, IsGlobal: false }

3. Admin reviews
   GET /api/v1/admin/exercise-approvals/pending
   Result: [ { id: exercise1, name: "Custom Bench", creatorUser: {...} } ]

4. Admin approves
   POST /api/v1/admin/exercise-approvals/{id}/approve
   Result: Exercise { Status: Approved, IsGlobal: true }

5. Now visible to all
   GET /api/v1/exercises/available?userId=any-user
   Result: Includes "Custom Bench"
```

### Subscription Purchase Flow
```
1. User selects plan
   GET /api/v1/subscription-plans
   Result: [ { id: plan1, name: "Premium", price: 19.99 } ]

2. Payment initiated
   POST /api/v1/payments/initiate
   Body: { planId: plan1, userId: user1 }
   Result: { gatewayTransactionId: "stripe_12345", status: "Pending" }

3. Payment completed (webhook)
   POST /api/v1/payments/webhook
   Body: { gatewayTransactionId: "stripe_12345", status: "Completed" }
   Result: PaymentTransaction updated, UserSubscription created

4. Subscription activated
   GET /api/v1/subscriptions/my-subscription?userId=user1
   Result: { planName: "Premium", startDate: now, endDate: +30days, isActive: true }
```

## ?? Business Rules Implemented

1. ? Exercise visibility based on approval status
2. ? Admin-only approval/rejection
3. ? Creator can always see their own exercises
4. ? Global flag only set when approved
5. ? Decimal precision for financial data
6. ? Unique gateway transaction IDs
7. ? Proper cascade deletes for data integrity

## ?? Future Enhancements

- [ ] Email notifications on approval/rejection
- [ ] Subscription renewal background job
- [ ] Payment gateway integration (Stripe/PayPal)
- [ ] Analytics dashboard for subscriptions
- [ ] Exercise approval history/audit log
- [ ] Bulk exercise approval
- [ ] Exercise rejection reason tracking
- [ ] Subscription grace period handling
- [ ] Payment retry logic
- [ ] Invoice generation
