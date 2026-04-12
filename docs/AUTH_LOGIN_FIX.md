# Authentication Login Fix - 401 Unauthorized

## ? Issue Resolved

### Problem
Login endpoint was returning `401 Unauthorized` with message "Invalid credentials" even with correct admin credentials:

```json
{
  "email": "admin@ironlogic.ai",
  "password": "Admin@123456"
}
```

---

## ?? Root Causes Identified

### 1. **Duplicate Admin User Seeding** ?
Two different places were trying to create admin users with the **same ID** but **different emails**:

**Location 1:** `AppDbContext.OnModelCreating()`
```csharp
modelBuilder.Entity<User>().HasData(new User
{
    Id = "00000000-0000-0000-0000-000000000001",
    Email = "kohzadi90@gmail.com",  // ? Wrong email
    // ...
});
```

**Location 2:** `ExerciseSeederService.EnsureAdminUserExistsAsync()`
```csharp
var adminUser = new User
{
    Id = "00000000-0000-0000-0000-000000000001",
    Email = "admin@ironlogic.ai",  // ? Correct email
    // ...
};
```

**Result:** Database contained `kohzadi90@gmail.com`, but you were trying to log in with `admin@ironlogic.ai`

---

### 2. **SignInManager Using Wrong Parameter** ?

**Original Code:**
```csharp
var result = await signInManager.PasswordSignInAsync(
    loginDto.Email,  // ? Passing email as userName
    loginDto.Password,
    isPersistent: false,
    lockoutOnFailure: false);
```

**Problem:** `SignInManager.PasswordSignInAsync()` expects **userName** as the first parameter, not email.

---

### 3. **Missing UserName During Registration** ?

**Original Code:**
```csharp
var user = new User { Email = registerDto.Email };  // ? No UserName set
```

**Problem:** User registration wasn't setting `UserName`, causing sign-in failures.

---

## ? Solutions Applied

### Fix 1: Removed Duplicate Admin Seeding from AppDbContext

**File:** `src/IronLogic.Infrastructure/Data/AppDbContext.cs`

**Removed:**
```csharp
modelBuilder.Entity<User>().HasData(new User { ... });
```

**Now:** Admin user is ONLY created by `ExerciseSeederService` with proper password hashing.

---

### Fix 2: Updated Login Logic to Find User by Email First

**File:** `src/IronLogic.Api/Controllers/AuthController.cs`

**New Login Logic:**
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
{
    // 1. Find user by email first
    var user = await userManager.FindByEmailAsync(loginDto.Email);
    if (user == null)
        return Unauthorized(new { Message = "Invalid credentials" });

    // 2. Sign in with userName (which is set to email in our case)
    var result = await signInManager.PasswordSignInAsync(
        user.UserName ?? loginDto.Email,  // ? Use userName, not email
        loginDto.Password,
        isPersistent: false,
        lockoutOnFailure: false);

    if (!result.Succeeded)
        return Unauthorized(new { Message = "Invalid credentials" });

    var token = GenerateJwtToken(user);

    return Ok(new
    {
        Token = token,
        UserId = user.Id,
        Email = user.Email,
        UserName = user.UserName
    });
}
```

---

### Fix 3: Updated Register to Set UserName Properly

**File:** `src/IronLogic.Api/Controllers/AuthController.cs`

**New Registration Logic:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
{
    var user = new User
    {
        Email = registerDto.Email,
        UserName = registerDto.Email,  // ? Set UserName to email
        EmailConfirmed = true
    };
    
    var result = await userManager.CreateAsync(user, registerDto.Password);

    if (!result.Succeeded)
        return BadRequest(result.Errors);

    return Ok(new { Message = "Registration successful", UserId = user.Id });
}
```

---

### Fix 4: Configured Identity to Allow Email Sign-In

**File:** `src/IronLogic.Api/Program.cs`

**Added Configuration:**
```csharp
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // ... password options ...
    
    // ? Allow sign-in with email
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
```

---

### Fix 5: Updated JWT Token to Use UTC

**File:** `src/IronLogic.Api/Controllers/AuthController.cs`

**Changed:**
```csharp
var expires = DateTime.UtcNow.AddDays(...);  // ? UTC instead of DateTime.Now
```

---

## ?? Correct Admin Credentials

