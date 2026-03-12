using Elmah;
using System;

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
        }

        /// <summary>
        /// Log an exception to ELMAH.
        /// </summary>
        public static void LogError(Exception exception)
        {
            if (exception == null)
                return;

            try
            {
                // HttpContext.Current is always null in self-hosted OWIN.
                // Passing null causes ELMAH to resolve the log from App.config.
                ErrorLog.GetDefault(null).Log(new Error(exception));
            }
            catch
            {
                // Logging must never crash the application.
            }
        }

        /// <summary>
        /// Log an error with a custom wrapper message.
        /// </summary>
        public static void LogErrorWithMessage(string message, Exception exception)
        {
            LogError(new System.ApplicationException(message, exception));
        }
    }
}
