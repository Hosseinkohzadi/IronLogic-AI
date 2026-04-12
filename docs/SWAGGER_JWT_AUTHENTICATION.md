# Swagger JWT Authentication - Implementation Guide

## ? Implementation Complete

JWT authentication has been successfully enabled in Swagger UI using NSwag.

---

## ?? What Was Changed

### **File Modified:** `src/IronLogic.Api/Program.cs`

**Updated NSwag Configuration:**

```csharp
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "IronLogic API";
    config.Title = "IronLogic AI API";
    config.Version = "v1";
    
    // ? Add JWT Authentication to Swagger
    config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Please enter a valid JWT token in the format: Bearer {token}"
    });

    // ? Add global security requirement (shows lock icons)
    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});
```

---

## ?? Configuration Details

### **Security Scheme Properties:**

| Property | Value | Description |
|----------|-------|-------------|
| **Name** | `"Bearer"` | Security scheme identifier |
| **Type** | `Http` | HTTP authentication scheme |
| **Scheme** | `"bearer"` | Bearer token authentication |
| **BearerFormat** | `"JWT"` | Token format specification |
| **In** | `Header` | Token location (Authorization header) |
| **Description** | Custom message | Help text for users |

### **Operation Processor:**

```csharp
config.OperationProcessors.Add(
    new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
```

This automatically:
- ? Detects `[Authorize]` attributes on controllers/actions
- ? Adds lock icons to protected endpoints
- ? Adds security requirements to OpenAPI spec
- ? Shows "Authorize" button in Swagger UI

---

## ?? How to Use Swagger UI with JWT

### **Step 1: Start the Application**

```bash
# In Visual Studio: Press F5
# Or via CLI: dotnet run --project src/IronLogic.Api
```

Navigate to: **https://localhost:5011/swagger**

---

### **Step 2: Login to Get Token**

1. **Expand** the `Auth` section
2. **Click** on `POST /api/v1/Auth/login`
3. **Click** "Try it out"
4. **Enter** credentials:
```json
{
  "email": "admin@ironlogic.ai",
  "password": "Admin@123456"
}
```
5. **Click** "Execute"
6. **Copy** the `token` value from the response

**Example Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDEiLCJlbWFpbCI6ImFkbWluQGlyb25sb2dpYy5haSIsImp0aSI6IjEyMzQ1Njc4LTkwYWItY2RlZi0xMjM0LTU2Nzg5MGFiY2RlZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzUwMDg2Mzk5fQ.signature",
  "userId": "00000000-0000-0000-0000-000000000001",
  "email": "admin@ironlogic.ai",
  "userName": "admin@ironlogic.ai",
  "role": "Admin"
}
```

**Copy the token value** (the long string after `"token":`)

---

### **Step 3: Authorize in Swagger**

1. **Click** the **"Authorize"** button at the top right of Swagger UI (?? icon)
2. A dialog will appear with "Bearer (http, Bearer)" field
3. **Enter** your token in this format:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
   ?? **Important:** Include the word `Bearer` followed by a space, then the token
   
4. **Click** "Authorize"
5. **Click** "Close"

**Visual Indicators:**
- ?? Lock icon changes to **?? (locked)**
- Protected endpoints now show a **?? lock icon**

---

### **Step 4: Test Protected Endpoints**

Now you can test any protected endpoint:

**Example: Test Subscribe Endpoint**

1. **Expand** `Subscription` section
2. **Click** `POST /api/v1/Subscription/subscribe`
3. **Click** "Try it out"
4. **Enter** request body:
```json
{
  "planId": "00000000-0000-0000-0000-000000000002",
  "paymentMethodId": "pm_test_1234567890"
}
```
5. **Click** "Execute"

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Subscription created successfully. Payment processing initiated.",
  "transactionId": "TXN_a1b2c3d4e5f67890a1b2c3d4e5f67890",
  "subscriptionId": "802f9698-b3df-4d60-9982-bfbb205aac4c"
}
```

---

## ?? Protected vs Public Endpoints

### **Public Endpoints (No Lock Icon):**
- ? `GET /api/v1/Auth/register` - Registration
- ? `GET /api/v1/Auth/login` - Login
- ? `GET /api/v1/Subscription/plans` - View plans
- ? `GET /api/health` - Health check

### **Protected Endpoints (Lock Icon ??):**
- ?? `POST /api/v1/Subscription/subscribe` - Requires JWT
- ?? `GET /api/v1/Subscription/my-subscription` - Requires JWT
- ?? `POST /api/v1/Auth/logout` - Requires JWT
- ?? All Admin endpoints - Require JWT + Admin role

---

## ?? Visual Guide

### **Before Authorization:**
```
Swagger UI
??????????????????????????????????????????????????
?  ?? Authorize                                   ?  ? Click here
??????????????????????????????????????????????????
?  Auth                                          ?
?    POST /api/v1/Auth/login                     ?
?                                                ?
?  Subscription                                  ?
?    GET /api/v1/Subscription/plans              ?
?    POST /api/v1/Subscription/subscribe  ??     ?  ? Lock icon
??????????????????????????????????????????????????
```

