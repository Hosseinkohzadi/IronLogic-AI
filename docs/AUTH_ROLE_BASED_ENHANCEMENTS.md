# Auth Controller - Role-Based Enhancement

## Overview
Enhanced the `AuthController` to support role-based authentication for frontend routing. The system now automatically assigns roles to users and includes role information in authentication responses.

## Changes Made

### 1. **Updated DTOs**

#### `RegisterDto.cs`
- Added optional `FullName` parameter for future use
- Maintains backward compatibility

#### `AuthResponseDto.cs`
- Added `Role` property to response
- Now returns: `{ token, userId, email, userName, role }`

### 2. **AuthController Enhancements**

#### **Primary Constructor**
```csharp
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager,  // ? Added
    IConfiguration configuration,
    ILogger<AuthController> logger)
```

#### **Register Endpoint** (`POST /api/v1/Auth/register`)
**Changes:**
- Automatically assigns `"User"` role to new registrations
- Calls `EnsureRolesExistAsync()` to create roles if missing
- Returns `AuthResponseDto` with role: `"User"`
- Generates JWT token with role claims

**Response Structure:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "802f9698-b3df-4d60-9982-bfbb205aac4c",
  "email": "admin@ironlogic.ai",
  "userName": "admin@ironlogic.ai",
  "role": "User"
}
```

#### **Login Endpoint** (`POST /api/v1/Auth/login`)
**Changes:**
- Fetches user's role using `UserManager.GetRolesAsync(user)`
- Includes role in response
- Defaults to `"User"` if no role assigned

**Response Structure:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "802f9698-b3df-4d60-9982-bfbb205aac4c",
  "email": "admin@ironlogic.ai",
  "userName": "admin@ironlogic.ai",
  "role": "Admin"
}
```

### 3. **JWT Token Enhancement**

#### **GenerateJwtTokenAsync()**
- Now async to fetch user roles
- Adds role claims to JWT token using `ClaimTypes.Role`
- Supports multiple roles (loops through all assigned roles)

**JWT Claims Structure:**
```csharp
new Claim(JwtRegisteredClaimNames.Sub, user.Id),
new Claim(JwtRegisteredClaimNames.Email, user.Email),
new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid()),
new Claim(ClaimTypes.Role, "Admin"),  // ? Role claim added
new Claim(ClaimTypes.Role, "User")    // ? Supports multiple roles
```

### 4. **Role Management**

#### **EnsureRolesExistAsync()**
- Automatically creates `"Admin"` and `"User"` roles if they don't exist
- Called during user registration
- Logs role creation for audit trail

**Roles:**
- `Admin` - For super admin/coaching staff
- `User` - Default role for athletes

## Frontend Integration

### Angular AuthService Compatibility
The response structure is compatible with the Angular `AuthService` which expects:
- `role` or `Role` property in response
- Maps `Admin` ? `SUPER_ADMIN`
- Maps `User` ? `ATHLETE`

### Role-Based Routing
```typescript
// Frontend automatically redirects based on role:
if (role === 'SUPER_ADMIN') {
  return '/admin/dashboard';
}
if (role === 'ATHLETE') {
  return '/athlete/dashboard';
}
```

## Database Schema
No schema changes required. Uses existing ASP.NET Identity tables:
- `AspNetRoles` - Stores role definitions
- `AspNetUserRoles` - Links users to roles

## Testing

### 1. Register a New User
```bash
POST /api/v1/Auth/register
{
  "email": "athlete@ironlogic.ai",
  "password": "Athlete@123456"
}

# Response includes role: "User"
```

### 2. Login as Admin
```bash
POST /api/v1/Auth/login
{
  "email": "admin@ironlogic.ai",
  "password": "Admin@123456"
}

# Response includes role: "Admin" or "User"
```

### 3. Verify JWT Token
Decode the JWT token to see role claims:
```json
{
  "sub": "user-id-guid",
  "email": "admin@ironlogic.ai",
  "role": "Admin",
  "jti": "unique-token-id",
  "exp": 1234567890
}
```

## Security Considerations

1. **Role Assignment:**
   - New users get `"User"` role by default
   - Admin role must be manually assigned via database or admin panel

2. **JWT Claims:**
   - Roles are embedded in JWT token
   - Token must be validated on protected endpoints using `[Authorize(Roles = "Admin")]`

3. **Role Persistence:**
   - Roles are stored in database
   - Cached in JWT token for performance

## Next Steps

### Optional Enhancements:
1. **Email Confirmation Flow** - Currently auto-confirmed
2. **Role Management API** - Admin endpoint to assign/revoke roles
3. **Refresh Tokens** - For better security and UX
4. **Multi-Role Support** - Users can have multiple roles simultaneously
5. **Custom Claims** - Add subscription tier, permissions, etc.

## Standards Compliance

? **C# 13 Features:**
- Primary constructors
- Collection expressions: `string[] roles = ["Admin", "User"];`

? **Project Standards:**
- XML documentation on all public methods
- PascalCase for C# properties (converted to camelCase by JSON serializer)
- UTC timestamps (`DateTime.UtcNow`)
- Detailed logging with structured logging

? **Clean Architecture:**
- DTOs in Application layer
- Controller handles HTTP concerns only
- Business logic delegated to Identity services

## Breaking Changes

?? **AuthResponseDto Structure Changed:**
- Old: `{ token, userId, email, userName }`
- New: `{ token, userId, email, userName, role }`

Frontend must be updated to handle the new `role` property (already compatible with current Angular implementation).
