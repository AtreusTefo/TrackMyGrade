using Owin;
using System.Web.Http;
using TrackMyGradeAPI.Data;

namespace TrackMyGradeAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Initialize database on startup
            ApplicationDbContext.Initialize();

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}
