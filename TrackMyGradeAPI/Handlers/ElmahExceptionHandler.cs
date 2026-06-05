using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using TrackMyGradeAPI.Logging;

namespace TrackMyGradeAPI.Handlers
{
    /// <summary>Global exception handler for Web API that logs errors to ELMAH.</summary>
    public class ElmahExceptionHandler : ExceptionHandler
    {
        /// <summary>Handles exceptions by creating a generic error response.</summary>
        /// <param name="context">The exception handler context.</param>
        public override void Handle(ExceptionHandlerContext context)
        {
            var response = context.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                new { message = "An internal server error occurred. Please try again later." });

            context.Result = new ResponseMessageResult(response);
        }

        /// <summary>Determines whether this handler should handle the exception.</summary>
        /// <param name="context">The exception handler context.</param>
        /// <returns>True to handle all exceptions; false otherwise.</returns>
        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            // Handle all exceptions
            return true;
        }
    }

    /// <summary>Exception logger that logs unhandled exceptions to ELMAH.</summary>
    public class ElmahExceptionLogger : ExceptionLogger
    {
        /// <summary>Logs the exception to ELMAH with request context information.</summary>
        /// <param name="context">The exception logger context.</param>
        public override void Log(ExceptionLoggerContext context)
        {
            ErrorLoggingConfig.LogErrorWithMessage(
                $"API Exception: {context.Request.RequestUri}",
                context.Exception);
        }

        /// <summary>Determines whether this logger should log the exception.</summary>
        /// <param name="context">The exception logger context.</param>
        /// <returns>True if exception exists and should be logged; false otherwise.</returns>
        public override bool ShouldLog(ExceptionLoggerContext context)
        {
            return context.Exception != null;
        }
    }
}