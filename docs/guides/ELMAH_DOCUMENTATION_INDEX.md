# ELMAH Documentation Index

## Quick Navigation

This index helps you find the right ELMAH documentation for your needs.

---

## Documentation Files

### 1. **ELMAH Quick Start** (`ELMAH_QUICK_START.md`)
**Length:** ~2 min read  
**Best for:** Getting started immediately  
**Contains:**
- API endpoint URLs
- PowerShell commands
- File system locations
- Quick troubleshooting

**When to read:** First time using ELMAH or need quick reference

---

### 2. **ELMAH Implementation and Usage Guide** (`ELMAH_IMPLEMENTATION_AND_USAGE.md`)
**Length:** ~15 min read  
**Best for:** Complete understanding of how ELMAH works  
**Contains:**
- Full implementation details
- Configuration changes made
- How errors are captured
- Step-by-step access methods
- File format explanation
- Best practices
- Troubleshooting guide

**When to read:** Want to understand the complete system

---

### 3. **ELMAH Examples and Tutorials** (`ELMAH_EXAMPLES_AND_TUTORIALS.md`)
**Length:** ~20 min read (reference)  
**Best for:** Learning through examples and real scenarios  
**Contains:**
- 8 detailed tutorials with expected output
- PowerShell command examples
- Browser access examples
- Stack trace analysis
- Error searching techniques
- Real-time monitoring
- Advanced scenarios

**When to read:** Want to see examples and learn by doing

---

## Which Document Should I Read?

### "I just want to access the logs right now"
→ Read **ELMAH Quick Start**
- Go to **API Endpoints** section
- Copy-paste a URL or PowerShell command
- Done in 2 minutes

---

### "I want to understand how ELMAH works"
→ Read **ELMAH Implementation and Usage Guide**
- Start with **Implementation Summary**
- Read **How Errors Are Captured**
- Understand the complete architecture
- Reference as needed

---

### "I want to see examples and try it myself"
→ Read **ELMAH Examples and Tutorials**
- Pick a tutorial (Tutorial 1-4 for API, Tutorial 5-8 for files)
- Follow step-by-step instructions
- See expected output
- Learn by doing

---

### "I want to monitor errors in production"
→ Read **ELMAH Implementation and Usage Guide**
- Section: **Method 1: API Endpoints**
- Section: **Best Practices**
- Section: **File Management**

---

### "I want to search/analyze errors with PowerShell"
→ Read **ELMAH Examples and Tutorials**
- Section: **Tutorial 7: Search Error Files**
- Section: **Advanced Usage**
- Copy scripts and customize

---

## Quick Reference

### File Locations
```
App.config:              TrackMyGradeAPI/App.config
DiagnosticsController:   TrackMyGradeAPI/Presentation/Controllers/DiagnosticsController.cs
Error logs:              TrackMyGradeAPI/App_Data/errors/
```

### API Endpoints
```
GET /api/diagnostics/elmah-logs                 - List recent errors
GET /api/diagnostics/elmah-logs?count=20        - List with custom count
GET /api/diagnostics/elmah-logs/{errorId}       - Get error details
GET /api/diagnostics/elmah-status               - Check ELMAH config
GET /api/diagnostics/health                     - Health check
```

### Storage
```
Type:     XML File-based (XmlFileErrorLog)
Location: App_Data\errors\
Format:   error-YYYYMMDD-###.xml
Retention: Unlimited (until manually deleted)
Survives restart: Yes
```

---

## Document Reading Path

### Path 1: Hands-On Developer
```
1. ELMAH Quick Start (2 min)
   ↓
2. Try the examples (10 min)
   ↓
3. ELMAH Examples and Tutorials (reference as needed)
```

### Path 2: Thorough Understanding
```
1. ELMAH Implementation and Usage Guide (full read, 15 min)
   ↓
2. ELMAH Examples and Tutorials (skim tutorials)
   ↓
3. ELMAH Quick Start (bookmark for quick access)
```

