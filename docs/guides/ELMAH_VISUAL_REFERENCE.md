# ELMAH Quick Access Guide (Visual Reference)

**Print this page or bookmark it!**

---

## Quick Start (30 Seconds)

```powershell
cd C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI
.\bin\TrackMyGradeAPI.exe
```

Then in browser:
```
http://localhost:5000/api/diagnostics/elmah-logs
```

**Done!** You're viewing error logs.

---

## API Endpoints (Copy & Paste)

### Get Recent Errors
```
http://localhost:5000/api/diagnostics/elmah-logs
```

### Get Last N Errors
```
http://localhost:5000/api/diagnostics/elmah-logs?count=20
http://localhost:5000/api/diagnostics/elmah-logs?count=50
http://localhost:5000/api/diagnostics/elmah-logs?count=100
```

### Get Specific Error (with stack trace)
```
http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001
```

### Check Status
```
http://localhost:5000/api/diagnostics/elmah-status
```

---

## PowerShell Commands (Copy & Paste)

### Get Recent Errors
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs"
```

### Get Last 20 Errors
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs?count=20"
```

### Get Specific Error
```powershell
curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001"
```

### Search for User's Errors
```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "teacher@school.com"
```

### Search for Exception Type
```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "NullReference"
```

### Count All Errors
```powershell
(Get-ChildItem "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\error-*.xml").Count
```

### List All Error Files
```powershell
Get-ChildItem -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\error-*.xml" | Select-Object Name, CreationTime, Length
```

---

## File System Access

### Open Error Folder
```
C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\
```

**Steps:**
1. Press `Windows + E` (File Explorer)
2. Paste path above
3. Press Enter
4. Right-click file → Open with → Notepad

---

## Documentation Map

| Need | File | Read Time |
|------|------|-----------|
| Quick start | **START_HERE.md** | 2 min |
| Commands | **ELMAH_QUICK_REFERENCE.md** | 1 min |
| Overview | **ELMAH_COMPLETE_GUIDE.md** | 5 min |
| Examples | **ELMAH_EXAMPLES_AND_TUTORIALS.md** | 20 min |
| Details | **ELMAH_IMPLEMENTATION_AND_USAGE.md** | 15 min |
| Navigation | **ELMAH_DOCUMENTATION_MASTER_INDEX.md** | 3 min |

---

## Troubleshooting

### Endpoint not found (404)
```powershell
msbuild "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\TrackMyGradeAPI.csproj"
.\bin\TrackMyGradeAPI.exe
```

### No error files
```powershell
New-Item -ItemType Directory -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors" -Force
```

### Need help
**→ Read START_HERE.md**

---

## What Gets Logged

```
Exception Type      → System.NullReferenceException
Error Message       → "Object reference not set"
Stack Trace         → Full call chain with line numbers
Request URL         → /api/students/get-grades
User                → teacher@school.com
Timestamp (UTC)     → 2026-06-01T11:50:00Z
Status Code         → 500
Hostname            → DEVELOPER-03
```

---

## Storage Info

| Property | Value |
|----------|-------|
| Type | XmlFileErrorLog (Persistent) |
| Location | `App_Data\errors\` |
| File Format | `error-YYYYMMDD-###.xml` |
| Capacity | Unlimited |
| Survives Restart | YES |
| Manual Cleanup | Required (delete old files) |

---

## Response Format

### List Response (JSON)
```json
{
  "success": true,
  "timestamp": "2026-06-01T12:00:30Z",
  "totalRetrieved": 50,
  "errors": [
	{
	  "id": "error-20260601-001",
	  "type": "System.NullReferenceException",
	  "message": "Object reference not set",
	  "url": "/api/students/grades",
	  "user": "teacher@school.com",
	  "timeUtc": "2026-06-01T11:50:00Z",
	  "statusCode": 500
	}
  ]
}
```

### Detail Response (JSON)
```json
{
  "success": true,
  "error": {
	"id": "error-20260601-001",
	"type": "System.NullReferenceException",
	"message": "Object reference not set",
	"url": "/api/students/grades",
	"user": "teacher@school.com",
	"timeUtc": "2026-06-01T11:50:00Z",
	"statusCode": 500,
	"stackTrace": "at TrackMyGradeAPI.Services..."
  }
}
```

---

## Common Workflows

### Workflow 1: Find Error by User
```powershell
# Search for user's errors
Select-String -Path "...\App_Data\errors\*.xml" -Pattern "teacher@school.com"

# Get error details via API
curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001"
```

### Workflow 2: Find Error by Type
```powershell
# Search for exception type
Select-String -Path "...\App_Data\errors\*.xml" -Pattern "NullReferenceException"

# Get details
curl "http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001"
```

### Workflow 3: Monitor Recent Errors
```powershell
# Check every minute
while($true) {
	curl "http://localhost:5000/api/diagnostics/elmah-logs?count=10"
	Start-Sleep -Seconds 60
}
```

### Workflow 4: Export Errors
```powershell
# Export all error IDs to file
Select-String -Path "...\App_Data\errors\*.xml" -Pattern 'error time=' | Out-File errors.txt

# Export all user errors
Select-String -Path "...\App_Data\errors\*.xml" -Pattern "teacher@school.com" | Out-File user_errors.txt
```

---

## Key Files

```
TrackMyGradeAPI/
├── App.config
│   └── <elmah>
│       <errorLog type="Elmah.XmlFileErrorLog, Elmah" 
│                  logPath="App_Data\errors"/>
│       </elmah>
│
├── Presentation/Controllers/
│   └── DiagnosticsController.cs (NEW)
│       ├── GetRecentErrors()
│       ├── GetErrorDetail()
│       ├── GetElmahStatus()
│       └── GetHealthStatus()
│
└── App_Data/errors/ (NEW)
	├── error-20260601-001.xml
	├── error-20260601-002.xml
	└── error-20260601-003.xml
```

---

## Implementation Checklist

✅ App.config updated  
✅ DiagnosticsController created  
✅ App_Data\errors directory created  
✅ API endpoints working  
✅ File storage configured  
✅ Documentation complete (9 files)  
✅ Build successful  
✅ Ready for use  

---

## What's New

| Feature | Before | Now |
|---------|--------|-----|
| Storage | Memory | File-based ✅ |
| Persistence | No | Yes ✅ |
| Limit | 500 errors | Unlimited ✅ |
| API Access | No | Yes ✅ |
| File Access | No | Yes ✅ |
| Documentation | Basic | 75+ pages ✅ |

---

## Key Endpoints at a Glance

```
📊 GET /api/diagnostics/elmah-logs
   └─ Returns: List of recent errors (JSON)

📊 GET /api/diagnostics/elmah-logs?count=N
   └─ Returns: Last N errors (JSON)

📊 GET /api/diagnostics/elmah-logs/{id}
   └─ Returns: Error detail with stack trace (JSON)

🔧 GET /api/diagnostics/elmah-status
   └─ Returns: ELMAH configuration info (JSON)

💚 GET /api/diagnostics/health
   └─ Returns: API health status (JSON)
```

---

## URLs You'll Use

**Main API:**
```
http://localhost:5000/api/diagnostics/elmah-logs
```

**File Location:**
```
C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\
```

**Documentation:**
```
C:\Users\Developer.03\Desktop\TrackMyGrade\docs\guides\
```

---

## Remember

✅ Errors automatically logged (no code changes needed)  
✅ Access via API or files  
✅ Survives API restart  
✅ Full documentation available  
✅ Production ready  

---

**Bookmark this page for quick reference!**

For detailed help: **START_HERE.md**
