using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace TrackMyGradeAPI.Infrastructure
{
    /// <summary>
    /// Global exception handler for Web API that provides a consistent error response 
    /// to the client when an unhandled exception occurs.
    /// </summary>
    public class ElmahExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// Handles the unhandled exception by returning a 500 Internal Server Error response.
        /// </summary>
        /// <param name="context">The exception handler context.</param>
        public override void Handle(ExceptionHandlerContext context)
        {
            context.Result = new InternalErrorResult
            {
                Request = context.ExceptionContext.Request,
                Content = "An unhandled error occurred in the API. The incident has been logged."
            };
        }

        /// <summary>
        /// Implementation of IHttpActionResult that returns a consistent 500 error response.
        /// </summary>
        private class InternalErrorResult : IHttpActionResult
        {
            /// <summary>Gets or sets the original request.</summary>
            public HttpRequestMessage Request { get; set; }
            /// <summary>Gets or sets the error message content.</summary>
            public string Content { get; set; }

            /// <summary>Executes the result asynchronously.</summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>A task containing the response message.</returns>
            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(Content),
                    RequestMessage = Request
                };
                return Task.FromResult(response);
            }
        }
    }
}