### Path 3: Troubleshooting
```
1. ELMAH Quick Start (check basic info)
   ↓
2. ELMAH Implementation and Usage Guide → Troubleshooting section
   ↓
3. ELMAH Examples and Tutorials (if need advanced techniques)
```

---

## Implementation Timeline

| Date | What Changed | Why | File |
|------|-------------|-----|------|
| 2026-06-01 | Changed from MemoryErrorLog to XmlFileErrorLog | Persistent storage | App.config |
| 2026-06-01 | Created DiagnosticsController | API access to logs | DiagnosticsController.cs |
| 2026-06-01 | Created App_Data\errors folder | Storage location | (directory) |

---

## Common Tasks

### Task: Get Last 10 Errors

**Quick method:**
1. Open browser
2. Navigate to: `http://localhost:5000/api/diagnostics/elmah-logs?count=10`
3. View in browser

**Documentation:** ELMAH Quick Start → "API Endpoints"

---

### Task: Find Errors for Specific User

**File method:**
```powershell
Select-String -Path "C:\Users\Developer.03\Desktop\TrackMyGrade\TrackMyGradeAPI\App_Data\errors\*.xml" -Pattern "teacher@school.com"
```

**Documentation:** ELMAH Examples and Tutorials → "Tutorial 7: Search Error Files"

---

### Task: Get Stack Trace for Debugging

**API method:**
```
http://localhost:5000/api/diagnostics/elmah-logs/error-20260601-001
```

**File method:**
Open `error-20260601-001.xml` in Notepad, view `<detail>` element

**Documentation:** ELMAH Examples and Tutorials → "Tutorial 3: Get Specific Error with Stack Trace"

---

### Task: Monitor Errors While Testing

**File method:**
1. Open `App_Data\errors\` folder
2. Keep window visible
3. Run API and trigger errors
4. Watch new files appear

**Documentation:** ELMAH Examples and Tutorials → "Tutorial 8: Monitor Errors in Real-Time"

---

## Troubleshooting Reference

| Problem | Solution | Documentation |
|---------|----------|-----------------|
| Endpoint returns 404 | Rebuild project and restart API | ELMAH Implementation and Usage → Troubleshooting |
| No error files appearing | Check App.config and folder permissions | ELMAH Implementation and Usage → Troubleshooting |
| Logs lost after restart | Using MemoryErrorLog instead of XmlFileErrorLog | ELMAH Implementation and Usage → Implementation Summary |
| App.config errors | Validate XML syntax | ELMAH Quick Start → Troubleshooting |

---

## Key Concepts

### Method 1: API Endpoints
- Programmatic access to logs
- Returns JSON format
- Requires API running
- Best for dashboards and automation

### Method 2: File System
- Direct access to XML files
- Works when API is down
- Best for debugging and archival
- Requires file system access

### XmlFileErrorLog
- Persistent file-based storage
- Stores errors as individual XML files
- Location: `App_Data\errors\`
- Survives API restarts

### DiagnosticsController
- New API controller with 4 endpoints
- Provides programmatic access to ELMAH logs
- Fully documented with XML comments
- Returns formatted JSON responses

---

## Summary

**ELMAH is now fully implemented with:**
- ✓ Persistent file-based storage (XmlFileErrorLog)
- ✓ API endpoints for programmatic access (DiagnosticsController)
- ✓ Multiple access methods (API + File System)
- ✓ Complete documentation with examples
- ✓ Unlimited error retention
- ✓ Errors survive API restarts

**You can access logs three ways:**
1. **Browser:** `http://localhost:5000/api/diagnostics/elmah-logs`
2. **PowerShell:** `curl "http://localhost:5000/api/diagnostics/elmah-logs"`
3. **File System:** Open `App_Data\errors\` folder

---

## Next Steps

1. **Try it now:** Follow ELMAH Quick Start
2. **Understand it:** Read ELMAH Implementation and Usage Guide
3. **Learn by example:** Follow tutorials in ELMAH Examples and Tutorials
4. **Reference:** Bookmark ELMAH Quick Start for quick access

---

**Document Index Version:** 1.0  
**Last Updated:** 2026-06-01  
**Status:** Complete
