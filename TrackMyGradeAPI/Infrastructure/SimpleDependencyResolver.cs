using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using FluentValidation;
using TrackMyGradeAPI.Controllers;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Mapping;
using TrackMyGradeAPI.Services;
using TrackMyGradeAPI.Validators;

namespace TrackMyGradeAPI.Infrastructure
{
    public class SimpleDependencyResolver : IDependencyResolver
    {
        public IDependencyScope BeginScope()
        {
            return new SimpleDependencyScope();
        }

        public object GetService(Type serviceType) => null;

        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();

        public void Dispose() { }
    }

    public class SimpleDependencyScope : IDependencyScope
    {
        private readonly ApplicationDbContext _dbContext;
        private bool _disposed;

        public SimpleDependencyScope()
        {
            _dbContext = new ApplicationDbContext();
        }

        public object GetService(Type serviceType)
        {
            var mapper = AutoMapperConfig.Mapper;

            if (serviceType == typeof(TeachersController))
                return new TeachersController(
                    new TeacherService(_dbContext, mapper));

            if (serviceType == typeof(StudentsController))
                return new StudentsController(
                    new StudentService(_dbContext, mapper));

            if (serviceType == typeof(IValidator<StudentCreateDto>))
                return new StudentCreateValidator();

            if (serviceType == typeof(IValidator<StudentUpdateDto>))
                return new StudentUpdateValidator();

            if (serviceType == typeof(IValidator<TeacherRegisterDto>))
                return new TeacherRegisterValidator();

            if (serviceType == typeof(IValidator<TeacherLoginDto>))
                return new TeacherLoginValidator();

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();

        public void Dispose()
        {
            if (!_disposed)
            {
                _dbContext?.Dispose();
                _disposed = true;
            }
        }
    }
}
