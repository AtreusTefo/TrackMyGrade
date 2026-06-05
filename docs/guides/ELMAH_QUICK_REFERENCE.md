# ELMAH Quick Reference Card

## Using ELMAH in Code

### Basic Exception Logging
```csharp
using TrackMyGradeAPI.Logging;

try { /* code */ }
catch (Exception ex)
{
	ErrorLoggingConfig.LogError(ex);
	return BadRequest(ex.Message);
}
```

### With Custom Message
```csharp
ErrorLoggingConfig.LogErrorWithMessage("Failed to create teacher", ex);
```

### With User & Request Context
```csharp
string userId = Request.GetUserId().ToString();
string uri = Request.RequestUri?.ToString();
ErrorLoggingConfig.LogErrorWithContext(ex, userId, uri);
```

### With Full HTTP Context
```csharp
ErrorLoggingConfig.LogErrorWithFullContext(
	ex,
	Request.GetUserId().ToString(),
	Request.RequestUri?.ToString(),
	Request.Method.ToString(),
	Request.Content?.Headers?.ContentType?.ToString(),
	new Dictionary<string, object> { { "Custom", "Data" } }
);
```

### Validation Errors
```csharp
ErrorLoggingConfig.LogValidationError(ex, "Student", "Email", "invalid@");
```

### Data Integrity Errors
```csharp
ErrorLoggingConfig.LogDataIntegrityError(ex, "UPDATE", "Grade", 123);
```

---

## ELMAH Architecture

### Three Exception Capture Points

```
┌─────────────────────────────────────────────────────────────┐
│  Request Arrives                                             │
├─────────────────────────────────────────────────────────────┤
│  1. ErrorHandlingMiddleware (catches all middleware errors)  │
│         ↓ Exception?                                         │
│         LogErrorWithFullContext() → ELMAH                    │
├─────────────────────────────────────────────────────────────┤
│  2. Web API Pipeline                                         │
│     ElmahExceptionLogger (catches API exceptions)            │
│         ↓ Exception?                                         │
│         LogErrorWithMessage() → ELMAH                        │
├─────────────────────────────────────────────────────────────┤
│  3. Controller/Service Layer                                 │
│     Explicit LogError() calls                                │
│         ↓ Exception?                                         │
│         LogError() → ELMAH                                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Configuration

### Development (Current)

```xml
<!-- App.config -->
<elmah>
  <security allowRemoteAccess="0"/>
  <errorLog type="Elmah.MemoryErrorLog, Elmah" size="500"/>
</elmah>
```

### Production (Recommended)

```xml
<!-- App.config -->
<elmah>
  <security allowRemoteAccess="0"/>
  <errorLog type="Elmah.SqlErrorLog, Elmah" 
			connectionStringName="DefaultConnection"/>
  <errorMail from="error@domain.com" 
			 to="admin@domain.com"
			 smtpServer="smtp.yourdomain.com"
			 useSsl="true"/>
</elmah>
```

### Testing (XML Files)

```xml
<!-- App.config -->
<elmah>
  <security allowRemoteAccess="0"/>
  <errorLog type="Elmah.XmlFileErrorLog, Elmah" 
			logPath="App_Data\errors"/>
</elmah>
```

---

## Testing Error Logging

### Health Check
```bash
curl http://localhost:5000/api/test/health
# Returns: 200 OK with status message
```

### Basic Error
```bash
curl http://localhost:5000/api/test/throw-error
# Returns: 500 error, logged to ELMAH
```

### Error with Context
```bash
curl http://localhost:5000/api/test/throw-error-with-context
# Returns: 500 error with full context logged
```

### Data Integrity Error
```bash
curl http://localhost:5000/api/test/throw-data-integrity-error
# Returns: 500 error with operation context logged
```

---

## Accessing Error Logs

### Programmatic (C#)
```csharp
var errorLog = ErrorLog.GetDefault(null);
var errors = errorLog.GetErrors(0, 10);
foreach (var error in errors)
{
	Console.WriteLine($"[{error.Time}] {error.Exception.Type}: {error.Message}");
}
```

### File-Based (XmlFileErrorLog)
```powershell
# View error files
Get-ChildItem -Path "App_Data\errors" -Filter "*.xml"

# View latest error
Get-Content -Path "App_Data\errors\latest_error.xml"
```

### SQL Server (SqlErrorLog)
```sql
SELECT * FROM ELMAH_Error
ORDER BY TimeUtc DESC
LIMIT 10;
```

---

## Key Files

| File | Purpose |
|------|---------|
| `ErrorLoggingConfig.cs` | Logging API |
| `ElmahExceptionHandler.cs` | Web API handlers |
| `ErrorHandlingMiddleware.cs` | OWIN middleware |
| `App.config` | Configuration |
| `ELMAH_SETUP.md` | Complete reference |
| `ELMAH_TESTING_GUIDE.md` | Testing guide |

---

## Common Tasks

### Add Error Logging to New Controller

```csharp
using TrackMyGradeAPI.Logging;

[HttpGet, Route("endpoint")]
public IHttpActionResult GetData()
{
	try
	{
		// Your code here
		return Ok(data);
	}
	catch (Exception ex)
	{
		ErrorLoggingConfig.LogError(ex);  // Add this line
		return BadRequest(ex.Message);
	}
}
```

### Change Storage Backend

1. Update `App.config`:
   ```xml
   <errorLog type="Elmah.SqlErrorLog, Elmah" 
			 connectionStringName="DefaultConnection"/>
   ```

2. Create database tables (if using SQL Server)
3. Restart application

### View Recent Errors (Production)

```csharp
var errorLog = ErrorLog.GetDefault(null);
var errors = errorLog.GetErrors(0, 100);  // Last 100 errors
var recentErrors = errors.Where(e => e.Time > DateTime.UtcNow.AddHours(-1));
```

### Configure Email Alerts

```xml
<errorMail from="alert@company.com"
		   to="ops@company.com"
		   subject="TrackMyGrade API Error"
		   smtpServer="smtp.office365.com"
		   smtpPort="587"
		   useSsl="true"
		   userName="alert@company.com"
		   password="***"/>
```

---

## Troubleshooting Quick Links

| Issue | Solution |
|-------|----------|
| Errors not logging | Check App.config, verify ErrorLoggingConfig called |
| Memory log loses errors | Switch to SqlErrorLog or XmlFileErrorLog |
| `/elmah.axd` not working | Normal for self-hosted OWIN (IIS only) |
| Email not sending | Verify SMTP config, check firewall rules |
| Performance slow | Check logging frequency, consider async logging |
| Storage full | Archive old errors, increase capacity, or cleanup |

---

## Before Production

1. ✅ Remove TestController.cs or restrict with `[TokenAuthorize("Admin")]`
2. ✅ Switch errorLog to SqlErrorLog in App.config
3. ✅ Configure email notifications
4. ✅ Test all logging in staging environment
5. ✅ Review and approve alert recipients
6. ✅ Document error response procedures
7. ✅ Train operations team
8. ✅ Set up monitoring dashboard

---

## Support Resources

- **Full Guide:** `ELMAH_SETUP.md`
- **Testing:** `ELMAH_TESTING_GUIDE.md`
- **Analysis:** `ELMAH_INTEGRATION_REPORT.md`
- **Official ELMAH Docs:** https://elmah.github.io/
- **Troubleshooting:** See ELMAH_SETUP.md section "Troubleshooting"

---

**Last Updated:** May 18, 2026  
**Version:** 1.0  
**Status:** ✅ Ready for Production
