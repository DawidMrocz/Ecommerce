using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MroczwareFramework.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PasswordValidationAttribute : ValidationAttribute
    {
        public PasswordValidationAttribute() { }
        protected override ValidationResult? IsValid(object? value,
                                             ValidationContext validationContext)
        {
            IConfiguration? configuration = validationContext.GetService(typeof(IConfiguration)) as IConfiguration
                ?? throw new Exception("Service not found");

            string? paswordLenght = configuration["Password:Lenght"];
            string? paswordContainNumbers = configuration["Password:ContainNumbers"];
            string? paswordContainSigns = configuration["Password:ContainSpecialSigns"];
            string? paswordContainBigLetters = configuration["Password:ContainBigLetters"];
            string? paswordContainSmallLetters = configuration["Password:ContainSmallLetters"];

            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Password is required.");

            string passwordValue = value.ToString()!;

            if (!string.IsNullOrWhiteSpace(paswordLenght) && passwordValue.Length < int.Parse(paswordLenght)) return new ValidationResult($"Password tmust have at least {paswordLenght} length");

            if (!string.IsNullOrWhiteSpace(paswordContainNumbers) && bool.Parse(paswordContainNumbers))
                if (!new Regex(@"\d").IsMatch(passwordValue)) return new ValidationResult("Password must contain numbers");

            if (!string.IsNullOrWhiteSpace(paswordContainBigLetters) && bool.Parse(paswordContainBigLetters))
                if (!new Regex(@"[A-Z]").IsMatch(passwordValue)) return new ValidationResult("Password must contain big letters");

            if (!string.IsNullOrWhiteSpace(paswordContainSmallLetters) && bool.Parse(paswordContainSmallLetters))
                if (!new Regex(@"[a-z]").IsMatch(passwordValue)) return new ValidationResult("Password must contain small letters");

            if (!string.IsNullOrWhiteSpace(paswordContainSigns) && bool.Parse(paswordContainSigns))
                if (!new Regex(@"[^a-zA-Z0-9\s]").IsMatch(passwordValue)) return new ValidationResult("Password must contain specjal signs");

            return ValidationResult.Success;
        }
    }
}
