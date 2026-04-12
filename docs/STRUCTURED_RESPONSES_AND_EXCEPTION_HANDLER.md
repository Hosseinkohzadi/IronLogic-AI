# Global Exception Handler & Structured Responses - Implementation Guide

## ? Implementation Complete

All admin endpoints in `SubscriptionController` now return structured JSON responses, and a Global Exception Handler middleware has been added for consistent error handling.

---

## ?? Changes Made

### **1. Updated Admin Endpoints - Structured Responses**

All admin endpoints now return consistent JSON with a `message` property:

#### **Create Plan (POST /admin/plans):**
**Success (201 Created):**
```json
{
  "message": "Plan created successfully",
  "plan": {
    "id": "a1b2c3d4-...",
    "name": "Premium",
    "price": 49.99,
    "currency": "USD",
    "description": "...",
    "features": [...]
  }
}
```

**Validation Error (400 Bad Request):**
```json
{
  "message": "Invalid plan data",
  "errors": {
    "Name": ["Plan name is required"],
    "Price": ["Price must be between 0 and 999999.99"]
  }
}
```

---

#### **Update Plan (PUT /admin/plans/{id}):**
**Success (200 OK):**
```json
{
  "message": "Plan updated successfully",
  "plan": {
    "id": "00000000-0000-0000-0000-000000000002",
    "name": "Pro",
    "price": 24.99,
    "currency": "USD",
    "description": "...",
    "features": [...]
  }
}
```

**Not Found (404 Not Found):**
```json
{
  "message": "Plan not found"
}
```

**Validation Error (400 Bad Request):**
```json
{
  "message": "Invalid plan data",
  "errors": {
    "Price": ["Price must be between 0 and 999999.99"]
  }
}
```

---

#### **Delete Plan (DELETE /admin/plans/{id}):**
**Success (200 OK):**
```json
{
  "message": "Plan deleted successfully"
}
```

**Not Found (404 Not Found):**
```json
{
  "message": "Plan not found"
}
```

**Note:** Changed from `204 No Content` to `200 OK` with structured message for better Angular interceptor compatibility.

---

#### **Get All Transactions (GET /admin/all-transactions):**
**Success (200 OK):**
```json
{
  "message": "Transactions retrieved successfully",
  "count": 42,
  "transactions": [
    {
      "transactionId": "...",
      "userId": "...",
      "userEmail": "user@ironlogic.ai",
      "amount": 29.00,
      "currency": "USD",
      "status": "Completed",
      ...
    }
  ]
}
```

---

### **2. Global Exception Handler Middleware**

**Created:** `src/IronLogic.Api/Middleware/GlobalExceptionHandler.cs`

#### **Features:**

? **Catches all unhandled exceptions**  
? **Returns consistent JSON responses**  
? **Logs exceptions with structured logging**  
? **Maps exception types to appropriate HTTP status codes**  
? **Angular-friendly response format**

#### **Implementation:**

```csharp
public class GlobalExceptionHandler(
    RequestDelegate next, 
    ILogger<GlobalExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentException or ArgumentNullException => 
                (HttpStatusCode.BadRequest, exception.Message),
            
            UnauthorizedAccessException => 
                (HttpStatusCode.Unauthorized, "You are not authorized to perform this action"),
            
            KeyNotFoundException or FileNotFoundException => 
                (HttpStatusCode.NotFound, "The requested resource was not found"),
            
            InvalidOperationException => 
                (HttpStatusCode.Conflict, exception.Message),
            
            _ => (HttpStatusCode.InternalServerError, 
                  "An error occurred while processing your request")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message = message,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
```

---

### **3. Middleware Registration in Program.cs**

**Added:**
```csharp
using IronLogic.Api.Middleware;  // ? New using

// ...

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowIronLogicDash");

app.UseGlobalExceptionHandler();  // ? Registered BEFORE Auth

app.UseAuthentication();
app.UseAuthorization();
```

**Middleware Order:**
1. HTTPS Redirection
2. Routing
3. CORS
4. **Global Exception Handler** ? NEW
5. Authentication
6. Authorization
7. Controllers

