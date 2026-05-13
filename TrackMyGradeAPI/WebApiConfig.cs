using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Newtonsoft.Json.Serialization;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Infrastructure;

namespace TrackMyGradeAPI
{
    /// <summary>Web API configuration class for registering routes, filters, and services.</summary>
    public static class WebApiConfig
    {
        /// <summary>Registers the Web API configuration including dependency resolution, CORS, and error handling.</summary>
        /// <param name="config">The HTTP configuration object.</param>
        public static void Register(HttpConfiguration config)
        {
            // Register dependency resolver so controllers with constructor injection work
            config.DependencyResolver = new SimpleDependencyResolver();

            // Register FluentValidation as a global action filter
            config.Filters.Add(new FluentValidationFilter());

            // Enable CORS with explicit origins
            var cors = new EnableCorsAttribute(
                origins: "http://localhost:4200",
                headers: "*",
                methods: "*");
            config.EnableCors(cors);

            // Configure ELMAH exception handling
            config.Services.Replace(typeof(IExceptionHandler), new ElmahExceptionHandler());
            config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());

            // Web API routes - Attribute routes MUST come first
            config.MapHttpAttributeRoutes();

            // Note: Conventional routes are not needed when using attribute routing
            // Commenting out to prevent route conflicts
            // config.Routes.MapHttpRoute(
            //     name: "DefaultApi",
            //     routeTemplate: "api/{controller}/{id}",
            //     defaults: new { id = RouteParameter.Optional }
            // );

            // Register Swagger / Swagger UI
            SwaggerConfig.Register(config);

            // Configure JSON formatter
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = 
                Newtonsoft.Json.NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        }
    }
}
