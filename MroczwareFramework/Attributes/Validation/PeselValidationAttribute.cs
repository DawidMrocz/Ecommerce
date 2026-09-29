using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PeselValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success!;

            string? pesel = value as string;

            if (string.IsNullOrWhiteSpace(pesel))
                return new ValidationResult("Nie udane parsowanie");

            if (pesel.Length != 11)
                return new ValidationResult("PESEL musi mieć dokładnie 11 cyfr.");

            if (!pesel.All(char.IsDigit))
                return new ValidationResult("PESEL musi składać się tylko z cyfr.");

            if (!ValidatePesel(pesel))
                return new ValidationResult("PESEL jest nie poprawny");

            return ValidationResult.Success!;
        }

        private static bool ValidatePesel(string pesel)
        {
            int[] peselDigits = pesel.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray();
            int[] weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };
            int sum = 0;

            for (int i = 0; i <= weights.Length - 1; i++)
                sum += peselDigits[i] * weights[i];

            int reszta = sum % 10;

            string wynikString = reszta.ToString()[reszta.ToString().Length - 1].ToString();

            int wynik = 10 - int.Parse(wynikString);

            if (wynik == 10) wynik = 0;

            int controlValue = peselDigits[peselDigits.Length - 1];

            var outcome = wynik == controlValue;

            return outcome;
        }
    }
}
