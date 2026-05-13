using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using AutoMapper;
using TrackMyGradeAPI.Controllers;
using TrackMyGradeAPI.Data;
using TrackMyGradeAPI.Mapping;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Infrastructure
{
    /// <summary>Simple dependency resolver for Web API constructor injection.</summary>
    public class SimpleDependencyResolver : IDependencyResolver
    {
        /// <summary>Begins a dependency scope for this request.</summary>
        /// <returns>A new SimpleDependencyScope instance.</returns>
        public IDependencyScope BeginScope() => new SimpleDependencyScope();
        /// <summary>Gets a service instance of the specified type.</summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>Null; resolution is handled by SimpleDependencyScope.</returns>
        public object GetService(Type serviceType) => null;
        /// <summary>Gets all service instances of the specified type.</summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>An empty list; resolution is handled by SimpleDependencyScope.</returns>
        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();
        /// <summary>Disposes resources used by this resolver.</summary>
        public void Dispose() { }
    }

    /// <summary>Dependency scope that instantiates and manages services for a single request.</summary>
    public class SimpleDependencyScope : IDependencyScope
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;
        private bool _disposed;

        /// <summary>Initializes a new instance of the SimpleDependencyScope class.</summary>
        public SimpleDependencyScope()
        {
            _db           = new ApplicationDbContext();
            _mapper       = AutoMapperConfig.Mapper;
            _tokenService = new TokenService();
        }

        /// <summary>Gets a service instance of the specified type using manual constructor injection.</summary>
        /// <param name="serviceType">The service type to resolve and instantiate.</param>
        /// <returns>An instance of the requested service, or null if type is not recognized.</returns>
        public object GetService(Type serviceType)
        {
            // ── Existing controllers ───────────────────────────────────────
            if (serviceType == typeof(TeachersController))
                return new TeachersController(
                    new TeacherService(_db, _mapper, _tokenService));

            if (serviceType == typeof(StudentsController))
                return new StudentsController(
                    new StudentService(_db, _mapper));

            if (serviceType == typeof(StudentAuthController))
                return new StudentAuthController(
                    new StudentAuthService(_db, _mapper, _tokenService));

            // ── New controllers ────────────────────────────────────────────
            if (serviceType == typeof(AdminController))
                return new AdminController(
                    new AdminService(_db, _mapper, _tokenService, new AuditLogService(_db)),
                    new AuditLogService(_db));

            if (serviceType == typeof(ActivationController))
                return new ActivationController(
                    new ActivationService(_db, _tokenService));

            if (serviceType == typeof(TeacherClassController))
                return new TeacherClassController(
                    new AssignmentService(_db),
                    new StudentService(_db, _mapper));

            if (serviceType == typeof(StudentSubmissionController))
                return new StudentSubmissionController(
                    new AssignmentService(_db));

            return null;
        }

        /// <summary>Gets all service instances of the specified type.</summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <returns>An empty list.</returns>
        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();

        /// <summary>Disposes resources managed by this scope, including the database context.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _db?.Dispose();
                _disposed = true;
            }
        }
    }
}
