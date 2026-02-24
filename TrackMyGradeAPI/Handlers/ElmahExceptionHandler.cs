using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using TrackMyGradeAPI.Logging;

namespace TrackMyGradeAPI.Handlers
{
    /// <summary>
    /// Global exception handler for Web API that logs errors to ELMAH
    /// </summary>
    public class ElmahExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            Exception exception = context.Exception;

            // Create a response with error details
            // Logging is handled by ElmahExceptionLogger
            var response = context.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                new
                {
                    message = "An error occurred while processing your request.",
                    exceptionMessage = exception.Message,
                    exceptionType = exception.GetType().Name
                });

            context.Result = new ResponseMessageResult(response);
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            // Handle all exceptions
            return true;
        }
    }

    /// <summary>
    /// Exception logger that logs to ELMAH
    /// </summary>
    public class ElmahExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            ErrorLoggingConfig.LogErrorWithMessage(
                $"API Exception: {context.Request.RequestUri}",
                context.Exception);
        }

        public override bool ShouldLog(ExceptionLoggerContext context)
        {
            return context.Exception != null;
        }
    }
}
