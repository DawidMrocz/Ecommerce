using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NIPValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string nip = value.ToString().Replace("-", "").Replace(" ", "");

                if (nip.Length != 10)
                    return new ValidationResult("NIP musi mieć dokładnie 10 cyfr.");

                if (!nip.All(char.IsDigit))
                    return new ValidationResult("NIP musi składać się tylko z cyfr.");

                if (!ValidateNIP(nip))
                    return new ValidationResult("NIP jest nie poprawny");

                return ValidationResult.Success;
            }
            else
            {
                return ValidationResult.Success;
            }
        }

        private static bool ValidateNIP(string nip)
        {
            int[] nipDigits = nip.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray();
            int[] weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
            int sum = 0;

            for (int i = 0; i <= weights.Length - 1; i++)
                sum += nipDigits[i] * weights[i];

            int controlValue = sum % 11;

            if (controlValue == 10)
                controlValue = 0;

            return controlValue == nipDigits[nipDigits.Length - 1];
        }
    }
}
