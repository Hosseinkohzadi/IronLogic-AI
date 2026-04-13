# User Management - Quick Testing Guide

## Prerequisites

1. ✅ Backend running on `https://localhost:5011`
2. ✅ Frontend running on `https://localhost:4200`
3. ✅ Database seeded with test users

---

## Test Steps

### 1. **Login as Admin**

Navigate to: `https://localhost:4200/auth/login`

**Credentials:**
- **Email:** `admin@ironlogic.ai`
- **Password:** `Admin@123456`

### 2. **Navigate to User Management**

After login, go to: `https://localhost:4200/admin/users`

Or click: **Admin** → **Users** in the sidebar

---

## What You Should See

### **Top Section - KPI Cards**
- Premium Subscribers
- Weekly Active (WAU)
- Total Sessions
- Churn Risk

### **Grid Section - User List**

The grid should display all users from the database:

#### **Column 1: USER**
- Avatar (or initials)
- Full name
- Email address (subtitle)

#### **Column 2: EMAIL**
- Email icon
- Email address

#### **Column 3: ROLE**
Badge colors:
- 🟣 **Admin** - Purple badge
- 🟢 **Coach** - Green badge
- 🔵 **Athlete** - Blue badge

#### **Column 4: PLAN**
Badge colors:
- 🟡 **Elite** - Gold/Amber badge
- 🟣 **Pro** - Indigo badge
- ⚪ **Basic** - Gray badge

#### **Column 5: STATUS**
Badge colors:
- 🟢 **Active** - Green badge (subscription valid)
- 🔴 **Expired** - Red badge (no subscription)

#### **Column 6: ACTION**
- Edit button (three dots icon)

---

## Test Features

### ✅ **1. Search**
Type in the search box at the top:
- Search by name: `admin`
- Search by email: `@ironlogic.ai`
- Search by role: `Admin`
- Search by plan: `Elite`

**Expected:** Grid filters in real-time

---

### ✅ **2. Column Filters**
Click the filter icon on any column header:
- **Role:** Filter by Admin/Coach/Athlete
- **Plan:** Filter by Basic/Pro/Elite
- **Status:** Filter by Active/Expired

**Expected:** Grid shows only matching rows

---

### ✅ **3. Sorting**
Click on sortable column headers:
- Click once: Sort ascending
- Click again: Sort descending
- Click third time: Clear sort

**Expected:** Grid reorders rows

---

### ✅ **4. Selection**
- Click checkbox in the first column
- Select multiple users

**Expected:**
- Selected rows highlighted
- Bulk action bar appears at the bottom
- Shows count: "3 accounts selected"

---

### ✅ **5. Row Click**
Click anywhere on a row (except action button)

**Expected:** Drawer opens from the right with user details

---

### ✅ **6. Edit Action**
Click the three dots icon on any row

**Expected:** Edit form opens on the right

---

### ✅ **7. Loading State**
Refresh the page

**Expected:**
- Shows spinner: "Loading users..."
- Spinner disappears when data loads

---

### ✅ **8. Export**
Click the "Export" button

**Expected:** Downloads CSV file with all users

---

## Sample Users to Verify

Based on the seeded database, you should see at least:

### **User 1: Admin User**
- **Name:** kohzadi90 (or full name if added)
- **Email:** kohzadi90@gmail.com
- **Role:** 🟣 Admin
- **Plan:** 🟡 Elite (if seeded)
- **Status:** 🟢 Active (if subscription exists)

### **User 2: Athlete**
- **Name:** athlete (or full name if added)
- **Email:** athlete@ironlogic.ai
- **Role:** 🔵 Athlete
- **Plan:** 🟣 Pro (if seeded)
- **Status:** 🟢 Active (if subscription exists)

---

## API Testing (Optional)

### **1. Test GET Endpoint Directly**

**Swagger UI:** `https://localhost:5011/swagger`

1. Click "Authorize"
2. Login to get token
3. Find: `GET /api/v1/admin/users`
4. Click "Try it out"
5. Click "Execute"

**Expected Response:**
```json
[
  {
    "id": "...",
    "firstName": "kohzadi90",
    "lastName": "",
    "email": "kohzadi90@gmail.com",
    "role": "Admin",
    "plan": "Elite",
    "status": "Active",
    "subscriptionEndDate": "2027-12-31T23:59:59Z",
    "profileImageUrl": ""
  }
]
```

