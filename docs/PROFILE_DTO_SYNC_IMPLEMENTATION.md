# Profile DTO Synchronization Implementation

## Overview
This document describes the changes made to synchronize the `UserProfileResponseDto` with database entities and frontend requirements, ensuring proper handling of user profile data including FirstName, LastName, PhoneNumber, and ProfilePictureUrl.

## Changes Implemented

### 1. Updated `UserProfileResponseDto`
**File**: `src/IronLogic.Application/DTOs/Profile/UserProfileResponseDto.cs`

**Added Properties**:
```csharp
/// <summary>
/// Gets or sets the user's first name.
/// </summary>
public string FirstName { get; init; } = string.Empty;

/// <summary>
/// Gets or sets the user's last name.
/// </summary>
public string LastName { get; init; } = string.Empty;

/// <summary>
/// Gets or sets the user's phone number.
/// </summary>
public string PhoneNumber { get; init; } = string.Empty;

/// <summary>
/// Gets or sets the user's profile picture URL.
/// </summary>
public string ProfilePictureUrl { get; init; } = string.Empty;
```

**Key Design Decisions**:
- All new properties use `string.Empty` as the default instead of `null` to prevent frontend crashes
- Properties are non-nullable to ensure consistent API responses
- XML documentation follows project standards

### 2. Updated `UpdateProfileDto`
**File**: `src/IronLogic.Application/DTOs/Profile/UpdateProfileDto.cs`

**Added Properties**:
```csharp
[StringLength(50, ErrorMessage = "First name must be 50 characters or fewer")]
public string? FirstName { get; init; }

[StringLength(50, ErrorMessage = "Last name must be 50 characters or fewer")]
public string? LastName { get; init; }

[Phone(ErrorMessage = "Invalid phone number format")]
[StringLength(20, ErrorMessage = "Phone number must be 20 characters or fewer")]
public string? PhoneNumber { get; init; }

[Url(ErrorMessage = "Invalid URL format")]
[StringLength(500, ErrorMessage = "Profile picture URL must be 500 characters or fewer")]
public string? ProfilePictureUrl { get; init; }
```

**Validation Rules**:
- `FirstName` and `LastName`: Max 50 characters each
- `PhoneNumber`: Must be valid phone format, max 20 characters
- `ProfilePictureUrl`: Must be valid URL format, max 500 characters
- All properties are nullable (optional) in update requests

### 3. Updated `UserProfile` Entity
**File**: `src/IronLogic.Domain/Entities/UserProfile.cs`

**Added Properties**:
```csharp
/// <summary>
/// Gets or sets the user's first name.
/// </summary>
public string? FirstName { get; set; }

/// <summary>
/// Gets or sets the user's last name.
/// </summary>
public string? LastName { get; set; }

/// <summary>
/// Gets or sets the user's profile picture URL.
/// </summary>
public string? ProfilePictureUrl { get; set; }
```

**Rationale**:
- These properties belong in `UserProfile` rather than extending `IdentityUser` (User entity)
- Keeps profile-specific data separate from authentication/identity concerns
- Maintains Clean Architecture separation

### 4. Updated `ProfileService`
**File**: `src/IronLogic.Infrastructure/Services/ProfileService.cs`

#### MapToDto Method
**Before**:
```csharp
private static UserProfileResponseDto MapToDto(User user)
{
    return new UserProfileResponseDto
    {
        UserId = user.Id,
        Email = user.Email,
        Name = user.UserName,
        Gender = user.Profile?.Gender ?? Domain.Enums.Gender.Unknown,
        // ... other fields
    };
}
```

**After**:
```csharp
private static UserProfileResponseDto MapToDto(User user)
{
    return new UserProfileResponseDto
    {
        UserId = user.Id,
        Email = user.Email,
        Name = user.UserName,
        FirstName = user.Profile?.FirstName ?? string.Empty,
        LastName = user.Profile?.LastName ?? string.Empty,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        ProfilePictureUrl = user.Profile?.ProfilePictureUrl ?? string.Empty,
        Gender = user.Profile?.Gender ?? Domain.Enums.Gender.Unknown,
        // ... other fields
    };
}
```

