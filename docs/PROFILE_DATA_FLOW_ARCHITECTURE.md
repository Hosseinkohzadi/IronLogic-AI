# Profile Data Flow Architecture

## Overview
This document illustrates the data flow for user profile information through the IronLogic system.

## Entity Relationship

```
┌─────────────────────────────────────────────────────────────┐
│                      ASP.NET Core Identity                   │
│                         (User Entity)                        │
├─────────────────────────────────────────────────────────────┤
│ Id: string (PK)                                             │
│ UserName: string              ← Derived from FirstName +    │
│ Email: string                    LastName                   │
│ PhoneNumber: string           ← NEW: Mapped to DTO          │
│ ... (other Identity fields)                                 │
└─────────────────────────────┬───────────────────────────────┘
                              │
                              │ 1:1 Relationship
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       UserProfile Entity                     │
│                    (Extended Profile Data)                   │
├─────────────────────────────────────────────────────────────┤
│ Id: Guid (PK)                                               │
│ UserId: string (FK)                                         │
│                                                             │
│ FirstName: string?            ← NEW                         │
│ LastName: string?             ← NEW                         │
│ ProfilePictureUrl: string?    ← NEW                         │
│                                                             │
│ Bio: string?                                                │
│ DateOfBirth: DateTime?                                      │
│ Gender: Gender (enum)                                       │
│ Height: decimal?                                            │
│ CurrentWeight: decimal?                                     │
│ TargetWeight: decimal?                                      │
│ ActivityLevel: ActivityLevel (enum)                         │
│                                                             │
│ DateCreated: DateTimeOffset                                 │
│ DateModified: DateTimeOffset                                │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow: GET /api/v1/Account/me

```
┌──────────────┐
│   Frontend   │
│   (Angular)  │
└──────┬───────┘
       │ HTTP GET /api/v1/Account/me
       │ Authorization: Bearer {token}
       ▼
┌──────────────────────────────────────────────────────────────┐
│                      AccountController                        │
│  [HttpGet("me")]                                             │
│  public async Task<IActionResult> GetProfile()               │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ Call: profileService.GetProfileAsync(userId, ct)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                       ProfileService                          │
│  public async Task<Result<UserProfileResponseDto>>           │
│  GetProfileAsync(string userId, CancellationToken ct)         │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 1. Query UserManager
       │    .Include(u => u.Profile)
       │    .FirstOrDefaultAsync(u => u.Id == userId)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                  Database (SQLite/SQL Server)                 │
│  ┌────────────────────┐  ┌──────────────────────────┐       │
│  │   AspNetUsers      │◄─┤     UserProfiles         │       │
│  │  (Identity table)  │  │   (Extended profile)     │       │
│  └────────────────────┘  └──────────────────────────┘       │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 2. MapToDto(user)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                        Mapping Logic                          │
│                                                              │
│  return new UserProfileResponseDto                           │
│  {                                                           │
│    UserId = user.Id,                                         │
│    Email = user.Email,                                       │
│    Name = user.UserName,                                     │
│                                                              │
│    // From UserProfile entity                               │
│    FirstName = user.Profile?.FirstName ?? "",  ← NEW        │
│    LastName = user.Profile?.LastName ?? "",    ← NEW        │
│    ProfilePictureUrl = user.Profile?.ProfilePictureUrl ?? "",│
│                                                              │
│    // From User (IdentityUser)                              │
│    PhoneNumber = user.PhoneNumber ?? "",       ← NEW        │
│                                                              │
│    // Other profile fields...                               │
│    Gender = user.Profile?.Gender ?? Gender.Unknown,          │
│    ActivityLevel = user.Profile?.ActivityLevel ?? None,      │
│    // ...                                                    │
│  };                                                          │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 3. Return Result.Success(dto)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                    JSON Serialization                         │
│  (CXJsonSerializerOptions.Default - camelCase)               │
│                                                              │
│  {                                                           │
│    "userId": "550e8400-...",                                 │
│    "email": "athlete@ironlogic.ai",                          │
│    "name": "johndoe",                                        │
│    "firstName": "John",          ← NEW (never null)         │
│    "lastName": "Doe",            ← NEW (never null)         │
│    "phoneNumber": "+1234...",    ← NEW (never null)         │
│    "profilePictureUrl": "https://...", ← NEW (never null)   │
│    "gender": 1,                  ← Enum as integer          │
│    "activityLevel": 3,           ← Enum as integer          │
│    "dateOfBirth": "1990-05-15T00:00:00Z",                   │
│    "height": 180.5,                                          │
│    "currentWeight": 82.3,                                    │
│    "targetWeight": 78.0,                                     │
│    "bio": "Passionate athlete."                              │
│  }                                                           │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ HTTP 200 OK
       │ Content-Type: application/json
       ▼
