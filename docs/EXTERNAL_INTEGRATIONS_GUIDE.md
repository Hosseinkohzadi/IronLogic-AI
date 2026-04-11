# External Integrations & Global Billing Infrastructure

## Overview
This document describes the implementation of Stripe payment processing and Azure Blob Storage integration for the IronLogic AI platform, designed to support global users in Canada, USA, Europe, Australia, and beyond.

---

## 1. Stripe Payment Integration

### Architecture

```
User Request ? FinancialController ? StripeService ? Stripe API
                                    ?
                            SubscriptionService ? Database
                                    ?
                            PaymentTransaction (Recorded)
```

### Features

? **Multi-Currency Support:** USD, CAD, EUR, GBP, AUD  
? **Canadian Tax Compliance:** Automatic GST/HST calculation by province  
? **Subscription Management:** Create, activate, cancel subscriptions  
? **Webhook Processing:** Real-time subscription status updates  
? **Secure Configuration:** All keys via `IOptions<StripeSettings>`  

---

### Components

#### 1.1 StripeSettings Configuration

**File:** `src/IronLogic.Domain/Settings/StripeSettings.cs`

```csharp
public class StripeSettings
{
    public string SecretKey { get; set; }        // "sk_test_..." or "sk_live_..."
    public string PublishableKey { get; set; }   // "pk_test_..." or "pk_live_..."
    public string WebhookSecret { get; set; }    // "whsec_..."
    public string SuccessUrl { get; set; }       // Redirect after success
    public string CancelUrl { get; set; }        // Redirect after cancel
    public bool UseStripeTax { get; set; }       // Auto tax calculation
    public decimal DefaultCanadianTaxRate { get; set; } // 0.13 (13% HST Ontario)
}
```

**Configuration (appsettings.json):**

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "SuccessUrl": "https://app.ironlogic.ai/subscription/success",
    "CancelUrl": "https://app.ironlogic.ai/subscription/cancel",
    "UseStripeTax": true,
    "DefaultCanadianTaxRate": 0.13
  }
}
```

---

#### 1.2 IStripeService Interface

**File:** `src/IronLogic.Application/Interfaces/IStripeService.cs`

**Methods:**

```csharp
// Create checkout session with multi-currency and tax
Task<string> CreateCheckoutSessionAsync(
    Guid planId,
    string userEmail,
    string userId,
    Currency currency,
    string countryCode,
    string? regionCode = null);

// Process webhook events (invoice.paid, subscription.deleted, etc.)
Task<bool> HandleWebhookAsync(string json, string stripeSignature);

// Get or create Stripe customer
Task<string> GetOrCreateCustomerAsync(string userId, string userEmail);

// Cancel subscription
Task<bool> CancelSubscriptionAsync(string stripeSubscriptionId);

// Calculate tax (Canadian GST/HST by province)
decimal CalculateTaxAmount(decimal subtotal, string countryCode, string? regionCode);
```

---

#### 1.3 StripeService Implementation

**File:** `src/IronLogic.Infrastructure/Services/Payment/StripeService.cs`

**Key Features:**

1. **Checkout Session Creation:**
   - Supports monthly/yearly subscriptions
   - Applies Canadian tax rates automatically
   - Stores metadata (userId, planId, countryCode, regionCode, taxAmount)

2. **Canadian Tax Calculation:**

```csharp
private decimal GetCanadianTaxRate(string? regionCode)
{
    return regionCode?.ToUpperInvariant() switch
    {
        "ON" => 0.13m,    // Ontario HST 13%
        "BC" => 0.12m,    // BC GST 5% + PST 7% = 12%
        "AB" => 0.05m,    // Alberta GST 5%
        "QC" => 0.14975m, // Quebec GST 5% + QST 9.975%
        "NB" => 0.15m,    // New Brunswick HST 15%
        "NS" => 0.15m,    // Nova Scotia HST 15%
        "PE" => 0.15m,    // PEI HST 15%
        "NL" => 0.15m,    // Newfoundland HST 15%
        "MB" => 0.12m,    // Manitoba GST 5% + PST 7%
        "SK" => 0.11m,    // Saskatchewan GST 5% + PST 6%
        _ => 0.13m        // Default Ontario HST
    };
}
```

3. **Webhook Event Handling:**

| Event | Action |
|-------|--------|
| `checkout.session.completed` | Activate subscription, record payment |
| `invoice.paid` | Record payment for renewal |
| `customer.subscription.deleted` | Deactivate subscription |
| `customer.subscription.updated` | Update subscription status |

---

### 1.4 SubscriptionService

**File:** `src/IronLogic.Application/Services/SubscriptionService.cs`

**Business Logic:**

```csharp
// Activate subscription after successful payment
Task<UserSubscription> ActivateSubscriptionAsync(
    string userId,
    Guid planId,
    string stripeSubscriptionId,
    string stripeCustomerId,
    decimal amount,
    decimal taxAmount,
    Currency currency,
    string countryCode,
    string? regionCode = null);

