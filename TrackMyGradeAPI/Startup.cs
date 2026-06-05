using Owin;
using System.Web.Http;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Mapping;
using TrackMyGradeAPI.Middleware;
using Microsoft.Owin.Cors;
using System.Web.Cors;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace TrackMyGradeAPI
{
    /// <summary>OWIN startup configuration class for TrackMyGrade API.</summary>
    public class Startup
    {
        /// <summary>Configures the OWIN pipeline with security middleware, services, and Web API routes.</summary>
        /// <param name="app">The OWIN application builder.</param>
        public void Configuration(IAppBuilder app)
        {
            // Configure OWIN CORS policy early in the pipeline so preflight requests
            // are handled before authentication middleware (prevents missing CORS headers).
            var corsPolicy = new CorsPolicy
            {
                AllowAnyHeader = false,
                AllowAnyMethod = false,
                AllowAnyOrigin = false,
                SupportsCredentials = true
            };

            // Allowed origin for local development Angular app
            corsPolicy.Origins.Add("http://localhost:4200");

            // Common headers used by the frontend (include Authorization and custom tokens)
            corsPolicy.Headers.Add("authorization");
            corsPolicy.Headers.Add("content-type");
            corsPolicy.Headers.Add("x-studenttoken");
            corsPolicy.Headers.Add("x-teacherid");

            // Allowed HTTP methods including OPTIONS for preflight
            corsPolicy.Methods.Add("GET");
            corsPolicy.Methods.Add("POST");
            corsPolicy.Methods.Add("PUT");
            corsPolicy.Methods.Add("DELETE");
            corsPolicy.Methods.Add("OPTIONS");

            app.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = ctx => Task.FromResult(corsPolicy)
                }
            });

            // Add error handling middleware (must be early to catch exceptions)
            app.Use<ErrorHandlingMiddleware>();

            // Add security headers middleware (second in pipeline)
            app.Use<SecurityHeadersMiddleware>();

            // Initialize AutoMapper profiles
            AutoMapperConfig.Initialize();

            // Initialize database on startup
            ApplicationDbContext.Initialize();

            // Initialize ELMAH error logging
            ErrorLoggingConfig.InitializeErrorLogging();

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}