┌──────────────┐
│   Frontend   │
│   (Angular)  │
│              │
│  receives:   │
│  AthleteProfile {                                            │
│    firstName: "John",    ← Safe to use (never null)         │
│    lastName: "Doe",      ← Safe to use (never null)         │
│    ...                                                       │
│  }                                                           │
└──────────────┘
```

## Data Flow: PUT /api/v1/Account/me

```
┌──────────────┐
│   Frontend   │
│   (Angular)  │
│              │
│  Sends:      │
│  {                                                           │
│    "firstName": "John",                                      │
│    "lastName": "Doe",                                        │
│    "phoneNumber": "+12345678901",                            │
│    "profilePictureUrl": "https://example.com/avatar.jpg",    │
│    "activityLevel": 3                                        │
│  }                                                           │
└──────┬───────┘
       │ HTTP PUT /api/v1/Account/me
       │ Authorization: Bearer {token}
       │ Content-Type: application/json
       ▼
┌──────────────────────────────────────────────────────────────┐
│                      AccountController                        │
│  [HttpPut("me")]                                             │
│  public async Task<IActionResult> UpdateProfile(             │
│    [FromBody] UpdateProfileDto request)                      │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 1. Model Validation
       │    - [EmailAddress] on Email
       │    - [Phone] on PhoneNumber      ← NEW validation
       │    - [Url] on ProfilePictureUrl  ← NEW validation
       │    - [StringLength] on all fields
       │    - [Range] on Height, Weights
       ▼
┌──────────────────────────────────────────────────────────────┐
│                 UpdateProfileDto (Validated)                  │
│                                                              │
│  Email: string?                                              │
│  Name: string?                                               │
│  FirstName: string?           ← NEW (optional)              │
│  LastName: string?            ← NEW (optional)              │
│  PhoneNumber: string?         ← NEW (optional, validated)   │
│  ProfilePictureUrl: string?   ← NEW (optional, validated)   │
│  Gender: Gender (enum)                                       │
│  DateOfBirth: DateTime?                                      │
│  Height: decimal?                                            │
│  CurrentWeight: decimal?                                     │
│  TargetWeight: decimal?                                      │
│  ActivityLevel: ActivityLevel (enum)                         │
│  Bio: string?                                                │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 2. Call: profileService.UpdateProfileAsync(userId, request, ct)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                       ProfileService                          │
│  public async Task<Result<UserProfileResponseDto>>           │
│  UpdateProfileAsync(string userId, UpdateProfileDto request, │
│                     CancellationToken ct)                     │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 3. Load User with Profile
       │    .Include(u => u.Profile)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                  Database Query                               │
│  var user = await userManager.Users                          │
│    .Include(u => u.Profile)                                  │
│    .FirstOrDefaultAsync(u => u.Id == userId);                │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 4. Update Entity Properties
       ▼
┌──────────────────────────────────────────────────────────────┐
│                     Property Updates                          │
│                                                              │
│  // Update User (IdentityUser) entity:                       │
│  if (!string.IsNullOrWhiteSpace(request.Email))              │
│    user.Email = request.Email;                               │
│                                                              │
│  if (!string.IsNullOrWhiteSpace(request.Name))               │
│    user.UserName = request.Name;                             │
│                                                              │
│  if (!string.IsNullOrWhiteSpace(request.PhoneNumber))        │
│    user.PhoneNumber = request.PhoneNumber;  ← NEW           │
│                                                              │
│  // Update UserProfile entity:                               │
│  if (!string.IsNullOrWhiteSpace(request.FirstName))          │
│    profile.FirstName = request.FirstName;   ← NEW           │
│                                                              │
│  if (!string.IsNullOrWhiteSpace(request.LastName))           │
│    profile.LastName = request.LastName;     ← NEW           │
│                                                              │
│  if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))  │
│    profile.ProfilePictureUrl = request.ProfilePictureUrl; ← NEW │
│                                                              │
│  profile.Gender = request.Gender;                            │
│  profile.ActivityLevel = request.ActivityLevel;              │
│  // ... other profile fields                                │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 5. Save Changes
       ▼
