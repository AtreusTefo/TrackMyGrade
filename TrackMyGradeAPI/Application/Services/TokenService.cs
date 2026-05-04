using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TrackMyGradeAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(int userId, string role, string email);
        ClaimsPrincipal ValidateToken(string token);
        (int UserId, string Role) ExtractClaims(string token);
    }

    public class TokenService : ITokenService
    {
        // Store this in App.config / environment in production
        private const string SecretKey   = "TrackMyGrade-JWT-Secret-Key-2026-Min32Chars!";
        private const string Issuer      = "TrackMyGradeAPI";
        private const string Audience    = "TrackMyGradeApp";
        private const int    ExpiryHours = 12;

        private SymmetricSecurityKey GetKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

        /// <summary>
        /// Generates a signed JWT containing userId, role and email.
        /// Expires after <see cref="ExpiryHours"/> hours.
        /// </summary>
        public string GenerateToken(int userId, string role, string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role,           role),
                new Claim(ClaimTypes.Email,          email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             Issuer,
                audience:           Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(ExpiryHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates a JWT and returns the ClaimsPrincipal, or null if invalid/expired.
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var handler    = new JwtSecurityTokenHandler();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = GetKey(),
                    ValidateIssuer           = true,
                    ValidIssuer              = Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = Audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };

                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts (UserId, Role) from a JWT string.
        /// Returns (-1, null) if the token is invalid.
        /// </summary>
        public (int UserId, string Role) ExtractClaims(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null) return (-1, null);

            var idClaim   = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

            return int.TryParse(idClaim, out int userId)
                ? (userId, roleClaim)
                : (-1, null);
        }
    }
}
