# 📋 Daily Development Report
**Project:** TrackMyGrade - Student Grade Management System  
**Date:** 27 February 2026  
**Developer:** TrackMyGrade Team  
**Branch:** main  
**Repository:** https://github.com/AtreusTefo/TrackMyGrade

---

## 1. 🗂️ What I Did Today

### Overview
Conducted end-to-end API integration testing using Postman for the TrackMyGrade API. Identified, diagnosed, and resolved multiple bugs and configuration issues that were preventing the API from functioning correctly. Updated Postman collection and environment files, and verified all fixes through successful testing.

### Activities

#### 🔬 API Testing & Bug Investigation
- Launched the TrackMyGrade API and imported the Postman collection
- Executed API requests using the `TrackMyGradeAPI.postman_collection.json` collection
- Systematically diagnosed all errors returned by the API
- Traced errors through stack traces, HTTP response messages, and configuration files

#### 🔧 Bug Fixing
- Analysed and fixed the ELMAH error logging configuration in `web.config`
- Refactored the API startup script `start-api.ps1` to correctly use the OWIN self-hosted runner instead of IIS Express
- Removed conflicting conventional routing from `WebApiConfig.cs`, switching to attribute-only routing
- Updated the Postman collection phone numbers to match the server-side validation rules
- Updated ELMAH documentation with correct assertion syntax and troubleshooting guidance

#### ✅ Verification & Testing
- Successfully registered a teacher via `POST /api/teachers/register`
- Successfully created a student via `POST /api/students`
- Successfully updated a student via `PUT /api/students/{id}`
- Verified API responses return correct calculated fields: `total`, `average`, `percentage`, `performanceLevel`

---

## 2. ✅ What Was Completed

### Bug Fixes

| # | Issue | HTTP Code | File Changed | Resolution |
|---|-------|-----------|--------------|------------|
| 1 | ELMAH configuration crashed the app on startup | 500 | `web.config` | Changed `<eq>` → `<equal>` |
| 2 | API startup script used IIS Express instead of OWIN | 404 | `start-api.ps1` | Rewrote script to run OWIN `.exe` directly |
| 3 | Postman phone numbers failed server-side validation | 400 | `postman_collection.json` | Updated from 10-digit to 8-digit phone numbers |
| 4 | PUT Update Student returned Method Not Allowed | 405 | `WebApiConfig.cs` | Removed conventional routing that conflicted with attribute routes |
| 5 | `{{lastStudentId}}` not replaced in URL | 404 | Postman workflow | Documented correct request execution order |

### Files Modified

| File | Type | Change |
|------|------|--------|
| `TrackMyGradeAPI/web.config` | Config | Fixed ELMAH `<errorFilter>` assertion syntax |
| `TrackMyGradeAPI/WebApiConfig.cs` | C# | Removed conventional route, attribute routing only |
| `TrackMyGradeAPI/start-api.ps1` | PowerShell | Changed from IIS Express to OWIN self-hosted |
| `TrackMyGradeAPI/Logging/ELMAH_SETUP.md` | Docs | Corrected syntax examples, added troubleshooting section |
| `README.md` | Docs | Updated with current API configuration |
| `TrackMyGradeAPI.postman_collection.json` | Postman | Fixed phone numbers, added environment variable scripts |
| `TrackMyGradeAPI.postman_environment.json` | Postman | Configured `baseUrl`, `teacherId`, `lastStudentId` variables |

### Endpoints Tested Successfully

| Method | Endpoint | Status | Result |
|--------|----------|--------|--------|
| POST | `/api/teachers/register` | ✅ 200 OK | Teacher registered, token generated |
| POST | `/api/students` | ✅ 201 Created | Student created, grades auto-calculated |
| PUT | `/api/students/{id}` | ✅ 200 OK | Student updated, metrics recalculated |

### Documentation Created

- `POSTMAN_INTEGRATION_GUIDE.md` - Complete Postman setup and usage guide
- `POSTMAN_WORKFLOW_GUIDE.md` - Correct request execution order
- `TrackMyGradeAPI/TROUBLESHOOTING.md` - Updated troubleshooting reference
- `TrackMyGradeAPI/run-postman-tests.ps1` - Automated Postman test runner script

---

## 3. ⚠️ Challenges Faced

### Challenge 1: ELMAH Crashing the App on Every Request (500)
**Severity:** 🔴 Critical — API completely non-functional

**Description:**  
Every API request returned a `500 Internal Server Error` before any controller code executed. The error originated in `web.config` during application startup.

**Error:**
```
Parser Error Message: Elmah.Assertions.AssertionFactory does not have a method named assert_eq.
Source File: web.config — Line 41
```

**Root Cause:**  
The ELMAH `<errorFilter>` block used `<eq>` as the assertion element. ELMAH's `AssertionFactory` maps XML element names to methods by convention using the prefix `assert_`. The element `<eq>` maps to `assert_eq`, which does not exist. The correct element is `<equal>`, which maps to `assert_equal`.

