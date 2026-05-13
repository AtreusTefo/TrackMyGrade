using Owin;
using System.Web.Http;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Mapping;
using TrackMyGradeAPI.Middleware;

namespace TrackMyGradeAPI
{
    /// <summary>OWIN startup configuration class for TrackMyGrade API.</summary>
    public class Startup
    {
        /// <summary>Configures the OWIN pipeline with security middleware, services, and Web API routes.</summary>
        /// <param name="app">The OWIN application builder.</param>
        public void Configuration(IAppBuilder app)
        {
            // Add security headers middleware (must be first)
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
