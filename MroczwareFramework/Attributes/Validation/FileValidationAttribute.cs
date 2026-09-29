using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MroczwareFramework.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FileValidationAttribute : ValidationAttribute
    {
        private int FileSize { get; set; }
        private string? AllowedExtensons { get; set; }
        public FileValidationAttribute(int fileSize = 0, string? allowedExtensons = null)
        {
            FileSize = fileSize;
            AllowedExtensons = allowedExtensons;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            IFormFile? file = value as IFormFile;
            if (file is null) return new ValidationResult("Lack of file");

            MemoryStream stream = new();
            file.CopyTo(stream);

            if (FileSize != 0)
            {
                long fileSize = file.Length / 1024 / 1024;
                if (fileSize > FileSize) return new ValidationResult($"Max allowed file size is {FileSize}Mb");
            }

            if (AllowedExtensons is not null)
            {
                string fileExtension = file.FileName[(file.FileName.LastIndexOf('.') + 1)..].ToLower();
                if (!AllowedExtensons.Contains(fileExtension)) return new ValidationResult("Not allowed extension");
            }

            return ValidationResult.Success;
        }
    }
}
