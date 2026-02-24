using Owin;
using System.Web.Http;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Mapping;

namespace TrackMyGradeAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
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
