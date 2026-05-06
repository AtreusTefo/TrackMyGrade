using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Student self-service: login and profile. Assessment self-scoring removed.</summary>
    [RoutePrefix("api/student-auth")]
    public class StudentAuthController : ApiController
    {
        private readonly IStudentAuthService _studentAuthService;

        public StudentAuthController(IStudentAuthService studentAuthService)
        {
            _studentAuthService = studentAuthService;
        }

        // POST: api/student-auth/login
        /// <summary>Authenticates an activated student and returns a JWT.</summary>
        [HttpPost, Route("login")]
        [ResponseType(typeof(StudentAuthResponseDto))]
        public IHttpActionResult Login([FromBody] StudentLoginDto request)
        {
            try { return Ok(_studentAuthService.Login(request)); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/student-auth/profile
        /// <summary>Returns the authenticated student's profile. Requires Authorization: Bearer token.</summary>
        [HttpGet, Route("profile")]
        [TokenAuthorize("Student")]
        [ResponseType(typeof(StudentAuthResponseDto))]
        public IHttpActionResult GetProfile()
        {
            try
            {
                // Token is validated by the filter; pull session token from header for DB lookup
                string token = Request.Headers.Authorization?.Parameter;
                return Ok(_studentAuthService.GetProfile(token));
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // NOTE: PUT /submit-assessments deliberately removed.
        // Students submit ASSIGNMENTS via StudentSubmissionController.
        // Teachers GRADE submissions via TeacherClassController.
    }
}
