using System.Threading.Tasks;
using Microsoft.Owin;

namespace TrackMyGradeAPI.Middleware
{
    /// <summary>OWIN middleware that adds security headers to all HTTP responses.</summary>
    public class SecurityHeadersMiddleware : OwinMiddleware
    {
        /// <summary>Initializes a new instance of the SecurityHeadersMiddleware class.</summary>
        /// <param name="next">The next middleware in the OWIN pipeline.</param>
        public SecurityHeadersMiddleware(OwinMiddleware next) : base(next) { }

        /// <summary>Invokes the middleware to add security headers to the response.</summary>
        /// <param name="context">The OWIN context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Invoke(IOwinContext context)
        {
            // Add Content Security Policy header
            // Allows Google Fonts and inline scripts (needed for Angular bootstrap)
            string cspPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; connect-src 'self' http://localhost:5000; img-src 'self' data:; frame-ancestors 'none'";
            context.Response.Headers.Add("Content-Security-Policy", new[] { cspPolicy });

            // Additional security headers
            context.Response.Headers.Add("X-Content-Type-Options", new[] { "nosniff" });
            context.Response.Headers.Add("X-Frame-Options", new[] { "DENY" });
            context.Response.Headers.Add("X-XSS-Protection", new[] { "1; mode=block" });

            await Next.Invoke(context);
        }
    }
}
