Issue Title: CORS preflight missing headers and 401 Unauthorized responses blocking Angular frontend requests

Root Cause:
The OWIN self-hosted pipeline lacked OWIN-level CORS handling. Although WebApi had EnableCorsAttribute configured, OWIN processes OPTIONS (preflight) requests before they reach Web API. Without OWIN-level CORS middleware, OPTIONS responses lacked Access-Control-Allow-* headers, causing the browser to block actual requests. Additionally, the TokenAuthorizeAttribute requires Authorization header which wasn't being exposed through CORS policy.

Fix Applied:

1. **Added Microsoft.Owin.Cors NuGet Package**
   - File: TrackMyGradeAPI/TrackMyGradeAPI.csproj
   - Added `<PackageReference Include="Microsoft.Owin.Cors" Version="4.2.2" />`

2. **Updated OWIN Startup to Enable CORS**
   - File: TrackMyGradeAPI/Startup.cs
   - Added using statements: `using Microsoft.Owin.Cors; using System.Web.Cors; using System.Threading.Tasks;`
   - Added CORS policy configuration BEFORE other middleware:
     - Allows origin: http://localhost:4200 (Angular app)
     - Allows headers: authorization, content-type, x-studenttoken, x-teacherid
     - Allows methods: GET, POST, PUT, DELETE, OPTIONS
     - Sets SupportsCredentials = true for token transmission
     - Applies via `app.UseCors(new CorsOptions { PolicyProvider = ... })`
   - Middleware order: UseCors() → Use<ErrorHandlingMiddleware> → Use<SecurityHeadersMiddleware> → UseWebApi()
   - This ensures preflight requests are handled before authentication/security middleware

3. **Simplified Database Constraint Handling**
   - File: TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs
   - Simplified EnsureSqlServerCheckConstraints() to be a no-op (moved to Configuration.Seed)
   - File: TrackMyGradeAPI/Migrations/Configuration.cs
   - Updated EnsureDataIntegrityConstraints() to use safer OBJECT_ID() calls with CHECK constraint type
   - Added Admins table constraint checks for Phone and Email

Testing Steps:

1. **Build the API:**
   ```powershell
   cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
   msbuild TrackMyGradeAPI.csproj
   ```

2. **Delete stale LocalDB database (if needed):**
   - The database is in SQL Server LocalDB with catalog name "TrackMyGrade"
   - If you see "Either the parameter @objname is ambiguous" error, delete the database via SQL Server Management Studio or run:
   ```sql
   EXEC msdb.dbo.sp_delete_database_backups @database_name = N'TrackMyGrade'
   ```
   - API will auto-create fresh database on next run via MigrateDatabaseToLatestVersion

3. **Start the API:**
   ```powershell
   cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\bin
   .\TrackMyGradeAPI.exe
   ```
   Expected output:
   ```
   TrackMyGrade API started successfully
   Listening on: http://localhost:5000
   ```

4. **Test preflight CORS from command line:**
   ```powershell
   curl -i -X OPTIONS http://localhost:5000/api/admin/subjects `
     -H "Origin: http://localhost:4200" `
     -H "Access-Control-Request-Method: GET" `
     -H "Access-Control-Request-Headers: authorization,content-type"
   ```
   Expected response headers:
   ```
   Access-Control-Allow-Origin: http://localhost:4200
   Access-Control-Allow-Headers: authorization,content-type,x-studenttoken,x-teacherid
   Access-Control-Allow-Methods: GET,POST,PUT,DELETE,OPTIONS
   Access-Control-Allow-Credentials: true
   ```

5. **Test actual request with valid token:**
   ```powershell
   curl -i http://localhost:5000/api/admin/subjects `
     -H "Origin: http://localhost:4200" `
     -H "Authorization: Bearer <valid-jwt-token>"
   ```
   Expected: 200 OK with subject list OR 401 if token invalid

6. **Test from Angular app:**
   - Open http://localhost:4200 in browser
   - Open DevTools Network tab
   - Navigate to admin dashboard or trigger API call
   - Verify OPTIONS preflight response shows CORS headers
   - Verify actual GET/POST request succeeds or returns proper 401

Troubleshooting:

**Still seeing "No 'Access-Control-Allow-Origin' header":**
- Confirm API is running updated binary (restart if needed)
- Check that Startup.cs contains app.UseCors(...) call
- Verify origin http://localhost:4200 is in CorsPolicy.Origins
- Check Fiddler or curl to see actual response headers

**401 Unauthorized on authenticated endpoints:**
- Ensure frontend sends Authorization: Bearer <token> header
- Verify token is valid JWT signed with server secret
- TokenAuthorizeAttribute expects header format: "Bearer <token>"
- Check TokenService.ExtractClaims() logic in Application/Services/TokenService.cs

**"Either the parameter @objname is ambiguous" SQL error:**
- This indicates stale LocalDB database from previous runs
- Delete and recreate: Use SQL Server Management Studio to drop 'TrackMyGrade' database
- Or run EF migration fresh: delete bin\ folder and rebuild
- API will auto-create fresh schema on startup

**CORS working but requests still failing:**
- Check browser console for other errors (Content-Security-Policy violations, etc.)
- SecurityHeadersMiddleware adds CSP header with connect-src 'self' http://localhost:5000 - this is correct
- If CSP is too strict, adjust in SecurityHeadersMiddleware.cs

Related Files Modified:
- TrackMyGradeAPI/TrackMyGradeAPI.csproj
- TrackMyGradeAPI/Startup.cs
- TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs
- TrackMyGradeAPI/Migrations/Configuration.cs

Related Files NOT Modified (existing CORS config):
- TrackMyGradeAPI/WebApiConfig.cs (EnableCorsAttribute still present, no change needed)

Browser Compatibility:
- All modern browsers support CORS (Chrome, Firefox, Safari, Edge)
- IE11 requires careful CORS configuration but should work
- Test on actual deployed browser, not just Postman (Postman ignores CORS)

Frontend Implications:
- Angular HttpClient automatically sends Authorization header if interceptor configured
- Ensure token is stored in localStorage with correct key (e.g., 'adminToken')
- Verify HttpInterceptor adds Authorization: Bearer <token> to outgoing requests
- Check that API endpoints being called are in correct CORS origin path (http://localhost:5000/api/...)

Production Deployment:
- Replace 'http://localhost:4200' with actual frontend domain
- Replace '*' with specific allowed headers, not all (current config uses '*' but can tighten)
- Set SupportsCredentials = false if not sending credentials
- Use HTTPS in production, update config to https://yourdomain.com

Updated: 2026-05-25
Version: 2.0 (with database constraint fixes)

