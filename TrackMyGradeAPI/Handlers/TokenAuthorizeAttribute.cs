using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TrackMyGradeAPI.Services;

namespace TrackMyGradeAPI.Handlers
{
    /// <summary>
    /// Action filter that enforces JWT authentication and optional role authorization.
    /// Usage: [TokenAuthorize] or [TokenAuthorize("Admin")] or [TokenAuthorize("Teacher","Admin")]
    /// Reads the Authorization header: "Bearer &lt;token&gt;"
    /// Sets Request.Properties["UserId"] and Request.Properties["UserRole"] for downstream use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TokenAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        /// <summary>Allows any authenticated user (any valid role).</summary>
        public TokenAuthorizeAttribute() : this(Array.Empty<string>()) { }

        /// <summary>Allows only users whose role matches one of the provided roles.</summary>
        /// <param name="roles">The allowed roles. If empty, any authenticated user is allowed.</param>
        public TokenAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles ?? Array.Empty<string>();
        }

        /// <summary>Enforces JWT token validation and optional role authorization before action execution.</summary>
        /// <param name="actionContext">The action context for the current request.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var request = actionContext.Request;

            // ── 1. Extract Bearer token ─────────────────────────────────
            string authHeader = request.Headers.Authorization?.Parameter;
            if (string.IsNullOrEmpty(authHeader) ||
                !request.Headers.Authorization.Scheme.Equals("Bearer",
                    StringComparison.OrdinalIgnoreCase))
            {
                actionContext.Response = Unauthorized(request, "Missing or invalid Authorization header. Please log in again.");
                return;
            }
            
            // Trim token to handle potential whitespace/transmission issues
            authHeader = authHeader?.Trim() ?? string.Empty;

            // ── 2. Validate token ───────────────────────────────────────
            var tokenService = new TokenService();          // lightweight; no DI needed for attribute
            var (userId, role) = tokenService.ExtractClaims(authHeader);

            if (userId < 0 || role == null)
            {
                actionContext.Response = Unauthorized(request, "Token is invalid or has expired.");
                return;
            }

            // ── 3. Role check ───────────────────────────────────────────
            if (_allowedRoles.Length > 0 &&
                !_allowedRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
            {
                actionContext.Response = Forbidden(request,
                    $"Role '{role}' is not permitted to access this resource.");
                return;
            }

            // ── 4. Expose identity to the controller ────────────────────
            request.Properties["UserId"]   = userId;
            request.Properties["UserRole"] = role;

            base.OnActionExecuting(actionContext);
        }

        // ── Helpers ─────────────────────────────────────────────────────

        private static HttpResponseMessage Unauthorized(HttpRequestMessage req, string message) =>
            req.CreateErrorResponse(HttpStatusCode.Unauthorized, message);

        private static HttpResponseMessage Forbidden(HttpRequestMessage req, string message) =>
            req.CreateErrorResponse(HttpStatusCode.Forbidden, message);
    }

    /// <summary>Extension methods to safely read identity from request properties.</summary>
    public static class RequestIdentityExtensions
    {
        /// <summary>Returns the authenticated user's ID from a guarded action.</summary>
        public static int GetUserId(this HttpRequestMessage request)
        {
            return request.Properties.TryGetValue("UserId", out var val) && val is int id
                ? id : throw new UnauthorizedAccessException("UserId not found in request context.");
        }

        /// <summary>Returns the authenticated user's role from a guarded action.</summary>
        public static string GetUserRole(this HttpRequestMessage request)
        {
            return request.Properties.TryGetValue("UserRole", out var val) && val is string role
                ? role : throw new UnauthorizedAccessException("UserRole not found in request context.");
        }
    }
}
