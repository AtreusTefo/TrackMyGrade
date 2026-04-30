using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Logging;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : ApiController
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: api/students
        /// <summary>Returns all students belonging to the requesting teacher.</summary>
        /// <remarks>Pass the teacher ID via the <c>X-TeacherId</c> request header. Defaults to teacher 1 if omitted.</remarks>
        /// <response code="200">List of students returned successfully.</response>
        /// <response code="400">An error occurred while retrieving students.</response>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(IEnumerable<StudentResponseDto>))]
        public IHttpActionResult GetAll()
        {
            try
            {
                // Get teacher ID from header or query - for now, we'll use a default
                // In a real app, this would come from authentication context
                int teacherId = 1; // Default for demo; should be from token/context

                if (int.TryParse(Request.Headers.FirstOrDefault(h => h.Key == "X-TeacherId").Value?.FirstOrDefault() ?? "", out int headerId))
                    teacherId = headerId;

                var result = _studentService.GetAllByTeacher(teacherId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // GET: api/students/{id}
        /// <summary>Returns a single student by ID.</summary>
        /// <param name="id">The student's unique identifier.</param>
        /// <response code="200">Student found and returned.</response>
        /// <response code="400">Student not found or does not belong to this teacher.</response>
        [HttpGet]
        [Route("{id:int}")]
        [ResponseType(typeof(StudentResponseDto))]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                int teacherId = 1; // Default for demo
                if (int.TryParse(Request.Headers.FirstOrDefault(h => h.Key == "X-TeacherId").Value?.FirstOrDefault() ?? "", out int headerId))
                    teacherId = headerId;

                var result = _studentService.GetById(id, teacherId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // POST: api/students
        /// <summary>Creates a new student and calculates their performance metrics.</summary>
        /// <param name="request">Student details. Assessment scores must be between 0 and 20.</param>
        /// <response code="201">Student created successfully. Returns the created student with computed totals.</response>
        /// <response code="400">Validation failed or an error occurred.</response>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(StudentResponseDto))]
        public IHttpActionResult Create([FromBody] StudentCreateDto request)
        {
            try
            {
                int teacherId = 1; // Default for demo
                if (int.TryParse(Request.Headers.FirstOrDefault(h => h.Key == "X-TeacherId").Value?.FirstOrDefault() ?? "", out int headerId))
                    teacherId = headerId;

                var result = _studentService.Create(request, teacherId);
                return Created($"api/students/{result.Id}", result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/students/{id}
        /// <summary>Updates an existing student's details and recalculates performance metrics.</summary>
        /// <param name="id">The student's unique identifier.</param>
        /// <param name="request">Updated student details.</param>
        /// <response code="200">Student updated successfully.</response>
        /// <response code="400">Validation failed, student not found, or an error occurred.</response>
        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(StudentResponseDto))]
        public IHttpActionResult Update(int id, [FromBody] StudentUpdateDto request)
        {
            try
            {
                int teacherId = 1; // Default for demo
                if (int.TryParse(Request.Headers.FirstOrDefault(h => h.Key == "X-TeacherId").Value?.FirstOrDefault() ?? "", out int headerId))
                    teacherId = headerId;

                var result = _studentService.Update(id, request, teacherId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/students/{id}
        /// <summary>Deletes a student by ID.</summary>
        /// <param name="id">The student's unique identifier.</param>
        /// <response code="200">Student deleted successfully.</response>
        /// <response code="400">Student not found or an error occurred.</response>
        [HttpDelete]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                int teacherId = 1; // Default for demo
                if (int.TryParse(Request.Headers.FirstOrDefault(h => h.Key == "X-TeacherId").Value?.FirstOrDefault() ?? "", out int headerId))
                    teacherId = headerId;

                _studentService.Delete(id, teacherId);
                return Ok(new { message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                ErrorLoggingConfig.LogError(ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