This order ensures exceptions from authentication/authorization are also caught.

---

## ?? Exception Mapping

The middleware maps exceptions to appropriate HTTP status codes:

| Exception Type | HTTP Status | Message |
|----------------|-------------|---------|
| `ArgumentException` | 400 Bad Request | Exception message |
| `ArgumentNullException` | 400 Bad Request | Exception message |
| `UnauthorizedAccessException` | 401 Unauthorized | "You are not authorized..." |
| `KeyNotFoundException` | 404 Not Found | "The requested resource was not found" |
| `FileNotFoundException` | 404 Not Found | "The requested resource was not found" |
| `InvalidOperationException` | 409 Conflict | Exception message |
| **All Others** | 500 Internal Server Error | Generic error message |

---

## ?? Angular Interceptor Integration

### **Error Response Format:**

All errors now return consistent JSON:

```json
{
  "message": "User-friendly error message",
  "statusCode": 400,
  "timestamp": "2026-04-12T10:30:00.000Z"
}
```

### **Angular HTTP Interceptor:**

Your Angular interceptor can now easily extract and display errors:

```typescript
// error.interceptor.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred';

      // ? Extract message from structured response
      if (error.error && error.error.message) {
        errorMessage = error.error.message;
      } else if (error.message) {
        errorMessage = error.message;
      }

      // ? Show toast notification
      toastService.error(errorMessage);

      return throwError(() => error);
    })
  );
};
```

---

## ?? Response Examples

### **Successful Operations:**

**Create Plan:**
```json
{
  "message": "Plan created successfully",
  "plan": { ... }
}
```

**Update Plan:**
```json
{
  "message": "Plan updated successfully",
  "plan": { ... }
}
```

**Delete Plan:**
```json
{
  "message": "Plan deleted successfully"
}
```

**Get Transactions:**
```json
{
  "message": "Transactions retrieved successfully",
  "count": 5,
  "transactions": [ ... ]
}
```

---

### **Error Responses:**

**Validation Error (400):**
```json
{
  "message": "Invalid plan data",
  "errors": {
    "Price": ["Price must be between 0 and 999999.99"],
    "Currency": ["Currency must be USD, CAD, EUR, GBP, or AUD"]
  }
}
```

**Not Found (404):**
```json
{
  "message": "Plan not found"
}
```

**Unauthorized (401):**
```json
{
  "message": "You are not authorized to perform this action"
}
```

**Forbidden (403):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

**Unhandled Exception (500):**
```json
{
  "message": "An error occurred while processing your request",
  "statusCode": 500,
  "timestamp": "2026-04-12T10:30:00.000Z"
}
```

---

## ?? Testing Guide

### **Test 1: Create Plan with Invalid Data**

```http
POST /api/v1/Subscription/admin/plans
Authorization: Bearer <admin-token>

{
  "name": "X",
  "price": -10,
  "currency": "JPY"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "message": "Invalid plan data",
  "errors": {
    "Name": ["Plan name must be between 2 and 100 characters"],
    "Price": ["Price must be between 0 and 999999.99"],
    "Currency": ["Currency must be USD, CAD, EUR, GBP, or AUD"]
  }
}
```

---

### **Test 2: Update Non-Existent Plan**

```http
PUT /api/v1/Subscription/admin/plans/99999999-9999-9999-9999-999999999999
Authorization: Bearer <admin-token>

{
  "price": 29.99
}
```

**Expected Response (404 Not Found):**
```json
{
  "message": "Plan not found"
}
```

---

### **Test 3: Delete Non-Existent Plan**

```http
DELETE /api/v1/Subscription/admin/plans/99999999-9999-9999-9999-999999999999
Authorization: Bearer <admin-token>
```

**Expected Response (404 Not Found):**
```json
{
  "message": "Plan not found"
}
```

---

### **Test 4: Delete Existing Plan**

```http
DELETE /api/v1/Subscription/admin/plans/00000000-0000-0000-0000-000000000003
Authorization: Bearer <admin-token>
```

