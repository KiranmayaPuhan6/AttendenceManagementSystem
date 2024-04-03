using AMS.Entities.Models.Domain.Entities;
using JwtAuthenticationManager.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
            IEnumerable<Account> users = null ;
            HttpClient client = new HttpClient();
            var response = client.GetAsync("https://localhost:7192/api/User/Validate/aec19c47-f1f4-4801-87a4-199f44443a5d");
            response.Wait();
            if (response.IsCompleted)
            {
                var result = response.Result;
                if (result.IsSuccessStatusCode)
                {
                    var data = result.Content.ReadAsStringAsync();
                    data.Wait();

                    users = JsonConvert.DeserializeObject(data.Result, typeof(List<Account>)) as List<Account>;
                }
            }
             var user = users?.Where(x => x.Email == tokenResponse.Email).FirstOrDefault();
            //var user = users?.Where(x => x.Email == tokenResponse.Email &&  BCrypt.Net.BCrypt.Verify(tokenResponse.RefreshToken, x.RefreshToken)).FirstOrDefault();

            //if (user == null || user.RefreshTokenExpires < DateTime.Now)
            //{
            //    return null;
            //}

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtTokenHandler.JWT_SECURITY_KEY);
            var expiresIn = DateTime.UtcNow.AddMinutes(JwtTokenHandler.JWT_TOKEN_VALIDITY_MINS);

            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, tokenResponse.Email),
                new Claim(ClaimTypes.Role, user.Role)
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
                ExpiresAt = expiresIn.ToLocalTime().TimeOfDay
            };
        }
    }
}