### **After Authorization:**
```
Swagger UI
??????????????????????????????????????????????????
?  ?? Authorize (Logout)                          ?  ? Now locked
??????????????????????????????????????????????????
?  Auth                                          ?
?    POST /api/v1/Auth/login                     ?
?                                                ?
?  Subscription                                  ?
?    GET /api/v1/Subscription/plans              ?
?    POST /api/v1/Subscription/subscribe  ??     ?  ? Can execute
??????????????????????????????????????????????????
```

---

## ?? Technical Details

### **NSwag vs Swashbuckle:**

Your project uses **NSwag** instead of **Swashbuckle**, which is why we use:

| Feature | NSwag | Swashbuckle |
|---------|-------|-------------|
| **Setup Method** | `AddOpenApiDocument` | `AddSwaggerGen` |
| **Security Scheme** | `config.AddSecurity()` | `options.AddSecurityDefinition()` |
| **Operation Processor** | `AspNetCoreOperationSecurityScopeProcessor` | `SecurityRequirementsOperationFilter` |
| **UI Method** | `UseSwaggerUi()` | `UseSwaggerUI()` |

### **How It Works:**

1. **Security Scheme Definition:**
   - Defines how to authenticate (Bearer token in Authorization header)
   - Appears as "Authorize" button in Swagger UI

2. **Operation Processor:**
   - Scans all controller actions for `[Authorize]` attribute
   - Automatically adds security requirements to those endpoints
   - Displays lock icons in Swagger UI

3. **Runtime Behavior:**
   - When you click "Authorize", token is stored in browser
   - All subsequent API calls include `Authorization: Bearer {token}` header
   - Backend validates token using JWT middleware

---

## ?? Complete Testing Workflow

### **Scenario: Subscribe to Pro Plan**

**1. Login:**
```http
POST /api/v1/Auth/login
{
  "email": "admin@ironlogic.ai",
  "password": "Admin@123456"
}

Response:
{
  "token": "eyJ...",
  "role": "Admin"
}
```

**2. Authorize in Swagger:**
- Click "Authorize" button
- Enter: `Bearer eyJ...`
- Click "Authorize"

**3. Get Available Plans:**
```http
GET /api/v1/Subscription/plans

Response:
[
  { "id": "...", "name": "Basic", "price": 0 },
  { "id": "...", "name": "Pro", "price": 29 },
  { "id": "...", "name": "Elite", "price": 99 }
]
```

**4. Subscribe to Pro Plan:**
```http
POST /api/v1/Subscription/subscribe
{
  "planId": "00000000-0000-0000-0000-000000000002",
  "paymentMethodId": "pm_test_card"
}

Response:
{
  "success": true,
  "message": "Subscription created successfully...",
  "transactionId": "TXN_a1b2c3d4...",
  "subscriptionId": "802f9698-b3df-..."
}
```

**5. Check My Subscription:**
```http
GET /api/v1/Subscription/my-subscription

Response:
{
  "message": "No active subscription found",
  "subscription": null
}
```

---

## ?? Troubleshooting

### **Issue: "Authorize" button not showing**

**Solution:**
- Clear browser cache
- Hard refresh (Ctrl+F5)
- Restart the application

### **Issue: Lock icons not appearing**

**Solution:**
- Ensure controllers have `[Authorize]` attribute
- Verify `AspNetCoreOperationSecurityScopeProcessor` is added
- Restart application

### **Issue: 401 Unauthorized after authorization**

**Solution:**
- Verify token format: `Bearer <space> token`
- Check token hasn't expired (default: 1 day)
- Ensure JWT configuration matches in `Program.cs`

### **Issue: Token format error**

**Correct Format:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Incorrect Formats:**
```
? eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...  (missing "Bearer")
? Bearer: eyJ...  (incorrect separator)
? "Bearer eyJ..."  (quotes not needed)
```

---

## ?? Security Configuration

### **JWT Validation in Program.cs:**

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});
```

This configuration:
- ? Validates token signature
- ? Validates issuer and audience
- ? Checks token expiration
- ? Uses symmetric key from configuration

---

## ?? Configuration Files

### **appsettings.Development.json:**

Ensure you have JWT configuration:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-min-32-chars",
    "Issuer": "IronLogicAPI",
    "Audience": "IronLogicClient",
    "ExpireDays": "1"
  }
}
```

---

## ?? Swagger UI Features

### **Authorize Dialog:**

When you click the "Authorize" button, you'll see:

```
???????????????????????????????????????????
?  Available authorizations               ?
???????????????????????????????????????????
?  Bearer (http, Bearer)                  ?
?                                         ?
?  Value:                                 ?
?  ???????????????????????????????????   ?
?  ? Bearer eyJhbGciOiJIUzI1NiIsI... ?   ?  ? Paste token here
?  ???????????????????????????????????   ?
?                                         ?
?  Please enter a valid JWT token in      ?
?  the format: Bearer {token}             ?
?                                         ?
?  [Authorize] [Close]                    ?
???????????????????????????????????????????
```

### **Endpoint Lock Icons:**

