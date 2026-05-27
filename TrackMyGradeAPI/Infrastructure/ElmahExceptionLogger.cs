using System.Web.Http.ExceptionHandling;
using TrackMyGradeAPI.Logging;

namespace TrackMyGradeAPI.Infrastructure
{
    /// <summary>
    /// Global exception logger for Web API that pipes all unhandled exceptions to ELMAH.
    /// In this self-hosted OWIN application, automatic exception capture via HTTP modules 
    /// is not available, so this logger serves as the primary bridge.
    /// </summary>
    public class ElmahExceptionLogger : ExceptionLogger
    {
        /// <summary>
        /// Logs the unhandled exception to ELMAH with full request context.
        /// </summary>
        /// <param name="context">The context containing the exception and the request information.</param>
        public override void Log(ExceptionLoggerContext context)
        {
            if (context?.Exception == null) return;

            var request = context.Request;
            string requestUri = request?.RequestUri?.ToString() ?? "unknown";
            string method = request?.Method?.Method ?? "unknown";
            string contentType = request?.Content?.Headers?.ContentType?.MediaType ?? "unknown";

            ErrorLoggingConfig.LogErrorWithFullContext(
                context.Exception,
                "System",
                requestUri,
                method,
                contentType);
        }
    }
}