# ELMAH Implementation and Usage Guide

## Overview

ELMAH (Error Logging Modules and Handlers) is an application-wide error logging framework integrated into TrackMyGrade API. This document describes the complete implementation and provides step-by-step instructions for accessing error logs using both programmatic (API) and file-based methods.

**Current Status:** Fully implemented with persistent file-based storage and diagnostic API endpoints.

---

## Implementation Summary

### Date Implemented
2026-06-01

### Configuration Changes

#### File: `TrackMyGradeAPI/App.config`
**Before (In-Memory Storage):**
```xml
<elmah>
  <security allowRemoteAccess="0"/>
  <errorLog type="Elmah.MemoryErrorLog, Elmah" size="500"/>
</elmah>
```

**After (File-Based Persistent Storage):**
```xml
<elmah>
  <security allowRemoteAccess="0"/>
  <errorLog type="Elmah.XmlFileErrorLog, Elmah" logPath="App_Data\errors"/>
</elmah>
```

**Key Changes:**
- Changed from `Elmah.MemoryErrorLog` to `Elmah.XmlFileErrorLog`
- Removed `size="500"` limit (file-based storage is unlimited)
- Added `logPath="App_Data\errors"` to specify persistent storage location
- Maintains `allowRemoteAccess="0"` for security (no remote elmah.axd viewer needed)

### Directory Structure

**Created Folder:**
```
TrackMyGradeAPI/
  └── App_Data/
	  └── errors/
		  ├── error-20260601-001.xml
		  ├── error-20260601-002.xml
		  └── [error logs persist here]
```

**Purpose:** Stores all ELMAH error logs as individual XML files with timestamps.

### Code Changes

#### File: `TrackMyGradeAPI/Presentation/Controllers/DiagnosticsController.cs` (New)

**Created new controller with 4 diagnostic endpoints:**

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/diagnostics/elmah-logs` | GET | Retrieve recent error logs (default: 50, max: 1000) |
| `/api/diagnostics/elmah-logs/{errorId}` | GET | Get specific error with full stack trace |
| `/api/diagnostics/elmah-status` | GET | Check ELMAH configuration and status |
| `/api/diagnostics/health` | GET | API health check |

**Implementation Details:**

1. **GetElmahLogs Endpoint**
   - Retrieves up to `count` most recent errors from ELMAH
   - Query parameter: `count` (default: 50, max: 1000)
   - Returns JSON with error summaries (message, type, timestamp, URL, user)
   - Safe pagination to prevent data overload

2. **GetElmahLogById Endpoint**
   - Retrieves specific error by ID
   - Includes full stack trace details
   - Returns 404 if error not found
   - Useful for deep-diving into individual errors

3. **GetElmahStatus Endpoint**
   - Returns ELMAH configuration information
   - Displays current storage type, location, and access methods
   - Verifies system is properly configured

4. **GetHealth Endpoint**
   - Simple health check
   - Returns 200 OK with timestamp

**Code Architecture:**

```csharp
public class DiagnosticsController : ApiController
{
	// Uses Elmah.ErrorLog.GetDefault(null) to access configured error log
	// Handles exceptions gracefully with InternalServerError responses
	// All methods fully documented with XML comments
	// Response format: { success: bool, data: [...], timestamp: DateTime }
}
```

### NuGet Dependencies

**Unchanged - Already installed:**
- `Elmah 1.2.2` - Core error logging framework
- `Elmah.XmlFileErrorLog` included in Elmah package (version 1.2.2+)

---

## How Errors Are Captured

### Automatic Capture (Web API Pipeline)

**Flow:**
1. Unhandled exception occurs in Web API controller
2. `ElmahExceptionLogger` (registered in `WebApiConfig.cs`) catches it
3. Exception passed to `ErrorLoggingConfig.LogError(exception)`
4. Error logged to ELMAH via `ErrorLog.GetDefault(null).Log(new Error(exception))`
5. Error written to `App_Data\errors\error-TIMESTAMP.xml`

**Code in WebApiConfig.cs:**
```csharp
config.Services.Replace(typeof(IExceptionHandler), new ElmahExceptionHandler());
config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());
```

### Manual Capture (Service Layer)

**Usage in Services:**
```csharp
using TrackMyGradeAPI.Logging;

