using System;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Handlers;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    /// <summary>
    /// Teacher-scoped student read endpoints.
    /// Create / Update / Delete is handled by AdminController.
    /// </summary>
    [RoutePrefix("api/students")]
    [TokenAuthorize("Teacher")]
    public class StudentsController : ApiController
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: api/students
        /// <summary>Returns all students enrolled in any of the authenticated teacher's classes.</summary>
        [HttpGet, Route("")]
        [ResponseType(typeof(System.Collections.Generic.List<StudentResponseDto>))]
        public IHttpActionResult GetAll()
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_studentService.GetByTeacher(teacherId));
            }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }

        // GET: api/students/{id}
        /// <summary>Returns one student — only if they are enrolled in the teacher's class.</summary>
        [HttpGet, Route("{id:int}")]
        [ResponseType(typeof(StudentResponseDto))]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                int teacherId = Request.GetUserId();
                return Ok(_studentService.GetById(id, teacherId));
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (Exception ex) { ErrorLoggingConfig.LogError(ex); return BadRequest(ex.Message); }
        }
    }
}
