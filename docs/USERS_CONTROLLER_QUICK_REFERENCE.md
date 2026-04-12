# UsersController - Quick Reference

## Admin Credentials
- **Email:** `admin@ironlogic.ai`
- **Password:** `Admin@123456`
- **Role:** `Admin`

---

## Endpoints

### 1. Get User Details
```http
GET /api/v1/admin/users/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": "user-id",
  "userName": "admin@ironlogic.ai",
  "email": "admin@ironlogic.ai",
  "emailConfirmed": true,
  "phoneNumber": null,
  "phoneNumberConfirmed": false,
  "twoFactorEnabled": false,
  "lockoutEnd": null,
  "lockoutEnabled": false,
  "accessFailedCount": 0,
  "roles": ["Admin"],
  "claims": []
}
```

---

### 2. Update User
```http
PUT /api/v1/admin/users/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "email": "newemail@ironlogic.ai",
  "name": "New Name",
  "roles": ["Admin", "User"]
}
```

**Response:**
```json
{
  "message": "User updated successfully",
  "userId": "user-id",
  "email": "newemail@ironlogic.ai",
  "userName": "newemail@ironlogic.ai"
}
```

---

## Quick Test in Swagger

1. **Login to get Admin token:**
   ```http
   POST /api/v1/Auth/login
   {
     "email": "admin@ironlogic.ai",
     "password": "Admin@123456"
   }
   ```
   Copy the `token` from response.

2. **Authorize in Swagger:**
   - Click "Authorize" button
   - Enter: `Bearer {your-token}`
   - Click "Authorize"

3. **Test Get User:**
   ```http
   GET /api/v1/admin/users/{user-id}
   ```

4. **Test Update User:**
   ```http
   PUT /api/v1/admin/users/{user-id}
   {
     "roles": ["Admin", "User"]
   }
   ```

---

## Angular Service

```typescript
@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/admin/users`;

  getUserById(userId: string): Observable<UserDetailDto> {
    return this.http.get<UserDetailDto>(`${this.apiUrl}/${userId}`);
  }

  updateUser(userId: string, updateDto: UpdateUserDto): Observable<UpdateUserResponse> {
    return this.http.put<UpdateUserResponse>(`${this.apiUrl}/${userId}`, updateDto);
  }
}
```

---

## Component Usage

```typescript
// Load user details
this.adminUsersService.getUserById(userId).subscribe({
  next: (details) => {
    this.userDetails.set(details);
  },
  error: (error) => {
    this.toast.error(error.error?.message ?? 'Failed to load user');
  }
});

// Update user
this.adminUsersService.updateUser(userId, { 
  email: 'new@ironlogic.ai',
  roles: ['Admin', 'User'] 
}).subscribe({
  next: (response) => {
    this.toast.success(response.message);
  },
  error: (error) => {
    this.toast.error(error.error?.message ?? 'Failed to update user');
  }
});
```

---

## TypeScript Interfaces

```typescript
export interface UserDetailDto {
  id: string;
  userName: string | null;
  email: string | null;
  emailConfirmed: boolean;
  phoneNumber: string | null;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
  lockoutEnd: string | null;
  lockoutEnabled: boolean;
  accessFailedCount: number;
  roles: string[];
  claims: UserClaimDto[];
}

export interface UserClaimDto {
  type: string;
  value: string;
}

export interface UpdateUserDto {
  email?: string;
  name?: string;
  roles?: string[];
}

export interface UpdateUserResponse {
  message: string;
  userId: string;
  email: string;
  userName: string;
}
```

---

## Error Responses

### Not Found (404)
```json
{
  "message": "User not found"
}
```

### Bad Request (400)
```json
{
  "message": "Failed to update email",
  "errors": [
    "Email 'admin@ironlogic.ai' is already taken."
  ]
}
```

### Unauthorized (401)
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### Forbidden (403)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

---

## Status

? **BUILD SUCCESSFUL**  
? **READY FOR TESTING**  
? **ANGULAR INTEGRATION READY**

---

## Files Created

1. ? `src/IronLogic.Application/DTOs/User/UserDetailDto.cs`
2. ? `src/IronLogic.Application/DTOs/User/UpdateUserDto.cs`
3. ? `src/IronLogic.Api/Controllers/Admin/UsersController.cs`
4. ? `docs/USERS_CONTROLLER_IMPLEMENTATION.md`
5. ? `docs/USERS_CONTROLLER_QUICK_REFERENCE.md`