try 
{
	// some code
}
catch (Exception ex)
{
	// Log with just exception
	ErrorLoggingConfig.LogError(ex);

	// OR log with custom message
	ErrorLoggingConfig.LogErrorWithMessage("Failed to create student", ex);
}
```

### Error Information Captured

For each error, ELMAH captures:
- **Message** - Exception message
- **Type** - Full exception type (e.g., `System.NullReferenceException`)
- **Time (UTC)** - Exact timestamp when error occurred
- **Host** - Server hostname
- **URL** - Request URL that caused error
- **User** - Authenticated user (if available)
- **Status Code** - HTTP status code (usually 500)
- **Stack Trace** - Full call stack for debugging
- **Request Details** - Headers, query parameters, form data

---

## How to Access ELMAH Logs

### Prerequisites

1. **API Running:**
   ```powershell
   cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
   .\bin\TrackMyGradeAPI.exe
   ```

   Expected output:
   ```
   [Seeding] Constraint management delegated to migrations.
   [2026-06-01T12:00:00.0000000Z] ELMAH initialized with config from App.config
   =========================================
	 TrackMyGrade API started successfully
	 Listening on: http://localhost:5000/
   =========================================
   Press Enter to stop...
   ```

2. **API Accessible:** Endpoints available at `http://localhost:5000/`

---

## Method 1: API Endpoints (Programmatic Access)

### 1.1 Get Recent Error Logs

**Endpoint:** `GET /api/diagnostics/elmah-logs`

**URL:**
```
http://localhost:5000/api/diagnostics/elmah-logs
```

**Query Parameters:**
- `count` (optional, default: 50, max: 1000) - Number of errors to retrieve

**Examples:**

**Get last 50 errors (default):**
```
http://localhost:5000/api/diagnostics/elmah-logs
```

**Get last 20 errors:**
```
http://localhost:5000/api/diagnostics/elmah-logs?count=20
```

**Get last 100 errors:**
```
http://localhost:5000/api/diagnostics/elmah-logs?count=100
```

**Response Format:**
```json
{
  "success": true,
  "timestamp": "2026-06-01T12:00:30Z",
  "totalRetrieved": 3,
  "errors": [
	{
	  "id": "error-20260601-003",
	  "message": "Attempted to divide by zero",
	  "type": "System.DivideByZeroException",
	  "timeUtc": "2026-06-01T11:59:45Z",
	  "statusCode": 500,
	  "hostname": "DEVELOPER-03",
	  "url": "/api/students/divide",
	  "user": "teacher@school.com"
	},
	{
	  "id": "error-20260601-002",
	  "message": "Object reference not set to an instance of an object",
	  "type": "System.NullReferenceException",
	  "timeUtc": "2026-06-01T11:55:12Z",
	  "statusCode": 500,
	  "hostname": "DEVELOPER-03",
	  "url": "/api/classes/123",
	  "user": "admin@school.com"
	}
  ],
  "storageLocation": "App_Data\\errors\\ (XML file-based persistent storage)"
}
```

**Using PowerShell:**
```powershell
# PowerShell - Using Invoke-WebRequest
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/diagnostics/elmah-logs" -Method GET
$response.Content | ConvertFrom-Json | ConvertTo-Json

# PowerShell - Using curl (Windows 10+)
curl "http://localhost:5000/api/diagnostics/elmah-logs"

# Get last 20 errors
curl "http://localhost:5000/api/diagnostics/elmah-logs?count=20"
```

**Using Browser:**
1. Open browser
2. Navigate to: `http://localhost:5000/api/diagnostics/elmah-logs`
3. Response displays as JSON in browser

---

