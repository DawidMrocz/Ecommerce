using MroczwareFramework.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.ApiModels.User
{
    [TrimAllStrings]
    public class ChangePasswordRequest : IValidatableObject
    {
        [PasswordValidation]
        public required string NewPassword { get; set; }
        public required string RepeatPassword { get; set; }
        public required string OldPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!NewPassword.Equals(RepeatPassword))
            {
                yield return new ValidationResult(
                    $"Passwords are not the same",
                    new[] { nameof(RepeatPassword) });
            }
        }
    }
}
