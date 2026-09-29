using MroczwareFramework.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.ApiModels.User
{
    [TrimAllStrings]
    public class UserForgotPassword
    {
        [Required]
        [EmailAddress(ErrorMessage = "Nie poprawny format adresu e-mail")]
        public string Email { get; set; } = null!;
    }
}
