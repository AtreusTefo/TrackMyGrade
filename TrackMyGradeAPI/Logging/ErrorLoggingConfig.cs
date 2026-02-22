using Elmah;
using System;
using System.Web;

namespace TrackMyGradeAPI.Logging
{
    /// <summary>
    /// ELMAH Error Logging Configuration and Utilities
    /// </summary>
    public static class ErrorLoggingConfig
    {
        /// <summary>
        /// Initialize ELMAH error logging
        /// Call this from Application_Start in Global.asax.cs
        /// </summary>
        public static void InitializeErrorLogging()
        {
            // ELMAH is configured via web.config modules — nothing to do at startup.
            // ErrorLog, ErrorMail and ErrorFilter modules are registered declaratively.
        }

        /// <summary>
        /// Log a specific error to ELMAH
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="context">Optional HTTP context</param>
        public static void LogError(Exception exception, HttpContextBase context = null)
        {
            if (exception == null)
                return;

            try
            {
                HttpContext currentContext = HttpContext.Current;
                if (currentContext != null)
                {
                    ErrorSignal.FromContext(currentContext).Raise(exception);
                }
                else
                {
                    ErrorLog.GetDefault(null).Log(new Error(exception));
                }
            }
            catch
            {
                // Logging must never crash the application
            }
        }

        /// <summary>
        /// Log an error with custom message
        /// </summary>
        /// <param name="message">Custom error message</param>
        /// <param name="exception">The exception</param>
        public static void LogErrorWithMessage(string message, Exception exception)
        {
            var customException = new System.ApplicationException(message, exception);
            LogError(customException);
        }

        /// <summary>
        /// Get the configured error log
        /// </summary>
        /// <returns>The ELMAH error log instance</returns>
        public static ErrorLog GetErrorLog()
        {
            return ErrorLog.GetDefault(null);
        }

        /// <summary>
        /// Clear all logged errors (useful for testing)
        /// </summary>
        public static void ClearErrorLog()
        {
            var errorLog = GetErrorLog();
            if (errorLog is MemoryErrorLog memoryLog)
            {
                // For MemoryErrorLog, you would need to implement a clear method
                // This is a placeholder for reference
            }
        }
    }
}