### 1.2 Get Specific Error Details

**Endpoint:** `GET /api/diagnostics/elmah-logs/{errorId}`

**URL Format:**
```
http://localhost:5000/api/diagnostics/elmah-logs/{errorId}
```

**Example:**
```
http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001
```

**Response Format:**
```json
{
  "success": true,
  "timestamp": "2026-06-01T12:01:00Z",
  "error": {
	"id": "error-20260601-001",
	"message": "Attempted to divide by zero",
	"type": "System.DivideByZeroException",
	"timeUtc": "2026-06-01T11:59:45Z",
	"statusCode": 500,
	"hostname": "DEVELOPER-03",
	"url": "/api/students/divide",
	"user": "teacher@school.com",
	"stackTrace": "at TrackMyGradeAPI.Application.Services.StudentService.CalculateGPA(int studentId, int total) in C:\\Users\\Developer.03\\Desktop\\TrackMyGrade\\TrackMyGradeAPI\\Application\\Services\\StudentService.cs:line 145\n   at TrackMyGradeAPI.Presentation.Controllers.StudentsController.GetStudentGPA(int id) in C:\\Users\\Developer.03\\Desktop\\TrackMyGrade\\TrackMyGradeAPI\\Presentation\\Controllers\\StudentsController.cs:line 78\n   at lambda_method(Closure , Object , Object[] )\n   at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.Execute(HttpControllerContext controllerContext, IDictionary`2 parameters)\n   --- End of stack trace ---"
  }
}
```

**Using PowerShell:**
```powershell
# Get specific error details
curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001"

# Get details and format nicely
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001" -Method GET
($response.Content | ConvertFrom-Json).error.stackTrace | Write-Host
```

**Using Browser:**
1. Open browser
2. Navigate to: `http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001`
3. View full error details including stack trace

---

### 1.3 Check ELMAH Configuration Status

**Endpoint:** `GET /api/diagnostics/elmah-status`

**URL:**
```
http://localhost:5000/api/diagnostics/elmah-status
```

**Response Format:**
```json
{
  "success": true,
  "timestamp": "2026-06-01T12:02:00Z",
  "elmahStatus": {
	"loggerType": "XmlFileErrorLog",
	"storageType": "XmlFileErrorLog (File-based persistent storage)",
	"logPath": "App_Data\\errors\\",
	"remoteAccessAllowed": false,
	"note": "Logs persist across API restarts. Errors stored as XML files.",
	"accessMethods": [
	  "GET /api/diagnostics/elmah-logs (list recent errors)",
	  "GET /api/diagnostics/elmah-logs/{errorId} (get error details)",
	  "Direct file access: App_Data\\errors\\error-*.xml"
	]
  }
}
```

**Using PowerShell:**
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-status"
```

**Purpose:** Verify ELMAH is correctly configured and using file-based persistent storage.

---

### 1.4 Health Check

**Endpoint:** `GET /api/diagnostics/health`

**URL:**
```
http://localhost:5000/api/diagnostics/health
```

**Response Format:**
```json
{
  "status": "healthy",
  "timestamp": "2026-06-01T12:02:30Z",
  "apiVersion": "1.0"
}
```

---

## Method 2: Direct File Access (File System)

### 2.1 Browse Error Log Files

**Location:**
```
C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\
```

**Steps:**
1. Open Windows Explorer
2. Navigate to above path
3. You will see error files like:
   - `error-20260601-001.xml`
   - `error-20260601-002.xml`
   - `error-20260601-003.xml`

**File Naming Convention:** `error-YYYYMMDD-###.xml`
- `YYYYMMDD` - Date in format Year/Month/Day
- `###` - Sequential number for that day

---

### 2.2 Open and View Error Details

