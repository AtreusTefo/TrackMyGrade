using System;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Account activation endpoints for new teachers and students.</summary>
    [RoutePrefix("api/auth")]
    public class ActivationController : ApiController
    {
        private readonly IActivationService _activationService;

        /// <summary>Initializes a new instance of the ActivationController class.</summary>
        /// <param name="activationService">The activation service dependency.</param>
        public ActivationController(IActivationService activationService)
        {
            _activationService = activationService;
        }

        // GET: api/auth/check-activation?token=xxx&role=Teacher
        /// <summary>
        /// Checks the status of an activation token.
        /// Returns the user's name and email so the Angular page can greet them.
        /// </summary>
        [HttpGet, Route("check-activation")]
        [ResponseType(typeof(ActivationStatusDto))]
        public IHttpActionResult CheckActivation(string token, string role)
        {
            try { return Ok(_activationService.CheckStatus(token, role)); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // POST: api/auth/activate
        /// <summary>
        /// Activates a new teacher or student account.
        /// Validates the one-time token, hashes the chosen password, and returns a JWT
        /// so the user is immediately logged in after activation.
        /// </summary>
        [HttpPost, Route("activate")]
        [ResponseType(typeof(ActivationResponseDto))]
        public IHttpActionResult Activate([FromBody] ActivateAccountDto request)
        {
            try { return Ok(_activationService.Activate(request)); }
            catch (ArgumentException ex)          { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex)  { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }
    }
}