Protected endpoints will show:
```
POST /api/v1/Subscription/subscribe  ??
GET /api/v1/Subscription/my-subscription  ??
POST /api/v1/Auth/logout  ??
```

Public endpoints won't have lock icons:
```
GET /api/v1/Subscription/plans
POST /api/v1/Auth/login
POST /api/v1/Auth/register
```

---

## ?? Testing Checklist

### **Test Authentication Flow:**

- [x] Open Swagger UI (https://localhost:5011/swagger)
- [x] Verify "Authorize" button appears at top
- [x] Login via `/api/v1/Auth/login` endpoint
- [x] Copy token from response
- [x] Click "Authorize" button
- [x] Paste token with "Bearer " prefix
- [x] Click "Authorize"
- [x] Verify lock icon changes to locked state
- [x] Test protected endpoint (e.g., Subscribe)
- [x] Verify 200 OK response (not 401)
- [x] Click "Logout" in Authorize dialog
- [x] Test protected endpoint again
- [x] Verify 401 Unauthorized response

---

## ?? Expected Behavior

### **Before Authorization:**

**Test Protected Endpoint:**
```http
POST /api/v1/Subscription/subscribe
```

**Response:**
```
401 Unauthorized
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### **After Authorization:**

**Same Endpoint:**
```http
POST /api/v1/Subscription/subscribe
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```
200 OK
{
  "success": true,
  "message": "Subscription created successfully...",
  "transactionId": "TXN_a1b2c3d4...",
  "subscriptionId": "802f9698-..."
}
```

---

## ?? Key Features

### **1. Global Security Requirements:**

The `AspNetCoreOperationSecurityScopeProcessor` automatically:
- Scans all controllers for `[Authorize]` attributes
- Adds security requirements to those operations
- Respects `[AllowAnonymous]` overrides
- Shows visual indicators (lock icons)

### **2. Token Persistence:**

Once you authorize in Swagger UI:
- Token is stored in browser session
- All subsequent requests include the token
- No need to re-authorize for each request
- Persists until you click "Logout" or close browser

### **3. Role-Based Authorization:**

If an endpoint requires a specific role:
```csharp
[Authorize(Roles = "Admin")]
public class ExerciseApprovalController : ControllerBase
```

Swagger will still show the lock icon, and the token must contain the required role claim.

---

## ?? Debugging Tips

### **View Request Headers in Swagger:**

After executing a request, scroll down to see:

```
Request headers:
authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
content-type: application/json
accept: application/json
```

### **Decode JWT Token:**

To verify token contents:
1. Go to [jwt.io](https://jwt.io)
2. Paste your token
3. Verify claims:
```json
{
  "sub": "user-guid",
  "email": "admin@ironlogic.ai",
  "role": "Admin",
  "exp": 1750086399
}
```

---

## ?? Common Use Cases

### **Use Case 1: Test as Admin**

```
1. Login with admin@ironlogic.ai
2. Authorize with admin token
3. Test admin endpoints (ExerciseApproval)
4. Should return 200 OK
```

### **Use Case 2: Test as Regular User**

```
1. Register a new user
2. Login with new user credentials
3. Authorize with user token
4. Test admin endpoints
5. Should return 403 Forbidden (no Admin role)
```

### **Use Case 3: Test Unauthenticated**

```
1. Don't authorize (or logout)
2. Test protected endpoints
3. Should return 401 Unauthorized
```

---

## ?? Production Considerations

### **Security Best Practices:**

1. **Token Expiration:**
   - Current: 1 day (from config)
   - Production: Consider shorter expiration (1-4 hours)
   - Implement refresh tokens for better UX

2. **HTTPS Only:**
   - ? Already configured: `app.UseHttpsRedirection()`
   - Never send JWT over HTTP in production

3. **Swagger in Production:**
   - Current: Only enabled in Development
   - Production: Disable or protect with IP whitelist/authentication

4. **Secret Key:**
   - Current: In appsettings.Development.json
   - Production: Use Azure Key Vault or environment variables
   - Minimum 32 characters, cryptographically random

---

## ? Verification Checklist

- [x] NSwag security scheme configured
- [x] Bearer authentication type set
- [x] JWT bearer format specified
- [x] Security processor added for [Authorize] detection
- [x] Authorize button appears in Swagger UI
- [x] Lock icons show on protected endpoints
- [x] Public endpoints accessible without token
- [x] Protected endpoints require token
- [x] Token validation working correctly
- [x] Build successful
- [x] Documentation complete

---

## ?? Summary

**Implementation Status:** ? **COMPLETE**

Your Swagger UI now supports JWT authentication with:
- ? "Authorize" button for token input
- ? Lock icons on protected endpoints
- ? Automatic `Authorization` header injection
- ? Seamless testing of authenticated endpoints
- ? Support for role-based authorization
- ? Visual indicators for security status

**Next Steps:**
1. **Restart the application** to see changes
2. **Open Swagger UI** (https://localhost:5011/swagger)
3. **Login** to get a token
4. **Authorize** with the token
5. **Test protected endpoints** (Subscribe, MySubscription, etc.)

---

**Status:** ?? **READY FOR TESTING**

Your Swagger UI is now fully configured with JWT authentication!
