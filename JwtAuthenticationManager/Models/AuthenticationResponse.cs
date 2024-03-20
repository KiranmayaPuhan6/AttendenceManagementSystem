using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthenticationManager.Models
{
    public class AuthenticationResponse
    {
        public string Email { get; set; }
        public string JwtToken { get; set; }
        public TimeSpan ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string? RefreshToken { get; set; }
    }
}