┌──────────────────────────────────────────────────────────────┐
│                     Database Persistence                      │
│                                                              │
│  await userManager.UpdateAsync(user);                        │
│  await dbContext.SaveChangesAsync(ct);                       │
│                                                              │
│  ┌────────────────────┐  ┌──────────────────────────┐       │
│  │   AspNetUsers      │  │     UserProfiles         │       │
│  │  UPDATE            │  │     UPDATE               │       │
│  │  PhoneNumber       │  │     FirstName            │       │
│  └────────────────────┘  │     LastName             │       │
│                          │     ProfilePictureUrl    │       │
│                          └──────────────────────────┘       │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ 6. MapToDto(user) - Return updated profile
       ▼
┌──────────────────────────────────────────────────────────────┐
│                   JSON Response (200 OK)                      │
│  {                                                           │
│    "userId": "550e8400-...",                                 │
│    "firstName": "John",      ← Updated                      │
│    "lastName": "Doe",        ← Updated                      │
│    "phoneNumber": "+1234...", ← Updated                     │
│    "profilePictureUrl": "https://...", ← Updated            │
│    // ... rest of profile                                    │
│  }                                                           │
└──────┬───────────────────────────────────────────────────────┘
       │
       │ HTTP 200 OK
       ▼
┌──────────────┐
│   Frontend   │
│   (Angular)  │
│              │
│  Updates UI  │
│  with new    │
│  profile data│
└──────────────┘
```

## Key Design Decisions

### 1. Data Storage Strategy

| Field | Stored In | Rationale |
|-------|-----------|-----------|
| `PhoneNumber` | `User` (IdentityUser) | Built-in ASP.NET Identity property |
| `FirstName` | `UserProfile` | Extended profile data, not identity |
| `LastName` | `UserProfile` | Extended profile data, not identity |
| `ProfilePictureUrl` | `UserProfile` | Extended profile data, not identity |

### 2. Null Handling Strategy

**Problem**: Frontend crashes when receiving `null` for string properties.

**Solution**: 
```csharp
// In MapToDto:
FirstName = user.Profile?.FirstName ?? string.Empty,
LastName = user.Profile?.LastName ?? string.Empty,
PhoneNumber = user.PhoneNumber ?? string.Empty,
ProfilePictureUrl = user.Profile?.ProfilePictureUrl ?? string.Empty,
```

**Result**: Frontend always receives empty strings, never `null`.

### 3. Validation Strategy

```csharp
// UpdateProfileDto:
[StringLength(50, ErrorMessage = "First name must be 50 characters or fewer")]
public string? FirstName { get; init; }

[Phone(ErrorMessage = "Invalid phone number format")]
[StringLength(20, ErrorMessage = "Phone number must be 20 characters or fewer")]
public string? PhoneNumber { get; init; }

[Url(ErrorMessage = "Invalid URL format")]
[StringLength(500, ErrorMessage = "Profile picture URL must be 500 characters or fewer")]
public string? ProfilePictureUrl { get; init; }
```

**Result**: Data integrity enforced at API boundary.

## Security Considerations

### PII Protection
- **PhoneNumber**: Sensitive PII
  - ✅ Never logged in plain text
  - ✅ Encrypted in transit (HTTPS)
  - ⚠️ Consider encryption at rest
  - ⚠️ Implement rate limiting on updates

### URL Validation
- **ProfilePictureUrl**:
  - ✅ URL format validation
  - ⚠️ Consider URL whitelist for hosted images
  - ⚠️ Frontend must sanitize before rendering

## Performance Analysis

### Database Queries
- **GET Profile**: 1 query with `.Include(u => u.Profile)`
- **UPDATE Profile**: 2 queries (UserManager.UpdateAsync + SaveChangesAsync)
- **No N+1 queries**
- **No additional round trips**

### Memory Impact
- Additional string properties: Negligible (~200 bytes per profile)
- No collection properties added
- No complex object graphs

### Response Size
- Additional ~100-200 bytes per profile response
- Minimal impact on network transfer

---

## Legend

```
┌───────┐
│ Box   │  = Component/Layer
└───────┘

   │
   ▼      = Data flow direction

  ◄─┤     = Relationship (FK, navigation property)

← NEW     = Newly added field/property
```

---

**Last Updated**: 2026-04-12  
**Diagram Version**: 1.0  
**Author**: Senior .NET Developer
