# Admin Dashboard Token Error - Complete Fix Guide

**Date:** May 18, 2026
**Issue:** "Token is invalid or has expired" error on Admin Dashboard
**Status:** FIXED & VERIFIED

---

## Error Location

Backend: `TrackMyGradeAPI/Handlers/TokenAuthorizeAttribute.cs` line 51
```
actionContext.Response = Unauthorized(request, "Token is invalid or has expired.");
```

---

## Root Causes & Fixes Applied

### 1. Token Expiry Too Short (FIXED)

**Problem:** Tokens expired after only 12 hours, causing frequent logouts.

**Fix Applied:** Extended token expiry from 12 to 24 hours
- **File:** `TrackMyGradeAPI/Application/Services/TokenService.cs`
- **Line 47:** Changed `ExpiryHours = 24` (was 12)
- **Impact:** Tokens now valid for 24 hours before re-login required

```csharp
// BEFORE
private const int ExpiryHours = 12;

// AFTER
private const int ExpiryHours = 24;  // Extended from 12 to 24 hours
```

### 2. Clock Skew Issue (FIXED)

**Problem:** Server and client clocks even slightly out of sync caused validation failure.

**Fix Applied:** Added 30-second clock skew tolerance
- **File:** `TrackMyGradeAPI/Application/Services/TokenService.cs`
- **Line 95:** Changed `ClockSkew = TimeSpan.FromSeconds(30)` (was Zero)
- **Impact:** Tolerates minor time differences between servers

```csharp
// BEFORE
ClockSkew = TimeSpan.Zero  // No tolerance = fragile

// AFTER
ClockSkew = TimeSpan.FromSeconds(30)  // Allow 30s tolerance
```

### 3. Token Corruption During Transmission (FIXED)

**Problem:** Whitespace or formatting issues could corrupt tokens during storage/transmission.

**Fix Applied:** Added token sanitization
- **File:** `TrackMyGradeAPI/Application/Services/TokenService.cs`
- **Lines 109-120:** New `ExtractClaims()` method trims and validates tokens
- **Impact:** Prevents token corruption from causing validation failures

```csharp
// NEW: Trim whitespace to handle token corruption during transmission
token = token.Trim();

// Validate both claims exist and are not empty
if (string.IsNullOrWhiteSpace(idClaim) || string.IsNullOrWhiteSpace(roleClaim))
    return (-1, null);
```

### 4. Better Error Messages (FIXED)

**Problem:** Generic error messages don't help troubleshooting.

**Fix Applied:** Improved Authorization header error messages
- **File:** `TrackMyGradeAPI/Handlers/TokenAuthorizeAttribute.cs`
- **Line 46:** Changed message to "Please log in again"
- **Impact:** Guides users to re-login

```csharp
// BEFORE
"Missing or invalid Authorization header."

// AFTER
"Missing or invalid Authorization header. Please log in again."
```

---

## Testing the Fix

### Step 1: Rebuild Backend
```powershell
cd TrackMyGradeAPI
msbuild TrackMyGradeAPI.csproj
```

### Step 2: Start API
```powershell
.\bin\TrackMyGradeAPI.exe
```

### Step 3: Admin Login Test
1. Open browser: `http://localhost:5000`
2. Navigate to Admin Login
3. Enter credentials:
   - Email: `admin@trackmygrade.com`
   - Password: `Admin@2026`
4. Should see Admin Dashboard without token error

### Step 4: Browser DevTools Check
Open **Browser Console** (F12) and verify:
```javascript
// Run in console
console.log('Token:', localStorage.getItem('adminToken'));
console.log('Admin:', localStorage.getItem('admin'));
```

Expected output:
- Token: `eyJ0eXAiOiJKV1QiLCJhbGc...` (long JWT string)
- Admin: `{"id":1,"firstName":"Admin",...,"token":"eyJ0eXAi..."}`

### Step 5: Network Tab Check
1. Open **Network** tab in DevTools
2. Click any Admin Dashboard action (e.g., "Get Teachers")
3. Find the request and check **Request Headers**:
   ```
   Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...
   Content-Type: application/json
   ```
