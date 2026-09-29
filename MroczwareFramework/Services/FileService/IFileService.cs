using Microsoft.AspNetCore.Http;

namespace MroczwareFramework.Services.FileService
{
    public interface IFileService
    {
        Task<(byte[] fileContent, string mimeType, string fileName)> Create(byte[] bytes, string fileName, Guid createdFileGuid, long? maxFileSize = null, string[]? allowedExtensions = null);
        Task<(byte[] fileContent, string mimeType, string fileName)> Create(IFormFile file, Guid guid, long? maxFileSize = null, string[]? allowedExtensions = null);
        void DeleteFromDisc(string fileName, Guid guid);
        Task<(byte[] fileContent, string mimeType, string fileName)> GetFilesFromDisc(string fileName, Guid guid);
    }
}
