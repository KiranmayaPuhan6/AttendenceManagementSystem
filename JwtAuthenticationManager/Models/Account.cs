using System.ComponentModel.DataAnnotations;

namespace JwtAuthenticationManager.Models
{
    public class Account
    {
        public int UserID { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
