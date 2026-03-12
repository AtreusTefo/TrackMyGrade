# ELMAH Error Logging Setup

This document describes the ELMAH integration in the TrackMyGrade API.

## Hosting Model — Important

This application is **self-hosted via OWIN** (`Microsoft.Owin.SelfHost`). This has direct consequences for ELMAH:

| Feature | IIS-Hosted | Self-Hosted OWIN (this app) |
|---|---|---|
| HTTP Modules (`ErrorLogModule`) | ✅ Run automatically | ❌ Do not run |
| `HttpContext.Current` | ✅ Available | ❌ Always `null` |
| `/elmah.axd` viewer | ✅ Works | ❌ Not available |
| `Global.asax Application_Error` | ✅ Fires | ❌ Does not fire |
| Explicit `ErrorLog.GetDefault(null).Log(...)` | ✅ Works | ✅ Works |

All logging in this app is **explicit and programmatic** — there is no automatic capture.

## NuGet Package

```
ELMAH 1.2.2
```

## Configuration (`App.config`)

```xml
<configSections>
  <sectionGroup name="elmah">
    <section name="security"    requirePermission="false" type="Elmah.SecuritySectionHandler, Elmah"/>
    <section name="errorLog"    requirePermission="false" type="Elmah.ErrorLogSectionHandler, Elmah"/>
    <section name="errorMail"   requirePermission="false" type="Elmah.ErrorMailSectionHandler, Elmah"/>
    <section name="errorFilter" requirePermission="false" type="Elmah.ErrorFilterSectionHandler, Elmah"/>
  </sectionGroup>
</configSections>

<elmah>
  <security allowRemoteAccess="0"/>
  <errorLog type="Elmah.MemoryErrorLog, Elmah" size="500"/>
</elmah>
```

- `allowRemoteAccess="0"` — use `"0"` (disabled) or `"1"` (enabled), not `"true"`/`"false"`
- `MemoryErrorLog` — stores up to 500 errors in memory; errors are lost on restart

## How Logging Works in This App

Exceptions flow through two paths:

### 1. Web API pipeline (unhandled exceptions)
`ElmahExceptionLogger` is registered in `WebApiConfig.cs`:
```csharp
config.Services.Replace(typeof(IExceptionHandler), new ElmahExceptionHandler());
config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());
```
Any exception not caught by a controller reaches `ElmahExceptionLogger.Log()` → `ErrorLoggingConfig.LogErrorWithMessage()`.

### 2. Controller catch blocks (handled exceptions)
```csharp
catch (Exception ex)
{
    ErrorLoggingConfig.LogError(ex);
    return BadRequest(ex.Message);
}
```

Both paths call `ErrorLog.GetDefault(null).Log(new Error(exception))`, which reads the `<elmah><errorLog>` section from `App.config`.

## Using ELMAH in Code

```csharp
// Log an exception
ErrorLoggingConfig.LogError(ex);

// Log with a custom wrapper message
ErrorLoggingConfig.LogErrorWithMessage("Failed during student creation", ex);
```

## Storage Options

### In-Memory (current — development only)
```xml
<errorLog type="Elmah.MemoryErrorLog, Elmah" size="500"/>
```
Errors are lost on restart. Suitable for development only.

### XML File (recommended for this hosting model)
```xml
<errorLog type="Elmah.XmlFileErrorLog, Elmah" logPath="App_Data\errors"/>
```
Persists errors to XML files on disk. Works in self-hosted OWIN with no extra infrastructure.

### SQLite
```xml
<errorLog type="Elmah.SQLiteErrorLog, Elmah" connectionStringName="ElmahConnection"/>
```
Requires the `Elmah.SQLite` NuGet package and a connection string pointing to a `.db` file.


## Overview

ELMAH is a flexible error logging framework that allows you to log, view, and filter unhandled exceptions in your ASP.NET application. The integration includes:

- **Automatic exception logging** for all unhandled errors
- **Error filtering** to exclude certain error types (e.g., 404 errors)
- **Web-based error log viewer** accessible via `/elmah.axd`
- **Email notifications** (optional - can be configured in web.config)
- **Multiple storage backends** (In-Memory, SQL Server, etc.)

## Installation

The ELMAH NuGet package has been added to the project:
- `ELMAH` v1.2.2 — Core error logging framework

## Configuration

### web.config

The main configuration is in `web.config`:

```xml
<elmah>
  <!-- Security: restrict remote access to error log viewer -->
  <security allowRemoteAccess="false"/>
  
  <!-- Error log storage: currently using in-memory storage -->
  <errorLog type="Elmah.MemoryErrorLog, Elmah" size="100"/>
  
  <!-- Error filtering: excludes 404 errors from logging -->
  <errorFilter>
    <test>
      <equal binding="HttpStatusCode" value="404" type="Int32"/>
    </test>
  </errorFilter>
</elmah>
```

## Accessing the Error Log

> ⚠️ **OWIN Self-Hosted Mode**: This application runs as a self-hosted OWIN console process.
> The `/elmah.axd` web viewer requires IIS HTTP handlers and **does not work** in this
> hosting model. Switch to `XmlFileErrorLog` or `SqlErrorLog` (see Storage Options below)
> for persistent, inspectable error storage.

### If Deployed Under IIS

Navigate to:
```
http://localhost:PORT/elmah.axd
```

This page displays:
- List of all logged errors with full stack traces and request details

### Security

Remote access is disabled by default (`allowRemoteAccess="0"`). To enable under IIS:

```xml
<security allowRemoteAccess="1"/>
```

**Warning**: Restrict access to Admin role only via the `<location path="elmah.axd">` authorization section in `web.config`.

## Using ELMAH in Code

### Automatic Logging

In OWIN self-hosted mode, only Web API pipeline exceptions are captured automatically:
1. **Web API Exception Handlers** — `ElmahExceptionLogger` registered in `WebApiConfig.cs`

The following do **not** apply in this hosting model:
- `Global.asax.cs Application_Error` — does not fire in OWIN self-host
- ELMAH HTTP modules (`ErrorLogModule`) — only active under IIS

### Manual Logging

To manually log exceptions in your code:

```csharp
using TrackMyGradeAPI.Logging;

// Log an exception
try 
{
    // some code
}
catch (Exception ex)
{
    ErrorLoggingConfig.LogError(ex);
}

// Log with custom message
ErrorLoggingConfig.LogErrorWithMessage("Custom error message", exception);
```

## Storage Options

### In-Memory Storage (Current)

```xml
<errorLog type="Elmah.MemoryErrorLog, Elmah" size="500"/>
```

**Pros**: No database required, fast
**Cons**: Errors lost on application restart, limited capacity

### SQL Server Storage

To store errors in SQL Server, update web.config:

```xml
<errorLog type="Elmah.SqlErrorLog, Elmah" connectionStringName="DefaultConnection"/>
```

Run the SQL Server installation script to create required tables:

```powershell
sqlcmd -S (local) -E < ELMAH_SQLServer.sql
```

## Email Notifications (Optional)

To enable email notifications for errors, uncomment and configure in web.config:

```xml
<errorMail 
  from="error@yourdomain.com" 
  to="admin@yourdomain.com" 
  subject="TrackMyGrade API Error"
  smtpServer="smtp.yourdomain.com"
  smtpPort="587"
  useSsl="true"
  userName="your-email@domain.com"
  password="your-password"/>
```

## Error Filtering

ELMAH allows you to exclude certain errors from logging. Current filter excludes 404 errors:

```xml
<errorFilter>
  <test>
    <equal binding="HttpStatusCode" value="404" type="Int32"/>
  </test>
</errorFilter>
```

**Important:** Use `<equal>` not `<eq>`. ELMAH's assertion syntax requires `<equal>` for equality tests.

To filter other error types, modify the `<test>` element. Examples:

```xml
<!-- Exclude 401 Unauthorized -->
<equal binding="HttpStatusCode" value="401" type="Int32"/>

<!-- Exclude specific exception types -->
<regex binding="Exception.Type" pattern="NullReferenceException"/>

<!-- Exclude by message -->
<regex binding="Exception.Message" pattern="timeout"/>

<!-- Multiple conditions (AND) -->
<test>
  <and>
    <equal binding="HttpStatusCode" value="404" type="Int32"/>
    <regex binding="Context.Request.Path" pattern="/api/"/>
  </and>
</test>

<!-- Multiple conditions (OR) -->
<test>
  <or>
    <equal binding="HttpStatusCode" value="404" type="Int32"/>
    <equal binding="HttpStatusCode" value="401" type="Int32"/>
  </or>
</test>
```

## Exception Handlers

### Global Handler (Global.asax.cs)

Catches unhandled exceptions from the entire application and logs them to ELMAH.

### Web API Exception Handlers

Two custom exception handlers are registered in WebApiConfig:

1. **ElmahExceptionHandler** - Handles exceptions and returns JSON error response
2. **ElmahExceptionLogger** - Logs exception details

These ensure API errors are properly logged and formatted.

## Best Practices

1. **Monitor Errors Regularly**: Switch from `MemoryErrorLog` to `XmlFileErrorLog` or `SqlErrorLog` so errors persist across restarts and can be reviewed offline
2. **Configure Email Alerts**: For production, enable email notifications for critical errors
3. **Use Persistent Storage**: For production, use `XmlFileErrorLog` or `SqlErrorLog` instead of in-memory storage
4. **Secure Access**: When deploying under IIS, restrict `/elmah.axd` to `Admin` role only via the `<location>` authorization block in `web.config`
5. **Filter Errors Appropriately**: Exclude expected errors (404, 401) to focus on real issues
6. **Review Stack Traces**: Use detailed error information to fix bugs quickly

## Troubleshooting

### Configuration Error: "AssertionFactory does not have a method named assert_eq"

**Error Message:**
```
Parser Error Message: Elmah.Assertions.AssertionFactory does not have a method named assert_eq.
Source File: web.config Line: 41
```

**Cause:** Incorrect ELMAH error filter syntax. Using `<eq>` instead of `<equal>`.

**Solution:** Change the error filter assertion from `<eq>` to `<equal>`:

```xml
<!-- ❌ WRONG -->
<errorFilter>
  <test>
    <eq binding="HttpStatusCode" value="404" type="Int32"/>
  </test>
</errorFilter>

<!-- ✅ CORRECT -->
<errorFilter>
  <test>
    <equal binding="HttpStatusCode" value="404" type="Int32"/>
  </test>
</errorFilter>
```

**Valid ELMAH Assertions:**
- `<equal>` - Equality check
- `<greater>` - Greater than
- `<lesser>` - Less than
- `<is-type>` - Type check
- `<regex>` - Regular expression match
- `<and>`, `<or>`, `<not>` - Logical operators

### ELMAH page not loading

- Ensure `modules` and `handlers` are configured in web.config
- Check that the path matches: `path="elmah.axd"`
- Verify IIS/IIS Express permissions
- **Note**: `/elmah.axd` does NOT work in OWIN self-hosted mode (only IIS)

### Errors not appearing

- Check error filters in web.config
- Ensure error log type is properly configured
- Check application event logs for ELMAH startup errors
- Verify `ErrorLoggingConfig.LogError()` is being called in catch blocks

### Storage issues

- For SQL Server: verify connection string and database tables exist
- For In-Memory: verify size parameter is set (default 100)

## References

- [ELMAH Official Documentation](https://elmah.github.io/)
- [Microsoft ELMAH Tutorial](https://learn.microsoft.com/en-us/aspnet/web-forms/overview/older-versions-getting-started/deploying-web-site-projects/logging-error-details-with-elmah-cs)
- [ELMAH Error Filtering Syntax](https://elmah.github.io/a/error-filtering/)