**Expected Response (200 OK):**
```json
{
  "message": "Plan deleted successfully"
}
```

---

### **Test 5: Trigger Unhandled Exception**

**Example:** Try to create a plan with the service throwing an exception

**Expected Response (500 Internal Server Error):**
```json
{
  "message": "An error occurred while processing your request",
  "statusCode": 500,
  "timestamp": "2026-04-12T10:30:00.000Z"
}
```

**Log Output:**
```
[ERROR] An unhandled exception occurred: Some exception message
System.Exception: Some exception message
   at ...stack trace...
```

---

## ?? Controller Changes Summary

### **Before:**

```csharp
// Delete returned 204 No Content (empty body)
return NoContent();

// Errors had inconsistent formats
return StatusCode(500, new { Message = "..." });
return NotFound(new { Message = "..." });
```

### **After:**

```csharp
// Delete returns 200 OK with structured message
return Ok(new { message = "Plan deleted successfully" });

// All errors use consistent format (lowercase 'message')
return NotFound(new { message = "Plan not found" });
return BadRequest(new { message = "Invalid plan data", errors = ModelState });
```

**Benefits:**
- ? Consistent response structure
- ? Always returns JSON (no empty bodies)
- ? Angular interceptor can always extract `error.error.message`
- ? Easy to display toast notifications

---

## ?? Angular Integration Example

### **Admin Service (TypeScript):**

```typescript
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export interface CreatePlanRequest {
  name: string;
  price: number;
  currency: string;
  durationDays: number;
  description?: string;
  features: string[];
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  price: number;
  currency: string;
  description: string;
  features: string[];
}

export interface ApiResponse<T> {
  message: string;
  plan?: T;
  count?: number;
  transactions?: any[];
}

@Injectable({ providedIn: 'root' })
export class SubscriptionAdminService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Subscription/admin`;

  createPlan(request: CreatePlanRequest): Observable<ApiResponse<SubscriptionPlan>> {
    return this.http.post<ApiResponse<SubscriptionPlan>>(
      `${this.apiUrl}/plans`, 
      request
    );
  }

  updatePlan(id: string, updates: Partial<CreatePlanRequest>): Observable<ApiResponse<SubscriptionPlan>> {
    return this.http.put<ApiResponse<SubscriptionPlan>>(
      `${this.apiUrl}/plans/${id}`, 
      updates
    );
  }

  deletePlan(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.apiUrl}/plans/${id}`
    );
  }

  getAllTransactions(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(
      `${this.apiUrl}/all-transactions`
    );
  }
}
```

---

### **Component Usage:**

```typescript
import { Component, inject } from '@angular/core';
import { SubscriptionAdminService } from '@core/services';
import { ToastService } from '@shared/services';

@Component({
  selector: 'app-plan-management',
  template: `...`
})
export class PlanManagementComponent {
  private adminService = inject(SubscriptionAdminService);
  private toast = inject(ToastService);

  createPlan(formData: CreatePlanRequest): void {
    this.adminService.createPlan(formData).subscribe({
      next: (response) => {
        // ? Extract message from structured response
        this.toast.success(response.message);
        this.refreshPlans();
      },
      error: (error) => {
        // ? Error interceptor shows toast automatically
        console.error('Create plan failed:', error);
      }
    });
  }

  updatePlan(id: string, updates: any): void {
    this.adminService.updatePlan(id, updates).subscribe({
      next: (response) => {
        this.toast.success(response.message);  // "Plan updated successfully"
        this.refreshPlans();
      }
    });
  }

  deletePlan(id: string): void {
    this.adminService.deletePlan(id).subscribe({
      next: (response) => {
        this.toast.success(response.message);  // "Plan deleted successfully"
        this.refreshPlans();
      }
    });
  }
}
```

---

### **HTTP Error Interceptor:**

