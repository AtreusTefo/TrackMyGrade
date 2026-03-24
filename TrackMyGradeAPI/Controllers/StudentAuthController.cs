using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Student self-service endpoints: login, profile and assessment submission.</summary>
    [RoutePrefix("api/student-auth")]
    public class StudentAuthController : ApiController
    {
        private readonly IStudentAuthService _studentAuthService;

        public StudentAuthController(IStudentAuthService studentAuthService)
        {
            _studentAuthService = studentAuthService;
        }

        // POST: api/student-auth/login
        /// <summary>Authenticates a student and returns an auth token.</summary>
        /// <param name="request">Student email and password.</param>
        /// <response code="200">Login successful. Returns the student profile with an auth token.</response>
        /// <response code="400">Invalid credentials.</response>
        [HttpPost]
        [Route("login")]
        [ResponseType(typeof(StudentAuthResponseDto))]
        public IHttpActionResult Login([FromBody] StudentLoginDto request)
        {
            try
            {
                var result = _studentAuthService.Login(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // GET: api/student-auth/profile
        /// <summary>Returns the authenticated student's profile including marks.</summary>
        /// <remarks>Pass the student token via the <c>X-StudentToken</c> request header.</remarks>
        /// <response code="200">Profile returned successfully.</response>
        /// <response code="400">Invalid or missing token.</response>
        [HttpGet]
        [Route("profile")]
        [ResponseType(typeof(StudentAuthResponseDto))]
        public IHttpActionResult GetProfile()
        {
            try
            {
                string token = Request.Headers
                    .FirstOrDefault(h => h.Key == "X-StudentToken").Value?.FirstOrDefault();

                if (string.IsNullOrEmpty(token))
                    return BadRequest("Authentication token is required");

                var result = _studentAuthService.GetProfile(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/student-auth/submit-assessments
        /// <summary>Allows the authenticated student to submit their own assessment scores.</summary>
        /// <param name="request">Assessment scores (each 0–20).</param>
        /// <remarks>Pass the student token via the <c>X-StudentToken</c> request header.</remarks>
        /// <response code="200">Assessments submitted successfully. Returns updated profile.</response>
        /// <response code="400">Validation failed or invalid token.</response>
        [HttpPut]
        [Route("submit-assessments")]
        [ResponseType(typeof(StudentAuthResponseDto))]
        public IHttpActionResult SubmitAssessments([FromBody] StudentSubmitAssessmentsDto request)
        {
            try
            {
                string token = Request.Headers
                    .FirstOrDefault(h => h.Key == "X-StudentToken").Value?.FirstOrDefault();

                if (string.IsNullOrEmpty(token))
                    return BadRequest("Authentication token is required");

                var result = _studentAuthService.SubmitAssessments(token, request);
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
