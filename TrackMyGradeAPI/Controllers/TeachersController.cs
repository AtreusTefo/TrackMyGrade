using System;
using System.Web.Http;
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
        [HttpPost]
        [Route("register")]
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
        [HttpPost]
        [Route("login")]
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
        [HttpGet]
        [Route("{id}")]
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
