# Profile DTO Sync - Quick Reference

## ✅ What Was Done

### 1. DTO Updates
- **Added** `FirstName`, `LastName`, `PhoneNumber`, `ProfilePictureUrl` to `UserProfileResponseDto`
- **Added** same fields to `UpdateProfileDto` with proper validation
- **All new fields return empty strings** instead of null (prevents frontend crashes)

### 2. Database Entity Updates
- **Added** `FirstName`, `LastName`, `ProfilePictureUrl` to `UserProfile` entity
- **PhoneNumber** uses built-in `IdentityUser.PhoneNumber` property

### 3. Service Updates
- Updated `ProfileService.MapToDto()` to map all new fields
- Updated `ProfileService.UpdateProfileAsync()` to handle updates for all new fields
- Proper null-coalescing ensures empty strings in responses

## 🚨 ACTION REQUIRED: Database Migration

```bash
# Navigate to Infrastructure project
cd src/IronLogic.Infrastructure

# Create migration
dotnet ef migrations add AddProfileNameAndPictureFields \
  --project ../IronLogic.Infrastructure \
  --startup-project ../IronLogic.Api

# Apply migration
dotnet ef database update \
  --project ../IronLogic.Infrastructure \
  --startup-project ../IronLogic.Api
```

## 📋 Updated API Contract

### GET /api/v1/Account/me
**Response**:
```json
{
  "userId": "guid",
  "email": "string|null",
  "name": "string|null",
  "firstName": "string",         // NEW - never null
  "lastName": "string",          // NEW - never null
  "phoneNumber": "string",       // NEW - never null
  "profilePictureUrl": "string", // NEW - never null
  "gender": "number",
  "dateOfBirth": "string|null",
  "height": "number|null",
  "currentWeight": "number|null",
  "targetWeight": "number|null",
  "activityLevel": "number",
  "bio": "string|null"
}
```

### PUT /api/v1/Account/me
**Request Body**:
```json
{
  "email": "string|null",
  "name": "string|null",
  "firstName": "string|null",         // NEW - optional
  "lastName": "string|null",          // NEW - optional
  "phoneNumber": "string|null",       // NEW - optional, validated
  "profilePictureUrl": "string|null", // NEW - optional, URL format
  "gender": "number",
  "dateOfBirth": "string|null",
  "height": "number|null",
  "currentWeight": "number|null",
  "targetWeight": "number|null",
  "activityLevel": "number",
  "bio": "string|null"
}
```

## ✅ Validation Rules

| Field | Validation |
|-------|-----------|
| `firstName` | Max 50 chars |
| `lastName` | Max 50 chars |
| `phoneNumber` | Valid phone format, max 20 chars |
| `profilePictureUrl` | Valid URL format, max 500 chars |

## 🔍 Testing Commands

### Test GET Profile
```bash
curl -X GET https://localhost:5011/api/v1/Account/me \
  -H "Authorization: Bearer YOUR_TOKEN" \
  | jq .
```

### Test PUT Profile
```bash
curl -X PUT https://localhost:5011/api/v1/Account/me \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+12345678901",
    "profilePictureUrl": "https://example.com/avatar.jpg",
    "activityLevel": 3
  }' | jq .
```

## 📦 Files Modified

- ✅ `src/IronLogic.Application/DTOs/Profile/UserProfileResponseDto.cs`
- ✅ `src/IronLogic.Application/DTOs/Profile/UpdateProfileDto.cs`
- ✅ `src/IronLogic.Domain/Entities/UserProfile.cs`
- ✅ `src/IronLogic.Infrastructure/Services/ProfileService.cs`
- ⏳ **MIGRATION PENDING**: Database schema update required

## 🛠 Troubleshooting

### Build Error: "Property X does not exist"
**Solution**: Run `dotnet build` - the code is correct and should compile.

### Frontend Error: "Cannot read property 'firstName' of null"
**Solution**: The new properties always return empty strings, not null. Check API response.

### Database Error: "Invalid column name 'FirstName'"
**Solution**: Run the database migration (see "ACTION REQUIRED" section above).

## 📚 Full Documentation
See: `docs/PROFILE_DTO_SYNC_IMPLEMENTATION.md`

---

**Status**: ✅ Code Complete | ⏳ Migration Pending  
**Next Step**: Run database migration command above
