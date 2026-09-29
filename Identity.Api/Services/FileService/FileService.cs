using Microsoft.EntityFrameworkCore;
using MroczwareFramework.Extensions;
using MroczwareFramework.Helpers;

namespace MroczwareFramework.Services.FileService
{
    internal class FileService : IFileService
    {
        private string _fileSavePath;

        public FileService(IConfiguration configuration)
        {
            _fileSavePath = configuration.GetValue<string>("File:SaveFilePath")
                ?? throw new Exception("Scieżka do zapisu plików nie została ustawiona...");
        }

        /// <summary>
        /// Stworzenie pliku na podstawie byte[]
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="fileName"></param>
        /// <param name="createdFileGuid"></param>
        /// <param name="maxSizeInMB"></param>
        /// <param name="allowedExtensions"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<(byte[] fileContent, string mimeType, string fileName)> Create(byte[] bytes, string fileName, Guid createdFileGuid, long? maxSizeInMB = null, string[]? allowedExtensions = null)
        {
            if (bytes.Length <= 0) throw new Exception("No file");

            if (maxSizeInMB is not null)
                if (bytes.Length / 1024 / 1024 > maxSizeInMB)
                    throw new Exception($"Max allowed file size is {maxSizeInMB}Mb");

            if (allowedExtensions is not null)
                if (!allowedExtensions.Select(e => e.ToLower()).Contains(fileName[(fileName.LastIndexOf('.') + 1)..].ToLower()))
                    throw new Exception("Not allowed extension");

            if (!Directory.Exists(_fileSavePath))
                Directory.CreateDirectory(_fileSavePath);

            string filePath = GetFilePath(fileName, createdFileGuid);

            System.IO.File.WriteAllBytes(filePath, bytes);

            return (await System.IO.File.ReadAllBytesAsync(filePath), MimeTypeExtension.GetMimeType(fileName), fileName);
        }

        /// <summary>
        /// Stworzenie pliku na podstawie IFormFile
        /// </summary>
        /// <param name="file"></param>
        /// <param name="guid"></param>
        /// <param name="maxSizeInMB"></param>
        /// <param name="allowedExtensions"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<(byte[] fileContent, string mimeType, string fileName)> Create(IFormFile file, Guid guid, long? maxSizeInMB = null, string[]? allowedExtensions = null)
        {
            byte[]? content = await file.ConvertFileToByteArrayAsync();

            if (file.Length <= 0) throw new Exception("No file");

            if (maxSizeInMB is not null)
                if (file.Length / 1024 / 1024 > maxSizeInMB)
                    throw new Exception($"Max allowed file size is {maxSizeInMB}Mb");

            if (allowedExtensions is not null)
                if (!allowedExtensions.Select(e => e.ToLower()).Contains(file.FileName[(file.FileName.LastIndexOf('.') + 1)..].ToLower()))
                    throw new Exception("Not allowed extension");

            if (!Directory.Exists(_fileSavePath))
                Directory.CreateDirectory(_fileSavePath);

            string filePath = GetFilePath(file.FileName, guid);

            System.IO.File.WriteAllBytes(filePath, content);

            return (await System.IO.File.ReadAllBytesAsync(filePath), MimeTypeExtension.GetMimeType(file.FileName), file.FileName);
        }

        /// <summary>
        /// Usunięcie pliku z dysku
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="guid"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void DeleteFromDisc(string fileName, Guid guid)
        {
            string filePath = GetFilePath(fileName, guid);

            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            System.IO.File.Delete(filePath);
        }

        /// <summary>
        /// Pobranie pliku z dysku
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<(byte[] fileContent, string mimeType, string fileName)> GetFilesFromDisc(string fileName, Guid guid)
        {
            string filePath = GetFilePath(fileName, guid);

            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return (await System.IO.File.ReadAllBytesAsync(filePath), MimeTypeExtension.GetMimeType(fileName), fileName);
        }

        private string GetFilePath(string fileName, Guid guid) => Path.Combine(_fileSavePath, $"{guid}_{Path.GetFileName(fileName)}");
    }
}
