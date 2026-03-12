using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentValidation;

namespace TrackMyGradeAPI.Handlers
{
    public class FluentValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var scope = actionContext.Request.GetDependencyScope();

            foreach (var argument in actionContext.ActionArguments)
            {
                if (argument.Value == null)
                {
                    actionContext.ModelState.AddModelError(argument.Key, "Request body is required.");
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.Value.GetType());
                var validator = scope.GetService(validatorType) as IValidator;
                if (validator == null)
                {
                    continue;
                }

                var validationResult = validator.Validate(new ValidationContext<object>(argument.Value));
                if (validationResult.IsValid)
                {
                    continue;
                }

                foreach (var error in validationResult.Errors)
                {
                    actionContext.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }

            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    actionContext.ModelState);
            }
        }
    }
}
