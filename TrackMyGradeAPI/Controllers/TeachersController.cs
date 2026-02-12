using System;
using System.Web.Http;
using FluentValidation;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    [RoutePrefix("api/teachers")]
    public class TeachersController : ApiController
    {
        private readonly ITeacherService _teacherService;
        private readonly IValidator<TeacherRegisterDto> _registerValidator;
        private readonly IValidator<TeacherLoginDto> _loginValidator;

        public TeachersController(ITeacherService teacherService, 
            IValidator<TeacherRegisterDto> registerValidator,
            IValidator<TeacherLoginDto> loginValidator)
        {
            _teacherService = teacherService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        // POST: api/teachers/register
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register([FromBody] TeacherRegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = _registerValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return BadRequest(ModelState);
            }

            try
            {
                var result = _teacherService.Register(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
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

            var validationResult = _loginValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return BadRequest(ModelState);
            }

            try
            {
                var result = _teacherService.Login(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
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
                return BadRequest(ex.Message);
            }
        }
    }
}