// Deactivate subscription (sets IsActive = false, CancelledAt = UTC now)
Task<bool> DeactivateSubscriptionAsync(
    string stripeSubscriptionId,
    string? cancellationReason = null);

// Get active subscription for user
Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);

// Record payment transaction with tax details
Task<PaymentTransaction> RecordPaymentAsync(
    string userId,
    decimal amount,
    decimal taxAmount,
    Currency currency,
    string gatewayTransactionId,
    string? stripeSubscriptionId = null,
    string? stripeInvoiceId = null,
    string? countryCode = null,
    string? regionCode = null);
```

**UTC Timestamps:** All dates use `DateTime.UtcNow` for global consistency.

---

### 1.5 FinancialController

**File:** `src/IronLogic.Api/Controllers/FinancialController.cs`

**Endpoints:**

#### POST `/api/v1/financial/checkout/create`
Creates a Stripe Checkout Session.

**Request Body:**
```json
{
  "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "user-123",
  "userEmail": "user@example.com",
  "currency": "CAD",
  "countryCode": "CA",
  "regionCode": "ON"
}
```

**Response:**
```json
{
  "sessionId": "cs_test_...",
  "checkoutUrl": "https://checkout.stripe.com/pay/cs_test_..."
}
```

---

#### POST `/api/v1/financial/webhook`
Webhook endpoint for Stripe events.

**Headers:**
- `Stripe-Signature`: Webhook signature for verification

**Response:**
- `200 OK`: Webhook processed successfully
- `400 Bad Request`: Signature verification failed

---

#### GET `/api/v1/financial/stats`
**[Authorize(Roles = "Admin")]**

Returns aggregated revenue and subscription statistics for Financial Dashboard.

**Query Parameters:**
- `baseCurrency` (optional): Currency for aggregation (default: USD)

**Response:**
```json
{
  "monthlyRevenue": 17100.00,
  "yearlyRevenue": 205200.00,
  "activeSubscriptions": 42,
  "pendingPayments": 3,
  "churnRate": 4.8,
  "revenueGrowth": 12.5,
  "baseCurrency": "CAD",
  "monthlyRevenueData": [
    { "month": "Jan", "amount": 13850 },
    { "month": "Feb", "amount": 14700 },
    { "month": "Mar", "amount": 15950 },
    { "month": "Apr", "amount": 17100 }
  ]
}
```

**Angular Integration:**
- DTO uses camelCase for seamless `FinancialDashboard` integration
- Matches TypeScript `RevenuePoint` and `PaymentRecord` interfaces

---

#### GET `/api/v1/financial/subscription/{userId}`
Retrieves the active subscription for a user.

**Response:**
```json
{
  "id": "...",
  "userId": "user-123",
  "planId": "...",
  "startDate": "2024-04-15T00:00:00Z",
  "endDate": "2024-05-15T00:00:00Z",
  "isActive": true,
  "stripeSubscriptionId": "sub_...",
  "stripeCustomerId": "cus_..."
}
```

---

#### POST `/api/v1/financial/subscription/{stripeSubscriptionId}/cancel`
Cancels a subscription at the end of the billing period.

**Response:**
```json
{
  "message": "Subscription cancelled successfully"
}
```

---

## 2. Azure Blob Storage Integration

### Architecture

```
User Upload ? API Controller ? AzureBlobStorageService ? Azure Blob Storage
                                        ?
                                  Return CDN URL
                                        ?
                               Store in Exercise.ImageUrl
