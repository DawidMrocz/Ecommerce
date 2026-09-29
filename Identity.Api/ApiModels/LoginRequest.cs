using System.ComponentModel.DataAnnotations;

namespace Identity.Api.ApiModels
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        public string Password { get; set; } = null!;
    }
}