**Key Changes**:
- Maps `FirstName`, `LastName`, and `ProfilePictureUrl` from `user.Profile`
- Maps `PhoneNumber` from `user.PhoneNumber` (built-in IdentityUser property)
- Uses null-coalescing to `string.Empty` for all nullable string properties
- Ensures frontend never receives `null` for string properties

#### UpdateProfileAsync Method
**Added Logic**:
```csharp
if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
{
    user.PhoneNumber = request.PhoneNumber;
}

if (!string.IsNullOrWhiteSpace(request.FirstName))
{
    profile.FirstName = request.FirstName;
}

if (!string.IsNullOrWhiteSpace(request.LastName))
{
    profile.LastName = request.LastName;
}

if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
{
    profile.ProfilePictureUrl = request.ProfilePictureUrl;
}
```

**Behavior**:
- Only updates fields if they are provided and non-empty
- `PhoneNumber` updates the `User` entity (IdentityUser)
- `FirstName`, `LastName`, `ProfilePictureUrl` update the `UserProfile` entity
- Maintains separation of concerns between identity and profile data

## Database Migration Required

### Migration Name
`AddProfileNameAndPictureFields`

### Migration Command
```bash
cd src/IronLogic.Infrastructure
dotnet ef migrations add AddProfileNameAndPictureFields --project ../IronLogic.Infrastructure --startup-project ../IronLogic.Api
dotnet ef database update --project ../IronLogic.Infrastructure --startup-project ../IronLogic.Api
```

### Expected Schema Changes
The migration will add the following columns to the `UserProfiles` table:
- `FirstName` (nvarchar(50), nullable)
- `LastName` (nvarchar(50), nullable)
- `ProfilePictureUrl` (nvarchar(500), nullable)

## Frontend Compatibility

### TypeScript Interface Alignment
The frontend `AthleteProfile` interface should match:

```typescript
interface AthleteProfile {
  userId: string;
  email: string | null;
  name: string | null;
  firstName: string;        // Now guaranteed non-null (empty string)
  lastName: string;         // Now guaranteed non-null (empty string)
  phoneNumber: string;      // Now guaranteed non-null (empty string)
  profilePictureUrl: string; // Now guaranteed non-null (empty string)
  gender: number;
  dateOfBirth: string | null;
  height: number | null;
  currentWeight: number | null;
  targetWeight: number | null;
  activityLevel: number;
  bio: string | null;
}
```

