using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Infrastructure;

namespace TrackMyGradeAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Register dependency resolver so controllers with constructor injection work
            config.DependencyResolver = new SimpleDependencyResolver();

            // Enable CORS
            var cors = new EnableCorsAttribute("http://localhost:4200", "*", "*");
            config.EnableCors(cors);

            // Configure ELMAH exception handling
            config.Services.Replace(typeof(IExceptionHandler), new ElmahExceptionHandler());
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure JSON formatter
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = 
                Newtonsoft.Json.NullValueHandling.Ignore;
        }
    }
}
