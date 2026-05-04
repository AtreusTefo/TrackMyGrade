using System;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Teacher authentication. Teacher accounts are created by AdminController.</summary>
    [RoutePrefix("api/teachers")]
    public class TeachersController : ApiController
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        // POST: api/teachers/login
        /// <summary>Authenticates an activated teacher and returns a JWT.</summary>
        [HttpPost, Route("login")]
        [ResponseType(typeof(TeacherResponseDto))]
        public IHttpActionResult Login([FromBody] TeacherLoginDto request)
        {
            try { return Ok(_teacherService.Login(request)); }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/teachers/{id}
        /// <summary>Returns a teacher's profile by ID.</summary>
        [HttpGet, Route("{id:int}")]
        [TokenAuthorize("Admin", "Teacher")]
        [ResponseType(typeof(TeacherResponseDto))]
        public IHttpActionResult GetById(int id)
        {
            try { return Ok(_teacherService.GetById(id)); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }
    }
}
