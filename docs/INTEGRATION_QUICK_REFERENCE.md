# External Integrations - Quick Reference Card

## ?? Configuration (appsettings.json)

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "SuccessUrl": "https://app.ironlogic.ai/subscription/success",
    "CancelUrl": "https://app.ironlogic.ai/subscription/cancel",
    "UseStripeTax": true
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;...",
    "ContainerName": "exercise-images",
    "BaseUrl": "https://ironlogic.blob.core.windows.net",
    "UseCdn": true,
    "CdnEndpoint": "https://ironlogic.azureedge.net"
  }
}
```

---

## ?? Stripe Services

### Create Checkout Session
```csharp
var sessionId = await stripeService.CreateCheckoutSessionAsync(
    planId: Guid.Parse("..."),
    userEmail: "user@example.com",
    userId: "user-123",
    currency: Currency.CAD,
    countryCode: "CA",
    regionCode: "ON"
);
```

### Calculate Canadian Tax
```csharp
var tax = stripeService.CalculateTaxAmount(100m, "CA", "ON");
// Returns: 13.00 (13% HST for Ontario)
```

### Handle Webhook
```csharp
var success = await stripeService.HandleWebhookAsync(json, stripeSignature);
// Processes: checkout.session.completed, invoice.paid, subscription.deleted
```

---

## ?? Azure Blob Storage

### Upload Exercise Image
```csharp
var imageUrl = await fileStorageService.UploadAsync(
    fileStream: imageStream,
    fileName: "exercise-abc123.jpg",
    contentType: "image/jpeg"
);
// Returns: "https://ironlogic.azureedge.net/exercise-images/exercise-abc123.jpg"
```

### Delete Image
```csharp
var deleted = await fileStorageService.DeleteAsync(imageUrl);
```

### Generate Unique File Name
```csharp
var fileName = fileStorageService.GenerateUniqueFileName("squat.jpg");
// Returns: "exercise-{guid}.jpg"
```

---

## ?? Financial Dashboard API

### GET /api/v1/financial/stats
**[Authorize(Roles = "Admin")]**

Response:
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
    { "month": "Feb", "amount": 14700 }
  ]
}
```

---

## ?? Subscription Management

### Activate Subscription
```csharp
var subscription = await subscriptionService.ActivateSubscriptionAsync(
    userId: "user-123",
    planId: planId,
    stripeSubscriptionId: "sub_...",
    stripeCustomerId: "cus_...",
    amount: 149.00m,
    taxAmount: 19.37m,
    currency: Currency.CAD,
    countryCode: "CA",
    regionCode: "ON"
);
```

### Deactivate Subscription
```csharp
var success = await subscriptionService.DeactivateSubscriptionAsync(
    stripeSubscriptionId: "sub_...",
    cancellationReason: "Cancelled by user"
);
```

### Get Active Subscription
```csharp
var subscription = await subscriptionService.GetActiveSubscriptionAsync(userId);
```

---

## ???? Canadian Tax Rates

| Province | Code | Rate | Type |
|----------|------|------|------|
| Ontario | ON | 13% | HST |
| BC | BC | 12% | GST+PST |
| Alberta | AB | 5% | GST |
| Quebec | QC | 14.975% | GST+QST |
| Maritime | NB/NS/PE/NL | 15% | HST |

---

## ?? Multi-Currency Support

```csharp
public enum Currency
{
    USD = 0,  // United States Dollar
    CAD = 1,  // Canadian Dollar
    EUR = 2,  // Euro
    GBP = 3,  // British Pound
    AUD = 4   // Australian Dollar
}
```

---

## ?? Testing (Stripe Test Cards)

```
Success:        4242 4242 4242 4242
Decline:        4000 0000 0000 0002
Insufficient:   4000 0000 0000 9995
```

---

## ?? Webhook Events

| Event | Trigger | Action |
|-------|---------|--------|
| `checkout.session.completed` | Checkout success | Activate subscription |
| `invoice.paid` | Subscription renewal | Record payment |
| `customer.subscription.deleted` | User cancels | Deactivate subscription |
| `customer.subscription.updated` | Status change | Update subscription |

---

## ?? Dependency Injection

```csharp
// Register in DependencyInjection.cs
services.AddScoped<IStripeService, StripeService>();
services.AddScoped<IFileStorageService, AzureBlobStorageService>();
services.AddScoped<ISubscriptionService, SubscriptionService>();

// Configure settings
services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
services.Configure<AzureStorageSettings>(configuration.GetSection("AzureStorage"));
```

---

## ?? Required NuGet Packages

```bash
dotnet add package Stripe.net --version 44.0.0
dotnet add package Azure.Storage.Blobs --version 12.19.1
```

---

## ?? Error Handling

### Stripe Errors
```csharp
try
{
    var session = await stripeService.CreateCheckoutSessionAsync(...);
}
catch (StripeException ex)
{
    logger.LogError(ex, "Stripe API error");
    throw new InvalidOperationException("Payment processing failed", ex);
}
```

### File Upload Errors
```csharp
if (fileStream.Length > _settings.MaxFileSizeBytes)
    throw new InvalidOperationException("File size exceeds 5MB limit");
```

---

## ?? Angular Integration

### Create Checkout
```typescript
createCheckout(planId: string): Observable<CheckoutSessionResponse> {
  return this.http.post<CheckoutSessionResponse>(
    '/api/v1/financial/checkout/create',
    {
      planId,
      userId: this.user.id,
      userEmail: this.user.email,
      currency: this.user.preferredCurrency,
      countryCode: this.user.countryCode,
      regionCode: this.user.regionCode
    }
  );
}
```

### Get Revenue Stats
```typescript
getRevenueStats(baseCurrency: string = 'USD'): Observable<RevenueStatsDto> {
  return this.http.get<RevenueStatsDto>(
    `/api/v1/financial/stats?baseCurrency=${baseCurrency}`
  );
}
```

---

## ?? Documentation Files

- **EXTERNAL_INTEGRATIONS_GUIDE.md** - Full implementation guide
- **INTEGRATION_IMPLEMENTATION_SUMMARY.md** - Executive summary
- **NUGET_PACKAGES_REQUIRED.md** - Package installation
- **appsettings.Integration.example.json** - Configuration template

---

## ? Pre-Deployment Checklist

- [ ] Install Stripe.net NuGet package
- [ ] Install Azure.Storage.Blobs NuGet package
- [ ] Configure Stripe API keys
- [ ] Configure webhook URL
- [ ] Create Azure Storage Account
- [ ] Create `exercise-images` container
- [ ] Test checkout flow
- [ ] Test webhook processing
- [ ] Test file uploads
- [ ] Deploy to staging
- [ ] Switch to live Stripe keys

---

**Quick Start:** See `EXTERNAL_INTEGRATIONS_GUIDE.md` for detailed setup instructions.