```

### Features

? **Global CDN:** Azure CDN for fast image delivery worldwide  
? **File Validation:** Size limits (default: 5MB)  
? **Unique Naming:** `exercise-{guid}.jpg`  
? **MIME Types:** Supports image/jpeg, image/png  
? **Public Access:** Blob-level public read access  

---

### Components

#### 2.1 AzureStorageSettings Configuration

**File:** `src/IronLogic.Domain/Settings/AzureStorageSettings.cs`

```csharp
public class AzureStorageSettings
{
    public string ConnectionString { get; set; }
    public string ContainerName { get; set; } = "exercise-images";
    public string BaseUrl { get; set; }
    public bool UseCdn { get; set; } = true;
    public string? CdnEndpoint { get; set; }
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB
}
```

**Configuration (appsettings.json):**

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=ironlogic;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "exercise-images",
    "BaseUrl": "https://ironlogic.blob.core.windows.net",
    "UseCdn": true,
    "CdnEndpoint": "https://ironlogic.azureedge.net",
    "MaxFileSizeBytes": 5242880
  }
}
```

---

#### 2.2 IFileStorageService Interface

**File:** `src/IronLogic.Application/Interfaces/IFileStorageService.cs`

**Methods:**

```csharp
// Upload file stream to cloud storage
Task<string> UploadAsync(Stream fileStream, string fileName, string contentType = "image/jpeg");

// Delete file from cloud storage
Task<bool> DeleteAsync(string fileUrl);

// Check if file exists
Task<bool> ExistsAsync(string fileName);

// Generate unique file name
string GenerateUniqueFileName(string originalFileName);
```

---

#### 2.3 AzureBlobStorageService Implementation

**File:** `src/IronLogic.Infrastructure/Services/Storage/AzureBlobStorageService.cs`

**Key Features:**

1. **File Upload:**
   - Validates file size against `MaxFileSizeBytes`
   - Sets `Content-Type` and `Cache-Control` headers
   - Returns CDN URL if enabled, otherwise blob URL

2. **File Deletion:**
   - Extracts file name from URL
   - Deletes blob from container

3. **URL Generation:**
```csharp
private string GetPublicUrl(string fileName)
{
    if (_settings.UseCdn && !string.IsNullOrWhiteSpace(_settings.CdnEndpoint))
        return $"{_settings.CdnEndpoint}/{_settings.ContainerName}/{fileName}";
    
    return $"{_settings.BaseUrl}/{_settings.ContainerName}/{fileName}";
}
```

**Example URLs:**
- **CDN:** `https://ironlogic.azureedge.net/exercise-images/exercise-abc123.jpg`
- **Blob:** `https://ironlogic.blob.core.windows.net/exercise-images/exercise-abc123.jpg`

---

## 3. Database Schema Impact

### UserSubscription Entity
```csharp
public string StripeSubscriptionId { get; set; }  // "sub_..."
public string StripeCustomerId { get; set; }      // "cus_..."
public DateTime? CancelledAt { get; set; }        // UTC
public string? CancellationReason { get; set; }
```

