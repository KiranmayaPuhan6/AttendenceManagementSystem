namespace JwtAuthenticationManager.Utility
{
    public class TokenResponse
    {
        public string Email { get; set; }
        public string JwtToken { get; set; }
        public string? RefreshToken { get; set; }
        public TimeSpan? ExpiresAt { get; set; }
    }
}
