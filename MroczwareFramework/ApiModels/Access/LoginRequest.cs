using MroczwareFramework.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.ApiModels.Access
{
    [TrimAllStrings]
    public class LoginRequest
    {
        [Required(ErrorMessage = "Adres e-mail jest wymagany")]
        [EmailAddress(ErrorMessage = "Nie poprawny format adresu e-mail")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Hasło jest wymagane")]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; } = true;
    }
}