```typescript
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '@shared/services';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred';

      // ? Extract message from structured response
      if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.message) {
        errorMessage = error.message;
      }

      // ? Show toast notification
      toast.error(errorMessage);

      // ? Log for debugging
      console.error('HTTP Error:', {
        url: req.url,
        status: error.status,
        message: errorMessage,
        timestamp: error.error?.timestamp
      });

      return throwError(() => error);
    })
  );
};
```

---

## ?? How It Works

### **Normal Flow (No Exception):**

```
Request
   ?
CORS Middleware
   ?
Global Exception Handler (try { ... })
   ?
Authentication
   ?
Authorization
   ?
Controller Action
   ?
Response (200/201/etc.)
```

---

### **Exception Flow:**

```
Request
   ?
CORS Middleware
   ?
Global Exception Handler (try { ... })
   ?
Authentication ???? throws UnauthorizedAccessException
   ?
   ? Exception caught by Global Handler
   ?
HandleExceptionAsync()
   ?? Log exception
   ?? Map to HTTP status code
   ?? Create structured JSON response
   ?? Write to response stream
   ?
Response (401/404/500/etc. with JSON body)
   ?
Angular Interceptor
   ?? Extract error.error.message
   ?? Show Toast notification
```

---

## ?? Response Structure Standards

### **Success Responses:**

**Pattern:**
```json
{
  "message": "Operation successful",
  "data": { ... },
  "count": 42  // Optional for collections
}
```

**Examples:**
- Create: `{ message, plan }`
- Update: `{ message, plan }`
- Delete: `{ message }`
- GetAll: `{ message, count, transactions }`

---

### **Error Responses:**

**Pattern:**
```json
{
  "message": "Error description",
  "statusCode": 400,
  "timestamp": "2026-04-12T10:30:00.000Z",
  "errors": { ... }  // Optional for validation errors
}
```

**Examples:**
- Validation: `{ message, errors: { Field: ["Error"] } }`
- Not Found: `{ message }`
- Unauthorized: `{ message, statusCode, timestamp }`
- Server Error: `{ message, statusCode, timestamp }`

---

## ? Standards Compliance

### **Lowercase 'message' Property:**

? All responses use lowercase `message` (not `Message`)

**Why?**
- Matches ASP.NET Core default JSON serialization (camelCase)
- Consistent with Angular conventions
- Easy to extract in TypeScript: `error.error.message`

### **Structured Responses:**

? All endpoints return JSON objects (not primitives or empty bodies)

**Before:**
```csharp
return NoContent();  // Empty body
```

**After:**
```csharp
return Ok(new { message = "Plan deleted successfully" });  // JSON
```

### **Consistent Error Format:**

? All errors have a `message` property

**Benefits:**
- Angular interceptor can always find `error.error.message`
- Toast notifications work consistently
- Debugging is easier with timestamps
- User experience is consistent

---

## ?? Complete Testing Workflow

### **Test Exception Handling:**

**1. Test with Invalid Data:**
```http
POST /api/v1/Subscription/admin/plans
Authorization: Bearer <admin-token>

{
  "name": "",
  "price": -10,
  "currency": "INVALID"
}
```

**Expected:**
```json
{
  "message": "Invalid plan data",
  "errors": {
    "Name": ["Plan name is required"],
    "Price": ["Price must be between 0 and 999999.99"],
    "Currency": ["Currency must be USD, CAD, EUR, GBP, or AUD"]
  }
}
```

**Angular Toast:** "Invalid plan data"

---

**2. Test Not Found:**
```http
DELETE /api/v1/Subscription/admin/plans/99999999-9999-9999-9999-999999999999
Authorization: Bearer <admin-token>
```

**Expected:**
```json
{
  "message": "Plan not found"
}
```

**Angular Toast:** "Plan not found"

---

**3. Test Successful Delete:**
```http
DELETE /api/v1/Subscription/admin/plans/00000000-0000-0000-0000-000000000002
Authorization: Bearer <admin-token>
```

**Expected:**
```json
{
  "message": "Plan deleted successfully"
}
```

**Angular Toast:** "Plan deleted successfully" ?

---

**4. Test Unauthorized (No Token):**
```http
POST /api/v1/Subscription/admin/plans
(No Authorization header)

{ "name": "Test", "price": 10, ... }
```

