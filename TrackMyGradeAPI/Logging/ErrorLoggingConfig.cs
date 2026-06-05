using Elmah;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrackMyGradeAPI.Logging
{
    /// <summary>
    /// ELMAH Error Logging Configuration and Utilities.
    /// This application is self-hosted via OWIN — HttpContext.Current is always null
    /// and HTTP Modules (ErrorLogModule) do not run. All logging is explicit via
    /// ErrorLog.GetDefault(null), which reads the elmah/errorLog section from App.config.
    /// </summary>
    public static class ErrorLoggingConfig
    {
        /// <summary>
        /// Called from Startup.cs. No runtime setup needed — ELMAH reads App.config automatically.
        /// </summary>
        public static void InitializeErrorLogging()
        {
            // ELMAH resolves its configuration from App.config via ConfigurationManager.
            // In this self-hosted OWIN app, HTTP Modules do not run, so there is no
            // automatic exception capture. All logging is performed explicitly below.
            System.Diagnostics.Trace.WriteLine($"[{DateTime.UtcNow:O}] ELMAH initialized with config from App.config");
        }

        /// <summary>
        /// Log an exception to ELMAH with enhanced context capture.
        /// </summary>
        public static void LogError(Exception exception)
        {
            if (exception == null)
                return;

            LogErrorInternal(exception, null, null);
        }

        /// <summary>
        /// Log an error with a custom wrapper message and optional context.
        /// </summary>
        public static void LogErrorWithMessage(string message, Exception exception)
        {
            if (exception == null)
                return;

            LogErrorInternal(exception, message, null);
        }

        /// <summary>
        /// Log an error with context data (e.g., UserId, Request URI, IP address).
        /// </summary>
        public static void LogErrorWithContext(Exception exception, string userId, string requestUri)
        {
            if (exception == null)
                return;

            LogErrorInternal(exception, null, new { UserId = userId, RequestUri = requestUri });
        }

        /// <summary>
        /// Log an error with full context information including request details.
        /// </summary>
        public static void LogErrorWithFullContext(
            Exception exception,
            string userId,
            string requestUri,
            string method,
            string contentType,
            Dictionary<string, object> contextData = null)
        {
            if (exception == null)
                return;

            var fullContext = new
            {
                UserId = userId,
                RequestUri = requestUri,
                Method = method,
                ContentType = contentType,
                CustomData = contextData,
                Timestamp = DateTime.UtcNow
            };

            LogErrorInternal(exception, null, fullContext);
        }

        /// <summary>
        /// Log a validation error with field names and values.
        /// </summary>
        public static void LogValidationError(Exception exception, string entityType, string fieldName, string fieldValue)
        {
            if (exception == null)
                return;

            var validationContext = new
            {
                EntityType = entityType,
                FieldName = fieldName,
                FieldValue = fieldValue,
                Timestamp = DateTime.UtcNow
            };

            LogErrorInternal(exception, $"Validation error in {entityType}.{fieldName}", validationContext);
        }

        /// <summary>
        /// Log a data consistency error (FK constraint, duplicate, etc.).
        /// </summary>
        public static void LogDataIntegrityError(Exception exception, string operation, string entityType, int entityId)
        {
            if (exception == null)
                return;

            var integrityContext = new
            {
                Operation = operation,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow
            };

            LogErrorInternal(exception, $"Data integrity violation during {operation} of {entityType}", integrityContext);
        }

        /// <summary>
        /// Core internal logging method that captures exception details to ELMAH.
        /// </summary>
        private static void LogErrorInternal(Exception exception, string wrapperMessage, object context)
        {
            try
            {
                Exception logException = exception;
                if (!string.IsNullOrEmpty(wrapperMessage))
                {
                    logException = new System.ApplicationException(wrapperMessage, exception);
                }

                var error = new Error(logException)
                {
                    Time = DateTime.UtcNow
                };

                // Add context data to ELMAH's contextual properties
                if (context != null)
                {
                    var contextProperties = context.GetType().GetProperties();
                    foreach (var prop in contextProperties)
                    {
                        try
                        {
                            var value = prop.GetValue(context);
                            error.ServerVariables.Add($"CONTEXT_{prop.Name}", value?.ToString() ?? "(null)");
                        }
                        catch
                        {
                            // Silently skip properties that can't be serialized
                        }
                    }
                }

                // Add exception chain depth for diagnostics
                int chainDepth = 0;
                var current = exception;
                while (current != null)
                {
                    chainDepth++;
                    current = current.InnerException;
                }
                error.ServerVariables.Add("ExceptionChainDepth", chainDepth.ToString());

                // HttpContext.Current is always null in self-hosted OWIN.
                // Passing null causes ELMAH to resolve the log from App.config.
                ErrorLog.GetDefault(null).Log(error);
            }
            catch (Exception logException)
            {
                // Logging failures must never crash the application.
                // Fallback to console output as last resort
                try
                {
                    System.Diagnostics.Trace.WriteLine(
                        $"[ELMAH Logging Failed] {logException.GetType().Name}: {logException.Message}\n" +
                        $"Original exception was: {exception?.GetType().Name}: {exception?.Message}");
                }
                catch
                {
                    // Absolute last resort - silent fail
                }
            }
        }
    }
}
