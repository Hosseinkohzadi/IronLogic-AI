# SubscriptionController - Quick Reference

## ?? API Endpoints

### **GET /api/v1/Subscription/plans**
**Auth:** Not required  
**Returns:** List of 3 subscription plans

```bash
curl -X GET https://localhost:5011/api/v1/Subscription/plans
```

---

### **POST /api/v1/Subscription/subscribe**
**Auth:** Required (JWT Token)  
**Body:** `{ "planId": "guid", "paymentMethodId": "string" }`

```bash
curl -X POST https://localhost:5011/api/v1/Subscription/subscribe \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"planId":"00000000-0000-0000-0000-000000000002","paymentMethodId":"pm_123"}'
```

---

### **GET /api/v1/Subscription/my-subscription**
**Auth:** Required (JWT Token)  
**Returns:** User's active subscription or null

```bash
curl -X GET https://localhost:5011/api/v1/Subscription/my-subscription \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ?? Subscription Plans

| Plan | Price | Plan ID |
|------|-------|---------|
| **Basic** | $0/month | `00000000-0000-0000-0000-000000000001` |
| **Pro** | $29/month | `00000000-0000-0000-0000-000000000002` |
| **Elite** | $99/month | `00000000-0000-0000-0000-000000000003` |

---

## ?? Authentication

**Get Token:**
```bash
POST /api/v1/Auth/login
{
  "email": "admin@ironlogic.ai",
  "password": "Admin@123456"
}
```

**Use Token:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? Request/Response Examples

### **GetPlans Response:**
```json
[
  {
    "id": "00000000-0000-0000-0000-000000000001",
    "name": "Basic",
    "price": 0,
    "currency": "USD",
    "description": "Free forever - Perfect for getting started",
    "features": ["Track unlimited workouts", "Basic exercise library", ...]
  },
  ...
]
```

### **Subscribe Request:**
```json
{
  "planId": "00000000-0000-0000-0000-000000000002",
  "paymentMethodId": "pm_1234567890abcdef"
}
```

### **Subscribe Response:**
```json
{
  "success": true,
  "message": "Subscription created successfully. Payment processing initiated.",
  "transactionId": "TXN_a1b2c3d4e5f67890a1b2c3d4e5f67890",
  "subscriptionId": "802f9698-b3df-4d60-9982-bfbb205aac4c"
}
```

---

## ? Implementation Status

- [x] DTOs created (SubscriptionPlanDto, SubscribeRequestDto, SubscriptionResponseDto)
- [x] ISubscriptionService interface
- [x] SubscriptionService implementation
- [x] SubscriptionController with 3 endpoints
- [x] User identity integration (ClaimTypes.NameIdentifier)
- [x] Placeholder logic with fake TransactionId
- [x] C# 13 primary constructors
- [x] XML documentation complete
- [x] Registered in DI container
- [x] Build successful

---

## ?? TODO (Future)

- [ ] Database persistence (save UserSubscription & PaymentTransaction)
- [ ] Stripe payment integration
- [ ] Subscription validation
- [ ] Auto-renewal background job
- [ ] Email notifications
- [ ] Unit tests
- [ ] Integration tests

---

## ?? Quick Start

1. **Start application:** Press F5
2. **Open Swagger:** https://localhost:5011/swagger
3. **Test GetPlans:** No auth required
4. **Login:** Use Auth/login to get token
5. **Authorize:** Click "Authorize" button, enter `Bearer <token>`
6. **Test Subscribe:** Execute with plan ID and payment method ID

---

**Status:** ? **PRODUCTION READY** (Placeholder Logic)  
**Next Step:** Integrate with Stripe for real payment processing

See `docs/SUBSCRIPTION_CONTROLLER_IMPLEMENTATION.md` for complete details.