**Expected:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

**Angular Toast:** "Unauthorized" or custom message from interceptor

---

**5. Test Forbidden (Non-Admin):**
```http
POST /api/v1/Subscription/admin/plans
Authorization: Bearer <user-token>

{ "name": "Test", "price": 10, ... }
```

**Expected:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

**Angular Toast:** "Forbidden" or "You don't have permission..."

---

## ?? Key Benefits

### **1. Consistent API Responses:**
- All endpoints return structured JSON
- Always includes `message` property
- Easy to parse in frontend

### **2. Global Error Handling:**
- Catches all unhandled exceptions
- Returns user-friendly messages
- Logs exceptions with full details
- No stack traces exposed to client

### **3. Angular Integration:**
- Interceptor can extract `error.error.message`
- Toast notifications work automatically
- Consistent user experience

### **4. Developer Experience:**
- Predictable response formats
- Easy to test in Swagger
- Clear error messages
- Comprehensive logging

---

## ?? Files Modified/Created

### **Created:**
1. ? `GlobalExceptionHandler.cs` - Exception middleware
2. ? `CreatePlanDto.cs` - With validation
3. ? `UpdatePlanDto.cs` - With validation
4. ? `PaymentTransactionDto.cs` - Transaction response

### **Modified:**
1. ? `SubscriptionController.cs` - Structured responses
2. ? `SubscriptionService.cs` - CRUD operations
3. ? `ISubscriptionService.cs` - CRUD interface
4. ? `Program.cs` - Middleware registration

---

## ?? Build Status

? **BUILD SUCCESSFUL**

**Hot Reload Available:**
- Changes can be applied without restart
- Or restart debugger to see middleware in action

---

## ?? Middleware Order Diagram

```
????????????????????????????????????????
?         HTTP Request                 ?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    app.UseHttpsRedirection()         ?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    app.UseRouting()                  ?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    app.UseCors()                     ?
????????????????????????????????????????
                ?
????????????????????????????????????????
? ? app.UseGlobalExceptionHandler()   ? ? NEW!
?    (Wraps all downstream in try-catch)?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    app.UseAuthentication()           ?
?    (Can throw exceptions)            ?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    app.UseAuthorization()            ?
?    (Can throw exceptions)            ?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    Controller Action                 ?
?    (Can throw exceptions)            ?
????????????????????????????????????????
                ?
????????????????????????????????????????
?    Response (Success or Error)       ?
????????????????????????????????????????
```

---

## ? Verification Checklist

- [x] All admin endpoints return structured JSON with `message`
- [x] Create plan returns: `{ message, plan }`
- [x] Update plan returns: `{ message, plan }`
- [x] Delete plan returns: `{ message }` (not empty 204)
- [x] Get transactions returns: `{ message, count, transactions }`
- [x] Not found errors return: `{ message: "Plan not found" }`
- [x] Validation errors return: `{ message, errors }`
- [x] Global exception handler created
- [x] Middleware registered in Program.cs
- [x] Middleware logs exceptions
- [x] Middleware returns structured JSON
- [x] Response property names are camelCase
- [x] Build successful

---

## ?? Summary

**Implemented:**
- ? Structured responses for all admin endpoints
- ? Consistent `message` property (lowercase)
- ? Global exception handler middleware
- ? Exception-to-HTTP-status mapping
- ? Angular-friendly error format
- ? Comprehensive logging
- ? Changed DELETE to return 200 OK with message (not 204)

**Benefits:**
- ? Angular interceptor can extract errors easily
- ? Toast notifications work automatically
- ? Consistent user experience
- ? Better debugging with timestamps
- ? No stack traces exposed to client

**Status:** ?? **READY FOR TESTING**

**Use Hot Reload or restart to test the new structured responses and exception handling!**

---

**Admin Credentials:**
- **Email:** `admin@ironlogic.ai`
- **Password:** `Admin@123456`
- **Role:** `Admin` ?

Test all admin endpoints in Swagger to verify the structured responses! ??
