using Elmah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace TrackMyGradeAPI.Presentation.Controllers
{
    /// <summary>
    /// Diagnostic controller providing system health and error logging endpoints.
    /// Used for troubleshooting and monitoring the API during development.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ApiController
    {
        /// <summary>
        /// Gets recent ELMAH error logs with optional filtering.
        /// Returns up to the specified number of most recent errors from the error log.
        /// </summary>
        /// <param name="count">Number of recent errors to retrieve (default: 50, max: 1000)</param>
        /// <returns>Object containing error count and list of error details</returns>
        /// <remarks>
        /// This endpoint demonstrates Method 1 (programmatic access).
        /// Errors are stored persistently in App_Data\errors\ (Method 2).
        /// 
        /// Example: /api/diagnostics/elmah-logs?count=20
        /// </remarks>
        [HttpGet]
        [Route("elmah-logs")]
        public IHttpActionResult GetElmahLogs([FromUri] int count = 50)
        {
            try
            {
                // Limit to reasonable max to prevent excessive data transfer
                if (count <= 0) count = 50;
                if (count > 1000) count = 1000;

                var errorLog = ErrorLog.GetDefault(null);

                // ErrorLogEntry is used to hold errors retrieved from the log
                var errors = new List<ErrorLogEntry>();

                // GetErrors(pageIndex, pageSize, errorEntryList)
                // pageIndex=0 gives most recent errors
                errorLog.GetErrors(0, count, errors);

                // Map to response DTO with sanitized details
                var errorDetails = errors.Select(e => new
                {
                    id = e.Id,
                    message = e.Message,
                    type = e.Type,
                    timeUtc = e.Time,
                    statusCode = e.StatusCode,
                    hostname = e.Host,
                    url = e.Source,
                    user = e.User
                }).ToList();

                return Ok(new
                {
                    success = true,
                    timestamp = DateTime.UtcNow,
                    totalRetrieved = errorDetails.Count,
                    errors = errorDetails,
                    storageLocation = "App_Data\\errors\\ (XML file-based persistent storage)"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception(
                    $"Failed to retrieve ELMAH logs: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Gets a specific error by ID from the ELMAH log.
        /// Returns the full error details including stack trace.
        /// </summary>
        /// <param name="errorId">The unique error ID to retrieve</param>
        /// <returns>Complete error details or 404 if not found</returns>
        /// <remarks>
        /// Example: /api/diagnostics/elmah-logs/error-20260601-001
        /// </remarks>
        [HttpGet]
        [Route("elmah-logs/{errorId}")]
        public IHttpActionResult GetElmahLogById(string errorId)
        {
            try
            {
                if (string.IsNullOrEmpty(errorId))
                    return BadRequest("errorId parameter is required");

                var errorLog = ErrorLog.GetDefault(null);
                var error = errorLog.GetError(errorId);

                if (error == null)
                    return NotFound();

                return Ok(new
                {
                    success = true,
                    timestamp = DateTime.UtcNow,
                    error = new
                    {
                        id = error.Id,
                        message = error.Message,
                        type = error.Type,
                        timeUtc = error.Time,
                        statusCode = error.StatusCode,
                        hostname = error.Host,
                        url = error.Source,
                        user = error.User,
                        stackTrace = error.Exception?.ToString() ?? "No stack trace available"
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception(
                    $"Failed to retrieve error {errorId}: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Gets diagnostic information about the error logging system.
        /// Returns storage type, location, and configuration details.
        /// </summary>
        /// <returns>ELMAH configuration and status information</returns>
        /// <remarks>
        /// Useful for verifying that file-based storage is correctly configured.
        /// Example: /api/diagnostics/elmah-status
        /// </remarks>
        [HttpGet]
        [Route("elmah-status")]
        public IHttpActionResult GetElmahStatus()
        {
            try
            {
                var errorLog = ErrorLog.GetDefault(null);

                return Ok(new
                {
                    success = true,
                    timestamp = DateTime.UtcNow,
                    elmahStatus = new
                    {
                        loggerType = errorLog?.GetType().Name ?? "Unknown",
                        storageType = "XmlFileErrorLog (File-based persistent storage)",
                        logPath = "App_Data\\errors\\",
                        remoteAccessAllowed = false,
                        note = "Logs persist across API restarts. Errors stored as XML files.",
                        accessMethods = new[]
                        {
                            "GET /api/diagnostics/elmah-logs (list recent errors)",
                            "GET /api/diagnostics/elmah-logs/{errorId} (get error details)",
                            "Direct file access: App_Data\\errors\\error-*.xml"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception(
                    $"Failed to retrieve ELMAH status: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Simple health check endpoint to verify API is running.
        /// </summary>
        /// <returns>Basic health status</returns>
        [HttpGet]
        [Route("health")]
        public IHttpActionResult GetHealth()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                apiVersion = "1.0"
            });
        }
    }
}
