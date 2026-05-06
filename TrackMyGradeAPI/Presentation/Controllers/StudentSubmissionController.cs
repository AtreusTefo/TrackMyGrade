using System;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Student-facing assignment submission endpoints.</summary>
    [RoutePrefix("api/student")]
    [TokenAuthorize("Student")]
    public class StudentSubmissionController : ApiController
    {
        private readonly IAssignmentService _assignmentService;

        public StudentSubmissionController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        // GET: api/student/assignments
        /// <summary>
        /// Returns all assignments for the classes the student is enrolled in.
        /// Each item includes the student's own submission status.
        /// </summary>
        [HttpGet, Route("assignments")]
        [ResponseType(typeof(System.Collections.Generic.List<AssignmentResponseDto>))]
        public IHttpActionResult GetMyAssignments()
        {
            try
            {
                int studentId = Request.GetUserId();
                return Ok(_assignmentService.GetStudentAssignments(studentId));
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // POST: api/student/assignments/{id}/submit
        /// <summary>
        /// Submits the student's answer for an assignment.
        /// Only allowed once per assignment. Late submissions are flagged automatically.
        /// </summary>
        [HttpPost, Route("assignments/{id:int}/submit")]
        [ResponseType(typeof(SubmissionResponseDto))]
        public IHttpActionResult Submit(int id, [FromBody] SubmissionCreateDto request)
        {
            try
            {
                int studentId = Request.GetUserId();
                return Created("", _assignmentService.SubmitAssignment(id, studentId, request));
            }
            catch (UnauthorizedAccessException ex)  { return Unauthorized(); }
            catch (InvalidOperationException ex)     { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/student/submissions
        /// <summary>
        /// Returns all of the student's own submissions including scores and teacher feedback.
        /// </summary>
        [HttpGet, Route("submissions")]
        [ResponseType(typeof(System.Collections.Generic.List<SubmissionResponseDto>))]
        public IHttpActionResult GetMySubmissions()
        {
            try
            {
                int studentId = Request.GetUserId();
                return Ok(_assignmentService.GetMySubmissions(studentId));
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }
    }
}
