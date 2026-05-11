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
    public class SimpleDependencyResolver : IDependencyResolver
    {
        public IDependencyScope BeginScope() => new SimpleDependencyScope();
        public object GetService(Type serviceType) => null;
        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();
        public void Dispose() { }
    }

    public class SimpleDependencyScope : IDependencyScope
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper              _mapper;
        private readonly ITokenService        _tokenService;
        private bool _disposed;

        public SimpleDependencyScope()
        {
            _db           = new ApplicationDbContext();
            _mapper       = AutoMapperConfig.Mapper;
            _tokenService = new TokenService();
        }

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

        public IEnumerable<object> GetServices(Type serviceType) => new List<object>();

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