**Fix:**
```xml
<!-- Before -->
<eq binding="HttpStatusCode" value="404" type="Int32"/>

<!-- After -->
<equal binding="HttpStatusCode" value="404" type="Int32"/>
```

---

### Challenge 2: API Start Script Launching Wrong Host (404)
**Severity:** 🔴 Critical — All endpoints unreachable

**Description:**  
After fixing the ELMAH issue, the API still returned `404 Not Found` on all endpoints including Swagger. Port 5000 was occupied by the System process (IIS/HTTP.sys).

**Root Cause:**  
`start-api.ps1` was configured to launch the API using **IIS Express** (`iisexpress.exe`), but TrackMyGrade is an **OWIN self-hosted console application**. IIS Express cannot host an OWIN app by pointing at a project directory — it requires a full IIS site configuration. The compiled `.exe` was available in the `bin\` folder and is the correct host.

**Fix:**  
Rewrote `start-api.ps1` to execute `bin\TrackMyGradeAPI.exe` directly, which launches the OWIN `WebApp.Start<Startup>()` listener correctly.

---

### Challenge 3: Phone Number Validation Mismatch (400)
**Severity:** 🟡 Medium — Registration and student creation blocked

**Description:**  
After resolving the startup issues, registration requests returned a `400 Bad Request` with the message `"Phone must be exactly 8 digits"`.

**Root Cause:**  
The Postman collection request bodies contained 10-digit phone numbers (`"1234567890"`, `"9876543210"`). The FluentValidation rules in both `TeacherRegisterValidator` and `StudentCreateValidator` enforce exactly 8 digits using the regex `^\d{8}$`. This was a mismatch between the test data and the validation rules — the validation rules were correct and intentional.

**Fix:**  
Updated all phone number values in the Postman collection from 10 digits to 8 digits:
- `"1234567890"` → `"12345678"`
- `"9876543210"` → `"98765432"`

---

### Challenge 4: PUT Update Student Returning Method Not Allowed (405)
**Severity:** 🔴 Critical — Update endpoint completely broken

**Description:**  
Calling `PUT /api/students/{id}` returned `405 Method Not Allowed` with the message `"The requested resource does not support http method 'PUT'."`

**Root Cause:**  
`WebApiConfig.cs` registered both **attribute routing** (`config.MapHttpAttributeRoutes()`) and a **conventional route** (`api/{controller}/{id}`). When a `PUT` request arrived at `/api/students/1`:

1. The conventional route matched the URL pattern `api/{controller}/{id}`
2. It attempted to find an action using Web API's naming convention — looking for a method named `Put` or `PutStudent`
3. The controller's update method is named `Update` (decorated with `[HttpPut]`)
4. Conventional routing does not read `[HttpPut]` attributes — it relies purely on method naming
5. No matching action was found → `405 Method Not Allowed`

The fix required removing the conventional route entirely, leaving attribute routing as the sole routing mechanism, which correctly reads the `[HttpPut]` attribute and routes to the `Update` method.

**Fix:**
```csharp
// Removed from WebApiConfig.cs
// config.Routes.MapHttpRoute(
//     name: "DefaultApi",
//     routeTemplate: "api/{controller}/{id}",
//     defaults: new { id = RouteParameter.Optional }
// );
```

---

### Challenge 5: Postman Variable `{{lastStudentId}}` Not Substituted (404)
**Severity:** 🟡 Medium — Update/Delete/GetById requests sent literal variable name

**Description:**  
Requests to `PUT /api/students/{{lastStudentId}}` sent the literal string `{{lastStudentId}}` in the URL instead of a real student ID, causing a 404.

**Root Cause:**  
Postman replaces `{{variable}}` tokens with values stored in the active environment. The `lastStudentId` variable is populated by the **Create Student** test script upon a `201 Created` response. If the Create Student request had not been run first — or if no environment was selected — the variable remained unset, and Postman passed it through unsubstituted.

**Resolution:**  
This was a workflow issue, not a code bug. Documented the correct execution order:
1. Register Teacher → sets `teacherId`
2. Create Student → sets `lastStudentId`
3. Get / Update / Delete Student → consume `lastStudentId`

---

## 4. 📌 Summary

| Category | Count |
|----------|-------|
| Bugs identified | 5 |
| Bugs fixed | 4 |
| Workflow issues documented | 1 |
| Files modified | 7 |
| Endpoints tested successfully | 3 |

**API Status at End of Day:** ✅ Fully operational on `http://localhost:5000`

---

## 5. 🔜 Remaining / Next Steps

- [ ] Test remaining endpoints: `GET /api/students`, `GET /api/students/{id}`, `DELETE /api/students/{id}`, `GET /api/teachers/{id}`, `POST /api/teachers/login`
- [ ] Run Postman validation test scenarios (invalid scores, negative values, missing fields)
- [ ] Run full Postman Collection Runner for automated test report
- [ ] Consider persisting ELMAH logs to XML file for production use
- [ ] Deploy API to production environment (Azure / IIS)
