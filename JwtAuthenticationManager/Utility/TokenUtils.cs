using AMS.Entities.Models.Domain.Entities;
using JwtAuthenticationManager.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthenticationManager.Utility
{
    public static class TokenUtils
    {
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static TokenResponse GenerateAccessTokenFromRefreshToken(TokenResponse tokenResponse)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtTokenHandler.JWT_SECURITY_KEY);
            var expiresIn = DateTime.UtcNow.AddMinutes(JwtTokenHandler.JWT_TOKEN_VALIDITY_MINS);

            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, tokenResponse.Email),
                new Claim(ClaimTypes.Role, tokenResponse.Role)
            });
            var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                );
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = expiresIn,
                SigningCredentials = signingCredentials
            };

            var token = tokenHandler.CreateToken(securityTokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return new TokenResponse
            {
                Email = tokenResponse.Email,
                JwtToken = jwtToken,
                RefreshToken = tokenResponse.RefreshToken,
                Role = tokenResponse.Role,
                ExpiresAt = expiresIn.ToLocalTime().TimeOfDay
            };
        }
    }
}

