using MroczwareFramework.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.ApiModels.User
{
    [TrimAllStrings]
    public class CreateUserRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nie prawidłowy e-mail")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [PasswordValidation]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Hasło jest wymagane")]
        [PasswordValidation]
        public string ConfirmPassword { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult(
                    $"Hasła się nie zgadzaja",
                    [nameof(Password), nameof(ConfirmPassword)]);
            }
        }
    }
}