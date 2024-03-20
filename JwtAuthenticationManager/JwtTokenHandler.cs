using AMS.Services.Utility.HelperMethods;
using JwtAuthenticationManager.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthenticationManager
{
    public class JwtTokenHandler
    {
        private readonly ILogger<JwtTokenHandler> _logger;
        internal const string JWT_SECURITY_KEY = "eyJhbGciOiJIUzI1NiJ9.eyJSb2xlIjoiQWRtaW4iLCJJc3N1ZXIiOiJJc3N1ZXIiLCJVc2VybmFtZSI6IkphdmFJblVzZSIsImV4cCI6MTY2NDE5OTEyOCwiaWF0IjoxNjY0MTk5MTI4fQ.FZGfQZKyicxnX5NkwWXzPushKnbT9i_3NNThj-sYpUQ";
        internal const int JWT_TOKEN_VALIDITY_MINS = 3;
        private List<Account> users;

        public JwtTokenHandler(ILogger<JwtTokenHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthenticationResponse> GenerateToken(AuthenticationRequest authenticationRequest)
        {
            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} started");
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://localhost:7192/api/User/Validate/aec19c47-f1f4-4801-87a4-199f44443a5d");

                var result = response.Content;
                if (response.IsSuccessStatusCode)
                {
                    var data = result.ReadAsStringAsync();
                    data.Wait();

                    this.users = JsonConvert.DeserializeObject(data.Result, typeof(List<Account>)) as List<Account>;
                }
            if (string.IsNullOrWhiteSpace(authenticationRequest.Email) || string.IsNullOrWhiteSpace(authenticationRequest.Password))
            {
                return null;
            }
            var user = users.Where(x => x.Email == authenticationRequest.Email && BCrypt.Net.BCrypt.Verify(authenticationRequest.Password, x.Password)).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(JWT_TOKEN_VALIDITY_MINS);
            var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, authenticationRequest.Email),
                new Claim(ClaimTypes.Role, user.Role)
            });
            var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature
                );
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signingCredentials
            };
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            _logger.LogDebug($"{MethodNameExtensionHelper.GetCurrentMethod()} in {this.GetType().Name} ended");
            return new AuthenticationResponse
            {
                Email = user.Email,
                ExpiresAt = tokenExpiryTimeStamp.ToLocalTime().TimeOfDay,
                JwtToken = token,
                UserId = user.UserID,
            };
        }
    }
}