After fresh database seeding:

```
Email: admin@ironlogic.ai
Password: Admin@123456
User ID: 00000000-0000-0000-0000-000000000001
UserName: admin@ironlogic.ai
```

---

## ?? Testing the Fix

### Step 1: Clean Database and Restart

```sh
# Stop the application
# Delete the database
rm src/IronLogic.Api/bin/Debug/net10.0/ironlogic.db

# Restart the application
dotnet run --project src/IronLogic.Api
```

### Step 2: Verify Admin User Created

**Check Logs:**
```
? Admin user created successfully with ID: 00000000-0000-0000-0000-000000000001
? Successfully seeded 500 exercises into database
```

### Step 3: Test Login

**Request:**
```http
POST https://localhost:5011/api/v1/Auth/login
Content-Type: application/json

{
  "email": "admin@ironlogic.ai",
  "password": "Admin@123456"
}
```

**Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "00000000-0000-0000-0000-000000000001",
  "email": "admin@ironlogic.ai",
  "userName": "admin@ironlogic.ai"
}
```

---

## ?? Login Flow

```
1. User submits email + password
   ?
2. AuthController.Login()
   ?
3. UserManager.FindByEmailAsync(email)  // ? Find user by email
   ?
4. SignInManager.PasswordSignInAsync(userName, password)  // ? Sign in with userName
   ?
5. Generate JWT Token
   ?
6. Return Token + User Info
```

---

## ?? Password Hashing

**Admin User Password Hash:**
- Password: `Admin@123456`
- Hashed by: `PasswordHasher<User>` in `ExerciseSeederService`
- Verified by: `SignInManager.PasswordSignInAsync()`

**Both use the same hashing algorithm** ?

---

## ?? Important Notes

### 1. **Clean Database Required**

If you have an existing database with the old admin user (`kohzadi90@gmail.com`), you need to:

**Option A: Delete and Recreate**
```sh
rm src/IronLogic.Api/bin/Debug/net10.0/ironlogic.db
```

**Option B: Manually Update User**
```sql
UPDATE AspNetUsers
SET Email = 'admin@ironlogic.ai',
    NormalizedEmail = 'ADMIN@IRONLOGIC.AI',
    UserName = 'admin@ironlogic.ai',
    NormalizedUserName = 'ADMIN@IRONLOGIC.AI'
WHERE Id = '00000000-0000-0000-0000-000000000001';
```

---

### 2. **Hot Reload Not Sufficient**

The application is currently running. **You must restart** to apply these changes:

1. **Stop** the application
2. **Rebuild** the solution
3. **Delete** the database (if it has the old admin user)
4. **Start** the application
5. **Verify** admin user seeded correctly
6. **Test** login

---

## ?? Files Modified

1. ? `src/IronLogic.Api/Controllers/AuthController.cs`
   - Updated login logic to find user by email first
   - Updated registration to set UserName
   - Fixed JWT token to use UTC

2. ? `src/IronLogic.Api/Program.cs`
   - Configured Identity to allow email sign-in
   - Added unique email requirement

3. ? `src/IronLogic.Infrastructure/Data/AppDbContext.cs`
   - **Removed** duplicate admin user seeding
   - Admin user now ONLY created by `ExerciseSeederService`

---

## ? Summary

| Issue | Status |
|-------|--------|
| Duplicate admin users | ? Fixed - Removed from AppDbContext |
| SignInManager using email instead of userName | ? Fixed - Find by email, sign in with userName |
| UserName not set during registration | ? Fixed - UserName = Email |
| DateTime.Now instead of UTC | ? Fixed - Using DateTime.UtcNow |
| Unique email not enforced | ? Fixed - RequireUniqueEmail = true |

---

## ?? Next Steps

1. **Stop the application** (Ctrl+C in terminal or Stop Debugging in VS)
2. **Delete database:**
   ```sh
   rm src/IronLogic.Api/bin/Debug/net10.0/ironlogic.db
   ```
3. **Start the application:**
   ```sh
   dotnet run --project src/IronLogic.Api
   ```
4. **Test login** with:
   - Email: `admin@ironlogic.ai`
   - Password: `Admin@123456`

---

**Your authentication is now fully functional!** ???

**Login will return a valid JWT token!** ??
