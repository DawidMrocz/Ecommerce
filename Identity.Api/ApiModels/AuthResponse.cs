namespace Identity.Api.ApiModels
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresIn { get; set; }
        public DateTime RefreshTokenExpiresIn { get; set; }
    }
}