**Step 1: Locate Error File**
- Navigate to `App_Data\errors\` folder
- Find error file (e.g., `error-20260601-001.xml`)

**Step 2: Open in Text Editor**
- Right-click on file
- Select "Open with"
- Choose "Notepad" (or any text editor)

**Example Content:**
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
	at TrackMyGradeAPI.Application.Services.StudentService.CalculateGPA(int studentId, int total) in C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\Application\Services\StudentService.cs:line 145
	at TrackMyGradeAPI.Presentation.Controllers.StudentsController.GetStudentGPA(int id) in C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\Presentation\Controllers\StudentsController.cs:line 78
	at lambda_method(Closure , Object , Object[] )
	at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.Execute(HttpControllerContext controllerContext, IDictionary`2 parameters)
  </detail>
</error>
```

**Parsing XML Structure:**

| Attribute | Meaning | Example |
|-----------|---------|---------|
| `time` | UTC timestamp of error | `2026-06-01T11:59:45Z` |
| `host` | Server hostname | `DEVELOPER-03` |
| `type` | Exception type | `System.DivideByZeroException` |
| `message` | Error message | `Attempted to divide by zero.` |
| `source` | Request URL | `/api/students/divide` |
| `user` | Authenticated user | `teacher@school.com` |
| `statusCode` | HTTP status | `500` |
| `<detail>` | Stack trace | Full call stack for debugging |

---

### 2.3 Search Error Files

**Using File Explorer Search:**
1. Open `App_Data\errors\` folder
2. Press `Ctrl+F`
3. Search for error details:
   - Error date: `20260601`
   - Exception type: `NullReference`
   - User: `teacher@school.com`

**Using PowerShell Search:**
```powershell
# Find all errors from specific date
Get-ChildItem "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\error-20260601-*.xml"

# Find errors for specific user
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "teacher@school.com"

# Find specific exception type
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "NullReferenceException"

# Count total errors
(Get-ChildItem "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\error-*.xml").Count
```

---

### 2.4 Monitor Error Files in Real-Time

**Using PowerShell (Watch for new errors):**
```powershell
# Watch folder for new error files
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"
$watcher.Filter = "error-*.xml"
$watcher.IncludeSubdirectories = $false

Register-ObjectEvent -InputObject $watcher -EventName "Created" -Action {
	Write-Host "New error logged: $($Event.SourceEventArgs.Name)"
}

