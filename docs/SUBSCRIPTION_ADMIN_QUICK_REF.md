# Subscription Admin CRUD - Quick Reference Card

## ?? Admin Endpoints

### **Create Plan**
```http
POST /api/v1/Subscription/admin/plans
Authorization: Bearer <admin-token>

{
  "name": "Premium",
  "price": 49.99,
  "currency": "USD",
  "durationDays": 30,
  "description": "Premium tier",
  "features": ["Feature 1", "Feature 2"]
}

? 201 Created
```

### **Update Plan**
```http
PUT /api/v1/Subscription/admin/plans/{id}
Authorization: Bearer <admin-token>

{
  "price": 44.99,
  "features": ["Updated feature list"]
}

? 200 OK
```

### **Delete Plan (Soft)**
```http
DELETE /api/v1/Subscription/admin/plans/{id}
Authorization: Bearer <admin-token>

? 204 No Content
```

### **Get All Transactions**
```http
GET /api/v1/Subscription/admin/all-transactions
Authorization: Bearer <admin-token>

? 200 OK with transaction list
```

---

## ? Validation Rules

| Field | Required | Rules |
|-------|----------|-------|
| **Name** | Yes | 2-100 chars |
| **Price** | Yes | 0-999999.99 ? |
| **Currency** | Yes | USD/CAD/EUR/GBP/AUD |
| **DurationDays** | Yes | 1-365 |
| **Description** | No | Max 500 chars |
| **Features** | No | Array of strings |

---

## ??? Soft Delete

**Why?**
- Preserves existing subscriptions
- Maintains data integrity
- Keeps historical data

**How?**
- Sets `IsActive = false`
- Plan hidden from public API
- Plan still in database

---

## ?? Authorization

**Admin Role Required:**
- POST /admin/plans
- PUT /admin/plans/{id}
- DELETE /admin/plans/{id}
- GET /admin/all-transactions

**How to Test:**
1. Login: `admin@ironlogic.ai` / `Admin@123456`
2. Copy token
3. Authorize in Swagger: `Bearer <token>`
4. Test endpoints

---

## ?? Plan IDs

| Plan | ID |
|------|-----|
| Basic | `00000000-0000-0000-0000-000000000001` |
| Pro | `00000000-0000-0000-0000-000000000002` |
| Elite | `00000000-0000-0000-0000-000000000003` |

---

## ?? Quick Test

```bash
# 1. Login
POST /api/v1/Auth/login
{ "email": "admin@ironlogic.ai", "password": "Admin@123456" }

# 2. Copy token

# 3. Create plan
POST /api/v1/Subscription/admin/plans
Bearer <token>
{ "name": "Test", "price": 9.99, "currency": "USD", "durationDays": 30, "features": [] }

# 4. Update plan
PUT /api/v1/Subscription/admin/plans/<new-plan-id>
{ "price": 19.99 }

# 5. Delete plan
DELETE /api/v1/Subscription/admin/plans/<new-plan-id>
```

---

## ? Status

**Build:** ? Successful  
**Hot Reload:** Available  
**Documentation:** Complete

**Files Created:**
- CreatePlanDto.cs
- UpdatePlanDto.cs
- PaymentTransactionDto.cs
- Updated ISubscriptionService.cs
- Updated SubscriptionService.cs
- Updated SubscriptionController.cs

**Ready for testing!** ??
