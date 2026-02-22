using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using FluentValidation;
using TrackMyGradeAPI.DTOs;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Controllers
{
    [RoutePrefix("api/students")]
    public class StudentsController : ApiController
    {
        private readonly IStudentService _studentService;
        private readonly IValidator<StudentCreateDto> _createValidator;
        private readonly IValidator<StudentUpdateDto> _updateValidator;

        public StudentsController(IStudentService studentService,
            IValidator<StudentCreateDto> createValidator,
            IValidator<StudentUpdateDto> updateValidator)
        {
            _studentService = studentService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // GET: api/students
        [HttpGet]
        [Route("")]
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
                return BadRequest(ex.Message);
            }
        }

        // GET: api/students/{id}
        [HttpGet]
        [Route("{id:int}")]
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
                return BadRequest(ex.Message);
            }
        }

        // POST: api/students
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] StudentCreateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return BadRequest(ModelState);
            }

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
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/students/{id}
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] StudentUpdateDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return BadRequest(ModelState);
            }

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
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/students/{id}
        [HttpDelete]
        [Route("{id:int}")]
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
                return BadRequest(ex.Message);
            }
        }
    }
}