# Press Ctrl+C to stop watching
```

**Using File Explorer:**
1. Navigate to `App_Data\errors\`
2. Keep window open
3. Run API and trigger errors
4. New files appear in real-time

---

## Complete Workflow Examples

### Example 1: Debug a Specific Error

**Scenario:** API returned a 500 error; you want to investigate.

**Steps:**

1. **Check recent errors via API:**
   ```powershell
   curl "http://localhost:5000/api/diagnostics/elmah-logs?count=5"
   ```

2. **Identify the error ID** (e.g., `error-20260601-005`)

3. **Get full details including stack trace:**
   ```powershell
   curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-005"
   ```

4. **Review stack trace** to identify problematic file and line number

5. **Open source code file** at indicated line to fix bug

---

### Example 2: Find All Errors from Specific User

**Scenario:** Teacher reports getting errors; need to see all their errors.

**Method 1 (API):**
```powershell
# Retrieve last 100 errors and filter in PowerShell
$errors = (Invoke-WebRequest -Uri "http://localhost:5000/api/diagnostics/elmah-logs?count=100" -Method GET).Content | ConvertFrom-Json
$errors.errors | Where-Object { $_.user -eq "teacher@school.com" }
```

**Method 2 (File System):**
```powershell
# Search all error files for specific user
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "teacher@school.com" | ForEach-Object { $_.FileName }
```

---

### Example 3: Monitor Errors During Testing

**Scenario:** Running tests and want to see errors as they occur.

**Steps:**

1. **Open file explorer** to `App_Data\errors\`
2. **Open PowerShell** and run API:
   ```powershell
   .\bin\TrackMyGradeAPI.exe
   ```
3. **Watch errors appear** in file explorer in real-time
4. **Double-click error file** to view details

---

## Troubleshooting

### Problem: No Errors Appearing

**Cause 1: App.config not updated**
- **Solution:** Verify `App.config` has `<errorLog type="Elmah.XmlFileErrorLog, Elmah" logPath="App_Data\errors"/>`

**Cause 2: App_Data\errors folder missing**
- **Solution:** Create folder manually:
  ```powershell
  New-Item -ItemType Directory -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors" -Force
  ```

**Cause 3: Folder permission issues**
- **Solution:** Ensure folder has write permissions for current user:
  ```powershell
  # Run PowerShell as Administrator
  icacls "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors" /grant:r "$(whoami):(OI)(CI)F"
  ```

---

### Problem: Endpoints returning 404

**Cause:** DiagnosticsController not loaded

**Solution:**
1. Rebuild project: `msbuild TrackMyGradeAPI.csproj`
2. Restart API: `.\bin\TrackMyGradeAPI.exe`
3. Verify endpoint: `curl "http://localhost:5000/api/diagnostics/health"`

---

### Problem: Logs lost after API restart

**This should NOT happen if using file-based storage.**

**Verification:**
1. Check `App.config` uses `XmlFileErrorLog` (not `MemoryErrorLog`)
2. Check `logPath="App_Data\errors"` is correct
3. Restart API: `.\bin\TrackMyGradeAPI.exe`
4. Files should persist in `App_Data\errors\`

---

## Best Practices

### 1. Regular Monitoring
- Check ELMAH logs daily during development
- Use API endpoint for quick checks
- Use file system for archival and historical analysis

### 2. Error Prevention
- Use try-catch in service layer with `ErrorLoggingConfig.LogError(ex)`
- Add context with `ErrorLoggingConfig.LogErrorWithMessage("context", ex)`
- Review stack traces to identify root causes

### 3. File Management
- Archive old error files monthly
- Delete error files older than 6 months
- Keep recent errors for debugging

**Cleanup Script:**
```powershell
# Delete error files older than 30 days
$path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"
$limit = (Get-Date).AddDays(-30)
Get-ChildItem -Path $path -Filter "error-*.xml" | Where-Object { $_.CreationTime -lt $limit } | Remove-Item -Force
```

### 4. Permissions
- Ensure API process has write access to `App_Data\errors\`
- Use IIS application pool identity in production
- Test permissions after deployment

---

## Files Modified/Created

| File | Type | Changes |
|------|------|---------|
| `TrackMyGradeAPI/App.config` | Modified | Changed error logging from MemoryErrorLog to XmlFileErrorLog |
| `TrackMyGradeAPI/Presentation/Controllers/DiagnosticsController.cs` | Created | Added 4 diagnostic endpoints for accessing ELMAH logs |
| `TrackMyGradeAPI/App_Data/errors/` | Created | Directory for persistent XML error logs |

---

## Summary

| Aspect | Details |
|--------|---------|
| **Storage Type** | XML File-based (persistent) |
| **Storage Location** | `App_Data\errors\` |
| **Retention** | Unlimited (until manually deleted) |
| **Access Methods** | API endpoints + File system |
| **Error Limit** | Unlimited (file-based) |
| **Survives Restart** | Yes |
| **Configuration** | `App.config` (XmlFileErrorLog) |

---

## Quick Reference Cards

### API Endpoints Quick Reference

```
GET /api/diagnostics/elmah-logs                  → Get last 50 errors
GET /api/diagnostics/elmah-logs?count=20         → Get last 20 errors
GET /api/diagnostics/elmah-logs/{errorId}        → Get error details
GET /api/diagnostics/elmah-status                → Check ELMAH config
GET /api/diagnostics/health                      → API health check
```

### File System Quick Reference

```
Location:  C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\
Files:     error-YYYYMMDD-###.xml
Example:   error-20260601-001.xml
Format:    XML with attributes and stack trace
```

---

**Document Version:** 1.0  
**Last Updated:** 2026-06-01  
**Status:** Complete and Verified
