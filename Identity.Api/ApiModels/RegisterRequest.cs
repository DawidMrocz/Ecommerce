using System.ComponentModel.DataAnnotations;

namespace Identity.Api.ApiModels
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(50, ErrorMessage = "Imię może mieć maksymalnie 50 znaków.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(50, ErrorMessage = "Nazwisko może mieć maksymalnie 50 znaków.")]
        public string LastName { get; set; } = null!;
    }
}
