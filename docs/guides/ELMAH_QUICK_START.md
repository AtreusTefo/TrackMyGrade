# ELMAH Quick Start Guide

## Start the API

```powershell
cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
.\bin\TrackMyGradeAPI.exe
```

Wait for:
```
TrackMyGrade API started successfully
Listening on: http://localhost:5000/
```

---

## Access Logs - Method 1: API Endpoints

### Get Recent Errors (Quick Overview)

**Last 50 errors:**
```
http://localhost:5000/api/diagnostics/elmah-logs
```

**Last 20 errors:**
```
http://localhost:5000/api/diagnostics/elmah-logs?count=20
```

**Get specific error details:**
```
http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001
```

**Check ELMAH status:**
```
http://localhost:5000/api/diagnostics/elmah-status
```

---

## Access Logs - Method 2: File System

### Open Error Folder

```
C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\
```

### View Error File

1. Find file: `error-20260601-001.xml`
2. Right-click → Open with → Notepad
3. View XML with stack trace

---

## PowerShell Commands

### Get last 50 errors
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs"
```

### Get last 20 errors
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs?count=20"
```

### Get specific error
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001"
```

### Check ELMAH config
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-status"
```

### Search errors for specific user
```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "teacher@school.com"
```

### Count total errors
```powershell
(Get-ChildItem "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\error-*.xml").Count
```

---

## Error File Format

### Location
```
App_Data\errors\error-YYYYMMDD-###.xml
```

### Example File: error-20260601-001.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<error time="2026-06-01T11:59:45.0000000Z" 
		host="DEVELOPER-03" 
		type="System.DivideByZeroException" 
		message="Attempted to divide by zero." 
		source="/api/students/divide" 
		user="teacher@school.com" 
		statusCode="500">
  <detail>
	[Full stack trace here]
  </detail>
</error>
```

---

## Data Flow

```
1. Error occurs in API
		↓
2. ElmahExceptionLogger catches it
		↓
3. ErrorLoggingConfig.LogError(exception)
		↓
4. Error written to App_Data\errors\error-*.xml
		↓
5. Access via:
   - API: GET /api/diagnostics/elmah-logs
   - Files: App_Data\errors\error-*.xml
```

---

## Summary Table

| Task | API Method | File Method |
|------|-----------|------------|
| View recent errors | `GET /api/diagnostics/elmah-logs` | Open `App_Data\errors\` folder |
| See specific error | `GET /api/diagnostics/elmah-logs/{id}` | Right-click file → Open with Notepad |
| Search errors | Use `?count=100` then filter | Use PowerShell Select-String |
| Monitor live | Keep browser tab open | Keep folder open |
| Get stack trace | In API response | In `<detail>` XML element |

---

## Troubleshooting

**Endpoint returns 404?**
- Rebuild: `msbuild TrackMyGradeAPI.csproj`
- Restart API

**No error files appearing?**
- Check folder exists: `C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\`
- Create if missing: `New-Item -ItemType Directory -Path "...\App_Data\errors" -Force`

**API won't start?**
- Check App.config has valid XML
- Check file permissions on App_Data folder

---

## Key Endpoints

```
/api/diagnostics/elmah-logs                 - List recent errors
/api/diagnostics/elmah-logs?count=20        - List specific count
/api/diagnostics/elmah-logs/{errorId}       - Get error details
/api/diagnostics/elmah-status               - Check configuration
/api/diagnostics/health                     - Health check
```

---

**Full documentation:** See `docs/guides/ELMAH_IMPLEMENTATION_AND_USAGE.md`
