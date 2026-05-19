using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using TrackMyGradeAPI.Logging;

namespace TrackMyGradeAPI.Middleware
{
    /// <summary>
    /// OWIN middleware that catches unhandled exceptions in the pipeline and logs them to ELMAH.
    /// This middleware wraps the entire request processing and acts as a safety net for exceptions
    /// that bypass the Web API exception handlers (e.g., routing errors, middleware errors).
    /// </summary>
    public class ErrorHandlingMiddleware : OwinMiddleware
    {
        /// <summary>Initializes a new instance of the ErrorHandlingMiddleware class.</summary>
        /// <param name="next">The next middleware in the OWIN pipeline.</param>
        public ErrorHandlingMiddleware(OwinMiddleware next) : base(next) { }

        /// <summary>
        /// Invokes the middleware to catch and log exceptions.
        /// If an exception occurs, it is logged to ELMAH with request context information.
        /// </summary>
        /// <param name="context">The OWIN context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                // Log the unhandled exception to ELMAH with full context
                try
                {
                    string requestUri = context.Request.Uri?.ToString() ?? "unknown";
                    string method = context.Request.Method ?? "unknown";
                    string userId = context.Request.User?.Identity?.Name ?? "anonymous";

                    ErrorLoggingConfig.LogErrorWithFullContext(
                        ex,
                        userId,
                        requestUri,
                        method,
                        context.Request.ContentType,
                        new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "MiddlewareLevel", "ErrorHandlingMiddleware" },
                            { "IsAuthenticated", context.Request.User?.Identity?.IsAuthenticated ?? false }
                        });
                }
                catch
                {
                    // If ELMAH logging fails, silently continue (don't mask the original exception)
                }

                // Re-throw the exception so the caller (typically WebApp.Start) can handle it
                throw;
            }
        }
    }
}
