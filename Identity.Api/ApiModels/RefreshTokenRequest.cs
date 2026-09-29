using System.ComponentModel.DataAnnotations;

namespace Identity.Api.ApiModels
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}