### PaymentTransaction Entity
```csharp
public decimal TaxAmount { get; set; }            // decimal(18,2)
public string CountryCode { get; set; }           // "CA", "US"
public string? RegionCode { get; set; }           // "ON", "BC"
public string? StripeSubscriptionId { get; set; } // "sub_..."
public string? StripeInvoiceId { get; set; }      // "in_..."
public PaymentStatus Status { get; set; }         // Enum
public DateTime? ProcessedAt { get; set; }        // UTC
```

---

## 4. Security & Compliance

### PCI Compliance
- ? Stripe handles all credit card data
- ? Never store full card numbers (use `PaymentMethodLast4`)
- ? Webhook signature verification via `EventUtility.ConstructEvent()`

### Tax Compliance
- ? Canadian GST/HST rates by province
- ? `TaxAmount` stored separately for auditing
- ? `CountryCode` + `RegionCode` for accurate tax calculation

### Configuration Security
- ? Secrets stored in `appsettings.json` (dev) or Azure Key Vault (production)
- ? Use `IOptions<StripeSettings>` and `IOptions<AzureStorageSettings>`
- ? Never commit secrets to source control

---

## 5. Testing

### Unit Tests (Recommended)

```csharp
[Fact]
public void CalculateTaxAmount_Ontario_Returns13Percent()
{
    // Arrange
    var service = new StripeService(...);
    
    // Act
    var tax = service.CalculateTaxAmount(100m, "CA", "ON");
    
    // Assert
    Assert.Equal(13m, tax); // 13% HST
}

[Fact]
public async Task UploadAsync_ValidFile_ReturnsUrl()
{
    // Arrange
    var service = new AzureBlobStorageService(...);
    var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
    
    // Act
    var url = await service.UploadAsync(fileStream, "test.jpg");
    
    // Assert
    Assert.Contains("exercise-images", url);
}
```

### Integration Tests

```csharp
[Fact]
public async Task CreateCheckoutSession_ValidRequest_ReturnsSessionId()
{
    // Test Stripe checkout session creation
}

[Fact]
public async Task HandleWebhookAsync_ValidSignature_ActivatesSubscription()
{
    // Test webhook processing
}
```

---

## 6. Deployment Checklist

### Stripe Setup
- [ ] Create Stripe account (test mode)
- [ ] Generate API keys (Secret Key, Publishable Key)
- [ ] Configure webhook endpoint: `https://api.ironlogic.ai/api/v1/financial/webhook`
- [ ] Subscribe to events: `checkout.session.completed`, `invoice.paid`, `customer.subscription.deleted`
- [ ] Copy webhook signing secret
- [ ] Test with Stripe CLI: `stripe listen --forward-to localhost:5000/api/v1/financial/webhook`
- [ ] Switch to live keys for production

### Azure Blob Storage Setup
- [ ] Create Azure Storage Account
- [ ] Create container: `exercise-images`
- [ ] Set public access level: Blob (read-only)
- [ ] Configure CORS for API domain
- [ ] (Optional) Set up Azure CDN endpoint
- [ ] Copy connection string to appsettings

### Configuration (appsettings.Production.json)
```json
{
  "Stripe": {
    "SecretKey": "sk_live_...",
    "PublishableKey": "pk_live_...",
    "WebhookSecret": "whsec_...",
    "SuccessUrl": "https://app.ironlogic.ai/subscription/success",
    "CancelUrl": "https://app.ironlogic.ai/subscription/cancel",
    "UseStripeTax": true
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=ironlogicprod;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "exercise-images",
    "BaseUrl": "https://ironlogicprod.blob.core.windows.net",
    "UseCdn": true,
    "CdnEndpoint": "https://cdn.ironlogic.ai",
    "MaxFileSizeBytes": 5242880
  }
}
```

---

## 7. Monitoring & Logging

### Stripe Events
```csharp
logger.LogInformation(
    "Stripe Checkout Session created: {SessionId} for user {UserId}, plan {PlanId}, currency {Currency}",
    session.Id, userId, planId, currency);
```

