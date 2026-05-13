using System;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>Teacher-facing assignment and class management endpoints.</summary>
    [RoutePrefix("api/teacher")]
    [TokenAuthorize("Teacher")]
    public class TeacherClassController : ApiController
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IStudentService    _studentService;

        /// <summary>Initializes a new instance of the TeacherClassController class.</summary>
        /// <param name="assignmentService">The assignment service dependency.</param>
        /// <param name="studentService">The student service dependency.</param>
        public TeacherClassController(IAssignmentService assignmentService, IStudentService studentService)
        {
            _assignmentService = assignmentService;
            _studentService    = studentService;
        }

        // GET: api/teacher/my-students
        /// <summary>Returns all students enrolled in any of this teacher's classes.</summary>
        [HttpGet, Route("my-students")]
        [ResponseType(typeof(System.Collections.Generic.List<StudentResponseDto>))]
        public IHttpActionResult GetMyStudents()
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_studentService.GetByTeacher(teacherId));
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/teacher/students/{id}
        /// <summary>Returns one student — only if they are enrolled in the teacher's class.</summary>
        [HttpGet, Route("students/{id:int}")]
        [ResponseType(typeof(StudentResponseDto))]
        public IHttpActionResult GetStudent(int id)
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_studentService.GetById(id, teacherId));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/teacher/assignments
        /// <summary>Returns all assignments created by this teacher.</summary>
        [HttpGet, Route("assignments")]
        [ResponseType(typeof(System.Collections.Generic.List<AssignmentResponseDto>))]
        public IHttpActionResult GetAssignments()
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_assignmentService.GetMyAssignments(teacherId));
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // POST: api/teacher/assignments
        /// <summary>Creates a new assignment for one of the teacher's class groups.</summary>
        [HttpPost, Route("assignments")]
        [ResponseType(typeof(AssignmentResponseDto))]
        public IHttpActionResult CreateAssignment([FromBody] AssignmentCreateDto request)
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Created("", _assignmentService.CreateAssignment(teacherId, request));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/teacher/assignments/{id}/submissions
        /// <summary>Returns all student submissions for a specific assignment.</summary>
        [HttpGet, Route("assignments/{id:int}/submissions")]
        [ResponseType(typeof(System.Collections.Generic.List<SubmissionResponseDto>))]
        public IHttpActionResult GetSubmissions(int id)
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_assignmentService.GetSubmissions(id, teacherId));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // PUT: api/teacher/submissions/{id}/grade
        /// <summary>Grades a student submission and adds feedback.</summary>
        [HttpPut, Route("submissions/{id:int}/grade")]
        [ResponseType(typeof(SubmissionResponseDto))]
        public IHttpActionResult GradeSubmission(int id, [FromBody] GradingDto request)
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_assignmentService.GradeSubmission(id, teacherId, request));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (ArgumentException ex)           { return BadRequest(ex.Message); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }
    }
}
