using Microsoft.AspNetCore.Http;

namespace MroczwareFramework.Azure.File
{
    public interface IFileService
    {
        Task<AddAzureFileResponse> AddAzureFile(IFormFile file, string? userId = null);
        Task<AddAzureFileResponse> AddAzureFile(string fileName, string contentType, byte[] content, string? userId = null);
        Task<(Stream content, string contentType, string blobName)> GetAzureFile(string fileName);
    }
}