### Payment Transactions
```csharp
logger.LogInformation(
    "Payment recorded: {Amount} {Currency} (tax: {TaxAmount}) for user {UserId}",
    amount, currency, taxAmount, userId);
```

### Subscription Changes
```csharp
logger.LogInformation(
    "Subscription activated for user {UserId} via checkout session {SessionId}",
    userId, session.Id);
```

### File Uploads
```csharp
logger.LogInformation(
    "File uploaded: {FileName} to {ContainerName}, size: {Size} bytes",
    fileName, containerName, fileStream.Length);
```

---

## 8. Frontend Integration (Angular)

### Checkout Flow

```typescript
// Create checkout session
createCheckout(planId: string, currency: string): Observable<CheckoutSessionResponse> {
  const request: CreateCheckoutSessionRequest = {
    planId,
    userId: this.authService.currentUserId(),
    userEmail: this.authService.currentUserEmail(),
    currency,
    countryCode: this.user.countryCode || 'US',
    regionCode: this.user.regionCode
  };
  
  return this.http.post<CheckoutSessionResponse>(
    '/api/v1/financial/checkout/create',
    request
  );
}

// Redirect to Stripe Checkout
redirectToCheckout(planId: string): void {
  this.createCheckout(planId, this.user.preferredCurrency).subscribe(response => {
    window.location.href = response.checkoutUrl;
  });
}
```

### Revenue Stats (Financial Dashboard)

```typescript
export class FinancialDashboardComponent {
  revenueStats = signal<RevenueStatsDto | null>(null);
  
  ngOnInit() {
    this.api.getRevenueStats().subscribe(stats => {
      this.revenueStats.set(stats);
    });
  }
}
```

### Exercise Image Upload

```typescript
uploadExerciseImage(file: File): Observable<string> {
  const formData = new FormData();
  formData.append('file', file);
  
  return this.http.post<{ imageUrl: string }>(
    '/api/v1/exercises/upload-image',
    formData
  ).pipe(map(response => response.imageUrl));
}
```

---

## 9. Error Handling

### Stripe Errors

```csharp
try
{
    var session = await service.CreateAsync(options);
}
catch (StripeException ex)
{
    logger.LogError(ex, "Stripe API error: {Message}", ex.Message);
    throw new InvalidOperationException("Payment processing failed", ex);
}
```

### Webhook Signature Verification

```csharp
try
{
    var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _settings.WebhookSecret);
}
catch (StripeException ex)
{
    logger.LogError(ex, "Webhook signature verification failed");
    return false;
}
```

### File Upload Errors

```csharp
if (fileStream.Length > _settings.MaxFileSizeBytes)
    throw new InvalidOperationException($"File size exceeds maximum allowed size of {_settings.MaxFileSizeBytes / 1024 / 1024}MB.");
```

---

## 10. Performance Optimizations

### Caching
- Cache approved subscription plans (rarely change)
- Cache CDN URLs for frequently accessed images

### Database Indexes
```sql
CREATE INDEX IX_PaymentTransactions_ProcessedAt ON PaymentTransactions(ProcessedAt);
CREATE INDEX IX_UserSubscriptions_StripeSubscriptionId ON UserSubscriptions(StripeSubscriptionId);
```

### Async Operations
- All Stripe API calls are async
- All Azure Blob operations are async
- Non-blocking webhook processing

---

## Summary

? **Stripe Integration:** Multi-currency subscriptions with Canadian tax compliance  
? **Azure Blob Storage:** Global CDN for exercise images  
? **Clean Architecture:** Separation of concerns (Domain, Application, Infrastructure)  
? **Security:** PCI compliance, webhook verification, secure configuration  
? **Global Ready:** Supports Canada, USA, Europe, Australia currencies and tax  
? **UTC Timestamps:** Consistent timezone handling  
? **camelCase DTOs:** Seamless Angular Signals integration  

**Production Ready!** ??
