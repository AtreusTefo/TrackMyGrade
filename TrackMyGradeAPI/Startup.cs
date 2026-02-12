using Owin;
using System.Web.Http;
using AutoMapper;
using FluentValidation;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Mapping;
using TrackMyGradeAPI.Services;
using TrackMyGradeAPI.Validators;

namespace TrackMyGradeAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Configure AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            var mapper = mapperConfig.CreateMapper();

            // Configure Validators
            var registerValidator = new TeacherRegisterValidator();
            var loginValidator = new TeacherLoginValidator();
            var studentCreateValidator = new StudentCreateValidator();
            var studentUpdateValidator = new StudentUpdateValidator();

            // Configure DbContext
            var dbContext = new ApplicationDbContext();
            dbContext.Database.CreateIfNotExists();

            // Configure Services with Dependency Injection
            // Note: For proper DI, you would use an IoC container like Autofac or SimpleInjector
            // This is a simplified version

            // Web API configuration and services
            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }
    }
}
