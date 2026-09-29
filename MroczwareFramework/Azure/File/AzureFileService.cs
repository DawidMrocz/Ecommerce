using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace MroczwareFramework.Azure.File
{
    public class AddAzureFileResponse
    {
        public string Name { get; set; } = null!;
        public Guid FileGuid { get; set; }
        public string? FileUserId { get; set; }
    }

    internal class FileService : IFileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<AddAzureFileResponse> AddAzureFile(IFormFile file, string? userId = null)
        {
            BlobServiceClient blobServiceClient = new(_configuration.GetConnectionString("DefaultConnection"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("wintersportblobs");
            await containerClient.CreateIfNotExistsAsync();

            StringBuilder builder = new(file.FileName);
            Guid fileGuid = Guid.NewGuid();
            builder.Append($"_{fileGuid}");
            if (userId is not null)
                builder.Append($"_{userId}");

            BlobClient blobClient = containerClient.GetBlobClient(builder.ToString());

            BlobHttpHeaders blobHttpHeaders = new();
            blobHttpHeaders.ContentType = file.ContentType;
            await blobClient.UploadAsync(file.OpenReadStream(), blobHttpHeaders);

            return new AddAzureFileResponse
            {
                Name = file.Name,
                FileGuid = fileGuid,
                FileUserId = userId
            };
        }

        public async Task<AddAzureFileResponse> AddAzureFile(string fileName, string contentType, byte[] content, string? userId = null)
        {
            BlobServiceClient blobServiceClient = new(_configuration.GetConnectionString("DefaultConnection"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("profilephotos");
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            StringBuilder builder = new(fileName);
            Guid fileGuid = Guid.NewGuid();
            builder.Append($"_{fileGuid}");
            if (userId is not null)
                builder.Append($"_{userId}");

            BlobHttpHeaders blobHttpHeaders = new();
            blobHttpHeaders.ContentType = contentType;
            await blobClient.UploadAsync(new MemoryStream(content), blobHttpHeaders);

            return new AddAzureFileResponse
            {
                Name = fileName,
                FileGuid = fileGuid,
                FileUserId = userId
            };
        }

        public async Task<(Stream content, string contentType, string blobName)> GetAzureFile(string fileName)
        {
            BlobServiceClient blobServiceClient = new(_configuration.GetConnectionString("DefaultConnection"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("profilephotos");
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadContentAsync();
            Stream content = response.Value.Content.ToStream();
            string contentType = blobClient.GetProperties().Value.ContentType;
            string blobName = fileName;

            return (content, contentType, blobName);
        }
    }
}
