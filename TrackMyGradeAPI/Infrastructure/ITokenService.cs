using System.Security.Claims;

namespace TrackMyGradeAPI.Infrastructure
{
    /// <summary>
    /// Interface for JWT security token operations.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a signed JWT token with user claims.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="role">The role of the user (Admin, Teacher, Student).</param>
        /// <param name="email">The email of the user.</param>
        /// <returns>A signed JWT token string.</returns>
        string GenerateToken(int userId, string role, string email);

        /// <summary>Validates a JWT token and returns the principal if valid.</summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>The claims principal if valid, or null.</returns>
        ClaimsPrincipal ValidateToken(string token);

        /// <summary>Extracts user ID and role claims from a JWT token.</summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>A tuple containing the user ID and role.</returns>
        (int UserId, string Role) ExtractClaims(string token);
    }
}