---

### **2. Test with Postman/cURL**

```bash
# 1. Login
curl -X POST https://localhost:5011/api/v1/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@ironlogic.ai",
    "password": "Admin@123456"
  }'

# Copy the token from response

# 2. Get Users
curl -X GET https://localhost:5011/api/v1/admin/users \
  -H "Authorization: Bearer <YOUR_TOKEN>"
```

---

## Troubleshooting

### ❌ **Issue: Empty Grid**

**Possible Causes:**
1. Backend not running
2. API endpoint not accessible
3. No users in database
4. Authorization token missing/invalid

**Solutions:**
- Check backend is running: `dotnet run`
- Check browser console for errors (F12)
- Verify login was successful
- Check database has users: `SELECT * FROM AspNetUsers`

---

### ❌ **Issue: Loading Forever**

**Possible Causes:**
1. CORS error
2. API endpoint not found
3. Network issue

**Solutions:**
- Check browser console (F12) → Network tab
- Verify API URL: `https://localhost:5011/api/v1/admin/users`
- Check CORS is configured in backend

---

### ❌ **Issue: Wrong Badge Colors**

**Possible Causes:**
1. Role/Plan/Status values don't match expected values
2. Badge style not configured correctly

**Solutions:**
- Check API response values
- Verify badgeStyle in column config
- Check getBadgeClass() method in grid-body.ts

---

### ❌ **Issue: 401 Unauthorized**

**Possible Causes:**
1. Token expired
2. Not logged in as Admin
3. Token not sent in request

**Solutions:**
- Re-login as admin
- Check token in localStorage
- Verify `[Authorize(Roles = "Admin")]` on endpoint

---

### ❌ **Issue: 403 Forbidden**

**Possible Causes:**
1. User logged in but not Admin role

**Solutions:**
- Login as admin@ironlogic.ai
- Check user has Admin role in database:
  ```sql
  SELECT u.Email, r.Name
  FROM AspNetUsers u
  JOIN AspNetUserRoles ur ON u.Id = ur.UserId
  JOIN AspNetRoles r ON ur.RoleId = r.Id
  ```

---

## Expected Performance

- **Initial Load:** < 1 second
- **Search:** Instant (client-side filtering)
- **Column Filter:** Instant (client-side filtering)
- **Sort:** Instant (client-side sorting)
- **Row Click:** Instant drawer open

---

## Success Criteria

✅ Grid loads with all users from database  
✅ Role badges show correct colors (Purple/Green/Blue)  
✅ Plan badges show correct colors (Gold/Indigo/Gray)  
✅ Status badges show correct colors (Green/Red)  
✅ Search filters in real-time  
✅ Column filters work correctly  
✅ Sorting works on all columns  
✅ Selection shows bulk action bar  
✅ Row click opens drawer  
✅ Edit button opens form  
✅ Loading state shows spinner  

---

## Screenshot Checklist

Take screenshots of:
- [ ] Full grid with data
- [ ] Role badge colors (Admin/Coach/Athlete)
- [ ] Plan badge colors (Elite/Pro/Basic)
- [ ] Status badge colors (Active/Expired)
- [ ] Search functionality
- [ ] Column filters
- [ ] Selection with bulk actions
- [ ] Loading state
- [ ] Empty state (if you clear all filters)

---

## Next Test: After Seeding More Users

If you seed the database with more realistic user data:

1. **Add users with different roles:**
   - Add Coach role users
   - Add more Athlete users

2. **Add users with different plans:**
   - Create Pro subscriptions
   - Create Elite subscriptions
   - Leave some with Basic (no subscription)

3. **Add users with expired subscriptions:**
   - Set subscription end date in the past

**Expected:** Grid should display all variations correctly with proper badge colors!

---

## Support

If you encounter issues:
1. Check browser console (F12)
2. Check backend logs
3. Verify database has users
4. Test API endpoint in Swagger
5. Check CORS configuration

---

**Status:** ✅ Ready for Testing!

Navigate to `https://localhost:4200/admin/users` and verify all features! 🎉
