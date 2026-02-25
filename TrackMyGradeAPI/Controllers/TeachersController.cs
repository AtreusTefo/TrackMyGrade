using System;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    [RoutePrefix("api/teachers")]
    public class TeachersController : ApiController
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        // POST: api/teachers/register
        /// <summary>Registers a new teacher account.</summary>
        /// <param name="request">Teacher registration details including name, email, subject and password.</param>
        /// <response code="200">Registration successful. Returns the teacher profile with an auth token.</response>
        /// <response code="400">Validation failed or email already in use.</response>
        [HttpPost]
        [Route("register")]
        [ResponseType(typeof(TeacherResponseDto))]
        public IHttpActionResult Register([FromBody] TeacherRegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = _teacherService.Register(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // POST: api/teachers/login
        /// <summary>Authenticates a teacher and returns an auth token.</summary>
        /// <param name="request">Teacher email and password.</param>
        /// <response code="200">Login successful. Returns the teacher profile with an auth token.</response>
        /// <response code="400">Invalid credentials.</response>
        [HttpPost]
        [Route("login")]
        [ResponseType(typeof(TeacherResponseDto))]
        public IHttpActionResult Login([FromBody] TeacherLoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = _teacherService.Login(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // GET: api/teachers/{id}
        /// <summary>Returns a teacher's profile by ID.</summary>
        /// <param name="id">The teacher's unique identifier.</param>
        /// <response code="200">Teacher profile returned.</response>
        /// <response code="400">Teacher not found.</response>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(TeacherResponseDto))]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var result = _teacherService.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
