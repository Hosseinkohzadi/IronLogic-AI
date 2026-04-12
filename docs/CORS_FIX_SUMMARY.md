# CORS Preflight Redirect Error - Fix Summary

## Problem
Angular app at `http://localhost:4200` was unable to call the API due to a CORS preflight error:
```
Access to XMLHttpRequest at 'http://localhost:5010/api/v1/auth/login' from origin 'http://localhost:4200' 
has been blocked by CORS policy: Response to preflight request doesn't pass access control check: 
Redirect is not allowed for a preflight request.
```

## Root Causes

### 1. HTTPS Redirection Before CORS Middleware
The middleware pipeline had `UseHttpsRedirection()` **before** `UseCors()`:
```csharp
app.UseHttpsRedirection();  // ❌ This redirects HTTP → HTTPS
app.UseRouting();
app.UseCors("AllowIronLogicDash");  // ❌ Too late - redirect already happened
```

When a browser sends a CORS preflight `OPTIONS` request to `http://localhost:5010`, ASP.NET Core would:
1. Receive the request
2. **Immediately redirect** to `https://localhost:5011` (before CORS headers are added)
3. CORS preflight **fails** because redirects are not allowed during preflight

### 2. Angular Using HTTP Instead of HTTPS
Angular environment was configured to use HTTP:
```typescript
apiUrl: 'https://localhost:5011/api/v1'  // ✅ Should use HTTPS directly
```

## Solution Applied

### Fix 1: Reorder Middleware Pipeline ✅
Moved `UseCors()` **before** `UseHttpsRedirection()`:

```csharp
app.UseRouting();
app.UseCors("AllowIronLogicDash");  // ✅ CORS first
app.UseHttpsRedirection();          // ✅ Redirect after CORS
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
```

**Why this works:**
- CORS headers are now added **before** any redirection occurs
- Preflight requests get proper CORS headers immediately
- No redirect during preflight = no error

### Fix 2: Add HTTPS Origin to CORS Policy ✅
Updated CORS configuration to allow both HTTP and HTTPS origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowIronLogicDash", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:5011")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Fix 3: Angular Environment Already Configured Correctly ✅
The Angular environment file already uses HTTPS:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5011/api/v1'
};
```

## How to Test

### 1. Restart the API
Since the app is currently being debugged with hot reload enabled:
- **Option A:** Use hot reload (if available) - press `Ctrl+R` or click "Hot Reload" in Visual Studio
- **Option B:** Stop debugging and restart the API

### 2. Verify Angular is Using HTTPS
Check `web/iron-logic-dashboard/environments/environment.ts`:
```typescript
apiUrl: 'https://localhost:5011/api/v1'  // ✅ Should be HTTPS
```

### 3. Test Login
Navigate to `http://localhost:4200/auth/login` and try logging in. The CORS error should be resolved.

## Expected Behavior

### Before Fix ❌
```
OPTIONS https://localhost:5011/api/v1/auth/login
Status: 307 (Temporary Redirect)  // ❌ Preflight request redirected
CORS Error: "Redirect is not allowed for a preflight request"
```

### After Fix ✅
```
OPTIONS https://localhost:5011/api/v1/auth/login
Status: 204 No Content  // ✅ Preflight succeeds
Headers:
  Access-Control-Allow-Origin: http://localhost:4200
  Access-Control-Allow-Methods: GET, POST, PUT, DELETE
  Access-Control-Allow-Headers: *
  Access-Control-Allow-Credentials: true

POST https://localhost:5011/api/v1/auth/login
Status: 200 OK  // ✅ Actual request succeeds
```

## Key Takeaways

### ASP.NET Core Middleware Order Matters
The correct order for CORS in ASP.NET Core is:
```csharp
1. app.UseRouting();
2. app.UseCors();           // ← BEFORE UseHttpsRedirection
3. app.UseHttpsRedirection();
4. app.UseAuthentication();
5. app.UseAuthorization();
6. app.MapControllers();
```

### CORS Preflight Rules
- Preflight requests (`OPTIONS`) **cannot follow redirects**
- CORS headers **must be present** on the first response
- Middleware that modifies the response (like `UseHttpsRedirection`) should come **after** `UseCors`

### Development vs Production
In **production**, you should:
- Use HTTPS exclusively (no HTTP)
- Configure allowed origins from `appsettings.Production.json`
- Consider using Azure Front Door or API Management for CORS

## Files Modified

1. ✅ `src/IronLogic.Api/Program.cs` - Middleware order + CORS origins
2. ✅ `web/iron-logic-dashboard/environments/environment.ts` - Already correct

## Related Documentation
- [ASP.NET Core CORS Middleware](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [ASP.NET Core Middleware Order](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
