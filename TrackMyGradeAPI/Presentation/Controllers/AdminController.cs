using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Admin-only management endpoints.</summary>
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private readonly IAdminService       _adminService;
        private readonly IAuditLogService    _auditLogService;

        public AdminController(IAdminService adminService, IAuditLogService auditLogService)
        {
            _adminService    = adminService;
            _auditLogService = auditLogService;
        }

        // POST: api/admin/login
        /// <summary>Authenticates the admin and returns a JWT.</summary>
        [HttpPost, Route("login")]
        [ResponseType(typeof(AdminResponseDto))]
        public IHttpActionResult Login([FromBody] AdminLoginDto request)
        {
            try { return Ok(_adminService.Login(request)); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/admin/diagnostic
        /// <summary>Returns diagnostic information about admin account setup (for debugging only).</summary>
        [HttpGet, Route("diagnostic")]
        [ResponseType(typeof(object))]
        public IHttpActionResult GetDiagnostic()
        {
            try
            {
                using (var context = new TrackMyGradeAPI.Data.ApplicationDbContext())
                {
                    var adminCount = context.Admins.Count();
                    var admins = context.Admins.Select(a => new { a.Id, a.Email, a.FirstName, a.LastName }).ToList();

                    return Ok(new
                    {
                        success = true,
                        message = "Admin account diagnostic information",
                        adminCount = adminCount,
                        admins = admins,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        // ── Teachers ──────────────────────────────────────────────────────

        // GET: api/admin/teachers
        /// <summary>Returns all teacher accounts.</summary>
        [HttpGet, Route("teachers")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(System.Collections.Generic.List<AdminTeacherDto>))]
        public IHttpActionResult GetTeachers()
        {
            try { return Ok(_adminService.GetAllTeachers()); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // POST: api/admin/teachers
        /// <summary>Creates a new teacher account. Returns the one-time activation token.</summary>
        [HttpPost, Route("teachers")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(AdminTeacherDto))]
        public IHttpActionResult CreateTeacher([FromBody] AdminCreateTeacherDto request)
        {
            try { return Created("", _adminService.CreateTeacher(request)); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // DELETE: api/admin/teachers/{id}
        /// <summary>Deletes a teacher account. Returns error if teacher has active classes or assignments.</summary>
        [HttpDelete, Route("teachers/{id:int}")]
        [TokenAuthorize("Admin")]
        public IHttpActionResult DeleteTeacher(int id)
        {
            try { _adminService.DeleteTeacher(id); return Ok(new { message = "Teacher deleted." }); }
            catch (KeyNotFoundException ex) { return NotFound(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // ── Students ──────────────────────────────────────────────────────

        // GET: api/admin/students
        /// <summary>Returns all student records.</summary>
        [HttpGet, Route("students")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(System.Collections.Generic.List<AdminStudentDto>))]
        public IHttpActionResult GetStudents()
        {
            try { return Ok(_adminService.GetAllStudents()); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // POST: api/admin/students
        /// <summary>Creates a new student account. Returns the one-time activation token.</summary>
        [HttpPost, Route("students")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(AdminStudentDto))]
        public IHttpActionResult CreateStudent([FromBody] AdminCreateStudentDto request)
        {
            try { return Created("", _adminService.CreateStudent(request)); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // PUT: api/admin/students/{id}
        /// <summary>Updates a student's personal details. Validates email and OMANG/Passport uniqueness.</summary>
        [HttpPut, Route("students/{id:int}")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(AdminStudentDto))]
        public IHttpActionResult UpdateStudent(int id, [FromBody] AdminUpdateStudentDto request)
        {
            try { return Ok(_adminService.UpdateStudent(id, request)); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // DELETE: api/admin/students/{id}
        /// <summary>Deletes a student record and all associated enrollments and submissions.</summary>
        [HttpDelete, Route("students/{id:int}")]
        [TokenAuthorize("Admin")]
        public IHttpActionResult DeleteStudent(int id)
        {
            try { _adminService.DeleteStudent(id); return Ok(new { message = "Student deleted." }); }
            catch (KeyNotFoundException ex) { return NotFound(); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // ── Courses ───────────────────────────────────────────────────────

        // GET: api/admin/courses
        [HttpGet, Route("courses")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(System.Collections.Generic.List<CourseDto>))]
        public IHttpActionResult GetCourses()
        {
            try { return Ok(_adminService.GetAllCourses()); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // POST: api/admin/courses
        [HttpPost, Route("courses")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(CourseDto))]
        public IHttpActionResult CreateCourse([FromBody] CreateCourseDto request)
        {
            try { return Created("", _adminService.CreateCourse(request)); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // ── Class Groups ──────────────────────────────────────────────────

        // GET: api/admin/class-groups
        [HttpGet, Route("class-groups")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(System.Collections.Generic.List<ClassGroupDto>))]
        public IHttpActionResult GetClassGroups()
        {
            try { return Ok(_adminService.GetAllClassGroups()); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // POST: api/admin/class-groups
        [HttpPost, Route("class-groups")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(ClassGroupDto))]
        public IHttpActionResult CreateClassGroup([FromBody] CreateClassGroupDto request)
        {
            try { return Created("", _adminService.CreateClassGroup(request)); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // POST: api/admin/class-groups/{id}/enroll
        /// <summary>Enrolls a student into a class group. Prevents duplicate enrollments.</summary>
        [HttpPost, Route("class-groups/{id:int}/enroll")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(ClassGroupDto))]
        public IHttpActionResult EnrollStudent(int id, [FromBody] EnrollStudentDto request)
        {
            try { return Ok(_adminService.EnrollStudent(id, request.StudentId)); }
            catch (KeyNotFoundException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // DELETE: api/admin/class-groups/{id}/enroll/{studentId}
        /// <summary>Removes a student from a class group.</summary>
        [HttpDelete, Route("class-groups/{id:int}/enroll/{studentId:int}")]
        [TokenAuthorize("Admin")]
        public IHttpActionResult UnenrollStudent(int id, int studentId)
        {
            try { _adminService.UnenrollStudent(id, studentId); return Ok(new { message = "Student unenrolled." }); }
            catch (KeyNotFoundException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // ── Audit Logs ─────────────────────────────────────────────────────

        // GET: api/admin/audit-logs
        /// <summary>Retrieve paginated audit logs with optional filtering (EntityType, Action, PerformedBy, date range).</summary>
        [HttpGet, Route("audit-logs")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(AuditLogPagedResponseDto))]
        public IHttpActionResult GetAuditLogs([FromUri] AuditLogFilterDto filter)
        {
            try
            {
                var result = _auditLogService.GetAuditLogs(filter);
                return Ok(result);
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // GET: api/admin/audit-logs/entity/{entityType}/{entityId}
        /// <summary>Retrieve all audit logs for a specific entity (e.g., Teacher with ID 5).</summary>
        [HttpGet, Route("audit-logs/entity/{entityType}/{entityId:int}")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(System.Collections.Generic.List<AuditLogDto>))]
        public IHttpActionResult GetAuditLogsByEntity(string entityType, int entityId)
        {
            try
            {
                var result = _auditLogService.GetAuditLogsByEntity(entityType, entityId);
                return Ok(result);
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }

        // GET: api/admin/audit-logs/user/{email}
        /// <summary>Retrieve all audit logs performed by a specific user (admin email).</summary>
        [HttpGet, Route("audit-logs/user/{email}")]
        [TokenAuthorize("Admin")]
        [ResponseType(typeof(System.Collections.Generic.List<AuditLogDto>))]
        public IHttpActionResult GetAuditLogsByUser(string email)
        {
            try
            {
                var result = _auditLogService.GetAuditLogsByUser(email);
                return Ok(result);
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return InternalServerError(ex); }
        }
    }
}
