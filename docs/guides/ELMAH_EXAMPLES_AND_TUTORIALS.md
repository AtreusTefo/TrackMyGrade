# ELMAH Access Examples and Step-by-Step Tutorials

## Overview

This document provides practical, step-by-step tutorials for accessing ELMAH logs using both methods with real examples and expected output.

---

## Table of Contents

1. [Method 1: API Endpoints](#method-1-api-endpoints)
2. [Method 2: File System](#method-2-file-system)
3. [Comparing Both Methods](#comparing-both-methods)
4. [Advanced Usage](#advanced-usage)

---

# Method 1: API Endpoints

## Setup

### Prerequisites Check

**Verify API is running:**

```powershell
# In PowerShell, check if API is listening on port 5000
netstat -ano | findstr :5000

# Expected output:
# TCP    127.0.0.1:5000    0.0.0.0:0    LISTENING    12345
```

**Verify endpoint is accessible:**

```powershell
curl -i "http://localhost:5000/api/diagnostics/health"

# Expected output:
# HTTP/1.1 200 OK
# {
#   "status": "healthy",
#   "timestamp": "2026-06-01T12:00:00Z",
#   "apiVersion": "1.0"
# }
```

---

## Tutorial 1: View Last 50 Errors

### Using Browser

**Steps:**

1. Open browser (Chrome, Firefox, Edge)
2. Navigate to: `http://localhost:5000/api/diagnostics/elmah-logs`
3. Browser will display formatted JSON response

**Expected Output (Browser):**

The browser will display something like:

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
	},
	{
	  "id": "error-20260601-001",
	  "message": "Index was out of range. Must be non-negative and less than the size of the collection.",
	  "type": "System.ArgumentOutOfRangeException",
	  "timeUtc": "2026-06-01T11:50:00Z",
	  "statusCode": 500,
	  "hostname": "DEVELOPER-03",
	  "url": "/api/grades/list",
	  "user": "teacher@school.com"
	}
  ],
  "storageLocation": "App_Data\\errors\\ (XML file-based persistent storage)"
}
```

**What Each Field Means:**

| Field | Meaning | Example |
|-------|---------|---------|
| `id` | Unique error identifier | `error-20260601-003` |
| `message` | Human-readable error description | `Attempted to divide by zero` |
| `type` | .NET exception type | `System.DivideByZeroException` |
| `timeUtc` | When error occurred (UTC) | `2026-06-01T11:59:45Z` |
| `statusCode` | HTTP response code | `500` (Server Error) |
| `hostname` | Server name | `DEVELOPER-03` |
| `url` | Request that caused error | `/api/students/divide` |
| `user` | Who was logged in | `teacher@school.com` |

---

### Using PowerShell (curl)

**Command:**

```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs"
```

**Output:**

Same JSON as above, displayed in terminal.

---

### Using PowerShell (Invoke-WebRequest with formatting)

**Command:**

```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/diagnostics/elmah-logs" -Method GET
$response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

**Output:**

Formatted JSON displayed in PowerShell.

---

## Tutorial 2: Get Last N Errors (Custom Count)

### Get Last 10 Errors

**URL:**
```
http://localhost:5000/api/diagnostics/elmah-logs?count=10
```

**PowerShell:**
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs?count=10"
```

**Expected:** Returns 10 most recent errors (or fewer if less than 10 exist).

---

### Get Last 100 Errors

**URL:**
```
http://localhost:5000/api/diagnostics/elmah-logs?count=100
```

**PowerShell:**
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs?count=100"
```

**Expected:** Returns up to 100 most recent errors.

---

## Tutorial 3: Get Specific Error with Stack Trace

### Step 1: Get Error ID

From the list of recent errors, identify the error ID you want to investigate.

Example: `error-20260601-003`

### Step 2: Fetch Full Error Details

**URL:**
```
http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-003
```

**PowerShell:**
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-003"
```

### Step 3: Review Stack Trace

**Expected Output:**

```json
{
  "success": true,
  "timestamp": "2026-06-01T12:01:30Z",
  "error": {
	"id": "error-20260601-003",
	"message": "Attempted to divide by zero",
	"type": "System.DivideByZeroException",
	"timeUtc": "2026-06-01T11:59:45Z",
	"statusCode": 500,
	"hostname": "DEVELOPER-03",
	"url": "/api/students/divide",
	"user": "teacher@school.com",
	"stackTrace": "at TrackMyGradeAPI.Application.Services.StudentService.CalculateGPA(int studentId, int total) in C:\\Users\\Developer.03\\Desktop\\TrackMyGrade\\TrackMyGradeAPI\\Application\\Services\\StudentService.cs:line 145\n   at TrackMyGradeAPI.Presentation.Controllers.StudentsController.GetStudentGPA(int id) in C:\\Users\\Developer.03\\Desktop\\TrackMyGrade\\TrackMyGradeAPI\\Presentation\\Controllers\\StudentsController.cs:line 78\n   at lambda_method(Closure , Object , Object[] )\n   at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.Execute(HttpControllerContext controllerContext, IDictionary`2 parameters)"
  }
}
```

### Step 4: Analyze Stack Trace

**Parsing the stack trace:**

```
Line 145 in StudentService.cs:
	var gpa = gradeSum / totalGrades;  // ERROR: totalGrades is 0

Line 78 in StudentsController.cs:
	var gpa = _studentService.CalculateGPA(studentId, 0);  // Passed 0 as totalGrades

ROOT CAUSE: Calling CalculateGPA with totalGrades=0 causes division by zero
```

**Action Items:**
1. Add validation: `if (totalGrades == 0) throw new ArgumentException(...)`
2. Or use safe division: `if (totalGrades > 0) gpa = gradeSum / totalGrades; else gpa = 0;`

---

## Tutorial 4: Check ELMAH Configuration

**Purpose:** Verify ELMAH is correctly configured with file-based storage.

**URL:**
```
http://localhost:5000/api/diagnostics/elmah-status
```

**PowerShell:**
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-status"
```

**Expected Output:**

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

**Verification Checklist:**

- ✓ `loggerType` = `XmlFileErrorLog` (not MemoryErrorLog)
- ✓ `logPath` = `App_Data\errors\`
- ✓ `remoteAccessAllowed` = `false` (secure)
- ✓ All 3 access methods listed

---

# Method 2: File System

## Setup

### Locate Error Folder

**Path:**
```
C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\
```

**Steps:**

1. Open Windows Explorer
2. Paste path in address bar
3. Press Enter
4. Folder opens showing error XML files

---

## Tutorial 5: Browse Error Files

### Step 1: Open App_Data\errors Folder

Windows Explorer shows files like:

```
📁 errors
  ├── error-20260601-001.xml
  ├── error-20260601-002.xml
  ├── error-20260601-003.xml
  ├── error-20260602-001.xml
  └── error-20260602-002.xml
```

### Step 2: Understand File Naming

**Format:** `error-YYYYMMDD-###.xml`

| Part | Meaning | Example |
|------|---------|---------|
| `error` | Fixed prefix | Always `error` |
| `YYYYMMDD` | Date (Year/Month/Day) | `20260601` = June 1, 2026 |
| `###` | Sequential number | `001`, `002`, `003` |
| `.xml` | File extension | Always `.xml` |

### Step 3: Find Recent Errors

**Method 1: Sort by Date Modified**

1. Click "Date modified" column header
2. Errors sort newest first
3. Most recent errors at top

**Method 2: Sort by Name**

1. Most recent dates appear last
2. Older errors appear first

---

## Tutorial 6: Open and View Error Details

### Step 1: Right-Click Error File

```
error-20260601-003.xml → Right-click
```

### Step 2: Select "Open with"

Choose from:
- Notepad (recommended for quick view)
- Visual Studio Code (better formatting)
- Internet Explorer (renders XML nicely)
- Any text editor

### Step 3: View Error Contents

**Example file: error-20260601-003.xml**

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
	at TrackMyGradeAPI.Application.Services.StudentService.CalculateGPA(int studentId, int total) 
	  in C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\Application\Services\StudentService.cs:line 145
	at TrackMyGradeAPI.Presentation.Controllers.StudentsController.GetStudentGPA(int id) 
	  in C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\Presentation\Controllers\StudentsController.cs:line 78
	at lambda_method(Closure , Object , Object[] )
	at System.Web.Http.Controllers.ReflectedHttpActionDescriptor.Execute(HttpControllerContext controllerContext, IDictionary`2 parameters)
  </detail>
</error>
```

### Step 4: Parse Error Information

**From XML attributes:**

| Attribute | Value |
|-----------|-------|
| `time` | `2026-06-01T11:59:45Z` - When error occurred |
| `host` | `DEVELOPER-03` - Server name |
| `type` | `System.DivideByZeroException` - Exception type |
| `message` | `Attempted to divide by zero.` - Error description |
| `source` | `/api/students/divide` - Request URL |
| `user` | `teacher@school.com` - Authenticated user |
| `statusCode` | `500` - HTTP status code |

**From `<detail>` element:**

Full stack trace showing where error occurred and call chain.

---

## Tutorial 7: Search Error Files

### Using File Explorer Search

**Steps:**

1. Open `App_Data\errors\` folder
2. Press `Ctrl + F`
3. Type search term:
   - Date: `20260601`
   - Exception type: `NullReference`
   - User: `teacher@school.com`
   - URL: `/api/students`

**Example: Find all errors from June 1st**

```
Search: 20260601
Result: error-20260601-001.xml, error-20260601-002.xml, error-20260601-003.xml
```

---

### Using PowerShell Select-String

**Find errors for specific user:**

```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "teacher@school.com"
```

**Expected Output:**

```
error-20260601-001.xml:8:user="teacher@school.com"
error-20260601-003.xml:8:user="teacher@school.com"
```

---

**Find specific exception type:**

```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "NullReferenceException"
```

**Expected Output:**

```
error-20260601-002.xml:8:type="System.NullReferenceException"
```

---

**Find errors on specific date:**

```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "2026-06-01T1[0-1]"
```

---

**Count total errors:**

```powershell
(Get-ChildItem "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\error-*.xml").Count
```

**Expected Output:**

```
5
```

---

## Tutorial 8: Monitor Errors in Real-Time

### Using File Explorer

**Steps:**

1. Open `App_Data\errors\` folder in File Explorer
2. Keep window visible
3. Run API and trigger errors
4. Watch new files appear automatically

**What you'll see:**

```
[Before API run]
error-20260601-001.xml
error-20260601-002.xml

[After running API and triggering error]
error-20260601-001.xml
error-20260601-002.xml
error-20260601-003.xml   ← NEW FILE APPEARS!
```

---

### Using PowerShell Watch

**Script:**

```powershell
$path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"

while ($true) {
	$count = (Get-ChildItem -Path $path -Filter "error-*.xml" | Measure-Object).Count
	Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Total errors: $count"
	Start-Sleep -Seconds 5
}
```

**Running:**

```powershell
PS> .\monitor-errors.ps1
[12:00:05] Total errors: 2
[12:00:10] Total errors: 2
[12:00:15] Total errors: 3    ← NEW ERROR!
[12:00:20] Total errors: 3
```

---

# Comparing Both Methods

## Quick Comparison Table

| Feature | API Method | File System |
|---------|-----------|------------|
| **Speed** | Instant, no file I/O | Depends on folder size |
| **Formatting** | Structured JSON | Raw XML |
| **Requires API Running** | Yes | No |
| **Real-time Monitoring** | Browser tab stays open | File Explorer watches folder |
| **Searching** | Limited (need custom filters) | PowerShell Select-String powerful |
| **Stack Traces** | Clean, single request | Must read full XML file |
| **Parsing** | Easy with JSON | Need XML parser |
| **Best For** | Development dashboards | Debugging, archival |
| **Scalability** | Good for many errors | Good for browsing recent |

---

## When to Use Each Method

### Use API Method When:

- ✓ Building monitoring dashboards
- ✓ Creating automated alerts
- ✓ Integrating with other systems
- ✓ Need JSON format
- ✓ Want RESTful access
- ✓ Building admin panels

### Use File System When:

- ✓ API not responding (can't use endpoints)
- ✓ Need raw XML data
- ✓ Archiving errors for compliance
- ✓ Running PowerShell scripts
- ✓ Want unlimited file retention
- ✓ Emergency debugging

---

# Advanced Usage

## Scenario 1: Export All Errors to CSV

**PowerShell Script:**

```powershell
$errors = @()
$path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"

Get-ChildItem -Path $path -Filter "error-*.xml" | ForEach-Object {
	[xml]$xml = Get-Content $_.FullName
	$error = $xml.error
	$errors += [PSCustomObject]@{
		Id = $_.Name
		Time = $error.time
		Type = $error.type
		Message = $error.message
		User = $error.user
		Url = $error.source
		StatusCode = $error.statusCode
	}
}

$errors | Export-Csv -Path "C:\Users\Developer.03\Desktop\errors-export.csv" -NoTypeInformation
Write-Host "Exported $($errors.Count) errors to errors-export.csv"
```

---

## Scenario 2: Find Errors by Date Range

**PowerShell Script:**

```powershell
$path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"
$startDate = "2026-06-01"
$endDate = "2026-06-02"

Select-String -Path "$path\*.xml" -Pattern "$startDate|$endDate" | ForEach-Object {
	"File: $($_.FileName)"
	"Match: $($_.Line)"
}
```

---

## Scenario 3: Build Error Statistics

**PowerShell Script:**

```powershell
$path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"
$errors = @()

Get-ChildItem -Path $path -Filter "error-*.xml" | ForEach-Object {
	[xml]$xml = Get-Content $_.FullName
	$errors += [PSCustomObject]@{
		Type = $xml.error.type
		User = $xml.error.user
	}
}

"Top Exception Types:"
$errors | Group-Object -Property Type | Sort-Object -Property Count -Descending | Select-Object -First 5 | Format-Table Name, Count

"Top Users with Errors:"
$errors | Group-Object -Property User | Sort-Object -Property Count -Descending | Select-Object -First 5 | Format-Table Name, Count
```

---

## Scenario 4: Clean Up Old Error Files

**PowerShell Script (DANGEROUS - USE WITH CAUTION):**

```powershell
$path = "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors"
$daysToKeep = 7  # Keep last 7 days
$cutoffDate = (Get-Date).AddDays(-$daysToKeep)

$deletedCount = 0
Get-ChildItem -Path $path -Filter "error-*.xml" | Where-Object { $_.CreationTime -lt $cutoffDate } | ForEach-Object {
	Remove-Item $_.FullName -Force
	$deletedCount++
	"Deleted: $($_.Name)"
}

Write-Host "Deleted $deletedCount old error files"
```

---

**Document Version:** 1.0  
**Last Updated:** 2026-06-01