### API Response Example
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "athlete@ironlogic.ai",
  "name": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+12345678901",
  "profilePictureUrl": "https://example.com/avatar.jpg",
  "gender": 1,
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "height": 180.5,
  "currentWeight": 82.3,
  "targetWeight": 78.0,
  "activityLevel": 3,
  "bio": "Passionate athlete focused on strength training."
}
```

### Null Safety
- **Before**: `firstName`, `lastName`, `phoneNumber`, `profilePictureUrl` could be `null`, causing frontend crashes
- **After**: These fields always return `string.Empty` when null in the database, preventing crashes

## Enum Serialization

### Gender Enum
**C# Definition** (`src/IronLogic.Domain/Enums/Gender.cs`):
```csharp
public enum Gender
{
    Unknown = 0,
    Male = 1,
    Female = 2,
    Other = 3
}
```

**JSON Serialization** (using default `CXJsonSerializerOptions`):
```json
{
  "gender": 1  // Serializes as integer
}
```

### ActivityLevel Enum
**C# Definition** (`src/IronLogic.Domain/Enums/ActivityLevel.cs`):
```csharp
public enum ActivityLevel
{
    None = 0,
    Sedentary = 1,
    LightlyActive = 2,
    ModeratelyActive = 3,
    VeryActive = 4,
    ExtremelyActive = 5
}
```

**JSON Serialization**:
```json
{
  "activityLevel": 3  // Serializes as integer
}
```

**Frontend Mapping**:
```typescript
const ActivityLevelMap: Record<string, number> = {
  'Sedentary': 1,
  'Lightly Active': 2,
  'Moderately Active': 3,
  'Very Active': 4,
  'Extremely Active': 5
};
```

## Testing Checklist

### Backend Tests
- [ ] Test `GetProfileAsync` returns all new fields with empty strings for null values
- [ ] Test `UpdateProfileAsync` correctly updates `FirstName`
- [ ] Test `UpdateProfileAsync` correctly updates `LastName`
- [ ] Test `UpdateProfileAsync` correctly updates `PhoneNumber`
- [ ] Test `UpdateProfileAsync` correctly updates `ProfilePictureUrl`
- [ ] Test validation for `PhoneNumber` format
- [ ] Test validation for `ProfilePictureUrl` URL format
- [ ] Test that enum values serialize correctly

### Frontend Tests
- [ ] Test profile form correctly displays `FirstName` and `LastName`
- [ ] Test profile form correctly displays `PhoneNumber`
- [ ] Test profile form correctly displays `ProfilePictureUrl`
- [ ] Test profile update sends all new fields to backend
- [ ] Test that empty strings don't cause rendering issues
- [ ] Test enum mapping for `Gender` and `ActivityLevel`

### Integration Tests
- [ ] Test end-to-end profile retrieval
- [ ] Test end-to-end profile update
- [ ] Test that profile picture URL updates are reflected in UI
- [ ] Test phone number formatting and validation

## Breaking Changes

### None
This is a non-breaking change because:
1. New properties in the response DTO are always populated (never `null`)
2. New properties in the update DTO are optional
3. Existing functionality remains unchanged
4. Frontend can gracefully handle both old and new API versions

## Migration Path

### Step 1: Deploy Backend Changes
1. Apply database migration: `dotnet ef database update`
2. Deploy updated API code
3. Verify API health endpoint

### Step 2: Verify API Responses
```bash
# Test GET /api/v1/Account/me
curl -X GET https://localhost:5011/api/v1/Account/me \
  -H "Authorization: Bearer YOUR_TOKEN" \
  | jq .

# Expected: New fields should appear in response
```

### Step 3: Update Frontend (Optional - Already Compatible)
If the frontend is using strict typing, update the TypeScript interface to match the new contract.

### Step 4: Verify End-to-End
1. Log in to the application
2. Navigate to Profile page
3. Verify all fields display correctly
4. Update profile with new data
5. Verify changes persist after refresh

## Rollback Plan

### If Issues Occur
1. **Revert Code**: 
   ```bash
   git revert HEAD
   ```

2. **Revert Migration** (if applied):
   ```bash
   dotnet ef database update PreviousMigrationName --project src/IronLogic.Infrastructure --startup-project src/IronLogic.Api
   dotnet ef migrations remove --project src/IronLogic.Infrastructure --startup-project src/IronLogic.Api
   ```

3. **Redeploy**: Deploy the reverted code

## Performance Considerations

### No Performance Impact
- New fields are simple string properties
- No additional database joins required
- `UserProfile` is already included in the query
- No additional round trips to the database

## Security Considerations

### ProfilePictureUrl Validation
- **Risk**: User could supply malicious URL
- **Mitigation**: 
  - `[Url]` validation attribute ensures valid URL format
  - Consider implementing URL whitelist if hosting images internally
  - Frontend should sanitize and validate URLs before rendering

### PhoneNumber Privacy
- **Risk**: Sensitive PII exposure
- **Mitigation**:
  - Never log phone numbers in plain text
  - Consider encrypting phone numbers at rest
  - Implement rate limiting on profile updates

## Future Enhancements

### Recommended Improvements
1. **Profile Picture Upload**: Implement Azure Blob Storage integration for profile pictures instead of storing URLs
2. **Phone Number Verification**: Add OTP-based phone verification flow
3. **Name Change Audit**: Track history of name changes for compliance
4. **Avatar Generation**: Auto-generate avatar from initials if no picture provided

## References

- [Project Coding Standards](.github/copilot-instructions.md)
- [Clean Architecture Guidelines](docs/CLEAN_ARCHITECTURE.md)
- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

---

**Document Version**: 1.0  
**Last Updated**: 2026-04-12  
**Author**: Senior .NET Developer  
**Status**: Implemented - Migration Pending
