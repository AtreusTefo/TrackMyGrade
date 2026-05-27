using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using TrackMyGradeAPI.Infrastructure;

namespace TrackMyGradeAPI.Infrastructure.Security
{
    /// <summary>
    /// Custom authorization attribute that validates JWT tokens via ITokenService.
    /// Supports role-based access control (Admin, Teacher, Student).
    /// </summary>
    public class TokenAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="roles">Optional roles that are allowed to access the resource.</param>
        public TokenAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        /// <summary>Performs authorization by validating the JWT token and checking user roles.</summary>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // Allow Anonymous access if the attribute is present
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return;

            var request = actionContext.Request;
            var authHeader = request.Headers.Authorization;

            if (authHeader == null || authHeader.Scheme != "Bearer" || string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                actionContext.Response = request.CreateResponse(HttpStatusCode.Unauthorized, "Missing or invalid Authorization header.");
                return;
            }

            // Resolve the TokenService manually since Attributes are not part of the Dependency Injection pipeline
            var resolver = actionContext.ControllerContext.Configuration.DependencyResolver;
            var tokenService = (ITokenService)resolver.GetService(typeof(ITokenService));

            if (tokenService == null)
            {
                actionContext.Response = request.CreateResponse(HttpStatusCode.InternalServerError, "Security service unavailable.");
                return;
            }

            var principal = tokenService.ValidateToken(authHeader.Parameter);
            if (principal == null || (_allowedRoles.Any() && !_allowedRoles.Any(role => principal.IsInRole(role))))
            {
                actionContext.Response = request.CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized access or insufficient permissions.");
                return;
            }

            // Set the principal for the current request
            actionContext.RequestContext.Principal = principal;
        }
    }
}