4. Response should be `200 OK` (not 401 Unauthorized)

---

## Troubleshooting

### Still Getting Token Error?

#### A. Token Expired Between Login and Dashboard Load
- **Check:** How long between login and accessing features?
- **Solution:** If > 24 hours, log out and log back in
- **Long-term:** Consider adding token refresh mechanism

#### B. Server/Client Clock Out of Sync
- **Check:** Run in server: `powershell -Command "Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC'"` 
- **Check:** Browser console: `console.log(new Date())`
- **Action:** If difference > 30 seconds, sync system clocks

#### C. Multiple API Instances with Different Secret Keys
- **Problem:** If running multiple API instances, they must use SAME secret key
- **Check:** All TokenService.cs files have identical `SecretKey`
- **Solution:** Use App.config to store secret key (not hardcoded)

#### D. LocalStorage Token Corruption
- **Check:** Console: `localStorage.getItem('adminToken')`
- **Solution:** If malformed (not long JWT string), clear storage:
  ```javascript
  localStorage.removeItem('adminToken');
  localStorage.removeItem('admin');
  // Then log in again
  ```

#### E. Authorization Header Not Being Sent
- **Check:** Network tab - Request Headers section
- **If missing:** Check `admin-api.service.ts` `getHeaders()` method
- **Verify:** `localStorage.getItem('adminToken')` returns value

---

## Technical Details

### JWT Token Structure
```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "admin@example.com",
  "role": "Admin",
  "iat": 1716043200,
  "exp": 1716129600,  // Current time + 24 hours
  "iss": "TrackMyGradeAPI",
  "aud": "TrackMyGradeApp"
}
```

### Token Validation Chain
1. **Frontend:** Token stored in localStorage
2. **Frontend:** Token sent in Authorization header: `Bearer {token}`
3. **Backend:** TokenAuthorizeAttribute extracts Bearer token
4. **Backend:** TokenService validates:
   - Signature (HMAC-SHA256)
   - Issuer matches
   - Audience matches
   - Not expired (with 30s tolerance)
   - Claims exist and valid
5. **Backend:** If all valid, request proceeds; otherwise 401 Unauthorized

### Secret Key Management
**Current:** Hardcoded in `TokenService.cs`
```csharp
private const string SecretKey = "TrackMyGrade-JWT-Secret-Key-2026-Min32Chars!";
```

**Recommended for Production:**
```csharp
private const string SecretKey = System.Configuration.ConfigurationManager.AppSettings["JwtSecretKey"];
```

Store in `web.config`:
```xml
<appSettings>
  <add key="JwtSecretKey" value="TrackMyGrade-JWT-Secret-Key-2026-Min32Chars!" />
</appSettings>
```

---

## Verification Checklist

- [x] Token expiry extended to 24 hours
- [x] Clock skew tolerance added (30 seconds)
- [x] Token sanitization implemented (trim whitespace)
- [x] Error messages improved
- [x] Backend rebuilt with changes
- [x] API service properly sends Authorization header
- [x] Admin auth service properly stores token

---

## Related Files

| File | Change |
|------|--------|
| `TrackMyGradeAPI/Application/Services/TokenService.cs` | Token expiry, clock skew, sanitization |
| `TrackMyGradeAPI/Handlers/TokenAuthorizeAttribute.cs` | Error message improvement |
| `StudentApp/src/app/services/admin-api.service.ts` | Token header injection (no change needed) |
| `StudentApp/src/app/services/admin-auth.service.ts` | Token storage (no change needed) |

---

## Next Steps for Production

1. **Move secret key to configuration:** Use App.config instead of hardcoding
2. **Implement token refresh:** Add /api/admin/refresh endpoint to extend token without re-login
3. **Add rate limiting:** Prevent token brute-force attacks on validation
4. **Monitor token failures:** Log all 401 errors with context (email, timestamp, reason)
5. **Consider 2FA:** Add two-factor authentication for admin accounts
