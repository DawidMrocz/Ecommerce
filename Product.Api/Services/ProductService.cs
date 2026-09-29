using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Contracts;
using Contracts.ApiModels.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Product.Api.ApiModel;
using Product.Api.Data;
using System.Text;

namespace Product.Api.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext _db;
    private readonly IPublishEndpoint _publish;
    private readonly IConfiguration _configuration;

    public ProductService(ProductDbContext db, IPublishEndpoint publish, IConfiguration configuration)
    {
        _db = db;
        _publish = publish;
        _configuration = configuration;
    }

    public async Task<GetProductResponse?> GetByIdAsync(int id)
    {
        return await _db.Products
            .AsNoTracking()
            .Select(p => new GetProductResponse()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock
            })
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<int> CreateAsync(CreateProductRequest request)
    {
        var model = new Models.Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        _db.Products.Add(model);
        await _db.SaveChangesAsync();

        var createdEvt = new ProductCreated(
            model.Id,
            model.Name,
            model.Price,
            model.Stock,
            model.CreatedAt,
            model.UpdatedAt);

        await _publish.Publish(createdEvt);

        return model.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new Exception("Nie znaleziono produktu do usunięcia");

        var snapshot = new
        {
            existing.Id,
            existing.Name,
            existing.Price,
            existing.Stock,
            existing.CreatedAt,
            existing.UpdatedAt
        };

        _db.Products.Remove(existing);
        await _db.SaveChangesAsync();

        var deletedEvt = new ProductDeleted(
            snapshot.Id,
            snapshot.Name,
            snapshot.Price,
            snapshot.Stock,
            snapshot.CreatedAt,
            snapshot.UpdatedAt);

        await _publish.Publish(deletedEvt);
    }

    public async Task AddToBasketAsync(int id, int userId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new Exception("Nie znaleziono produktu do dodania");

        if (!product.IsAvailable)
            throw new Exception("Zabrakło produktu");

        product.Stock -= 1;

        _db.Products.Update(product);
        await _db.SaveChangesAsync();

        var addToBasketEvent = new AddToBasket(
            product.Id,
            1,
            userId);

        await _publish.Publish(addToBasketEvent);
    }

    public async Task AddPhotoAsync(AddProductPhotoRequest request, int id, int userId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new Exception("Nie znaleziono produktu do dodania");

        var keyVaultUri = "https://mroczwarekv22.vault.azure.net/";
        var secretClient = new Azure.Security.KeyVault.Secrets.SecretClient(new Uri(keyVaultUri), new Azure.Identity.DefaultAzureCredential());
        var secret = await secretClient.GetSecretAsync("BlobStorageConnection")
            ?? throw new Exception("BlobStorageConnection secret not found");

        BlobServiceClient blobServiceClient = new(secret.Value.Value);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["Blog:BlobContainer"]);
        await containerClient.CreateIfNotExistsAsync();

        StringBuilder builder = new(request.Photo.FileName);
        Guid fileGuid = Guid.NewGuid();
        builder.Append($"_{fileGuid}");
        builder.Append($"_{userId}");


        BlobClient blobClient = containerClient.GetBlobClient(builder.ToString());

        BlobHttpHeaders blobHttpHeaders = new();
        blobHttpHeaders.ContentType = request.Photo.ContentType;
        await blobClient.UploadAsync(request.Photo.OpenReadStream(), blobHttpHeaders);

        product.Photo = new Models.ProductPhoto()
        {

            Name = builder.ToString(),
            Guid = fileGuid
        };

        await _db.SaveChangesAsync();
    }

    public async Task<(Stream content, string contentType, string fileName)> GetPhotoAsync(int id, int userId)
    {
        var product = await _db.Products.Include(ph => ph.Photo).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new Exception("Nie znaleziono produktu do dodania");

        if (product.Photo is null)
            throw new Exception("Produkt nie ma zdjecia");

        var keyVaultUri = "https://mroczwarekv22.vault.azure.net/";
        var secretClient = new Azure.Security.KeyVault.Secrets.SecretClient(new Uri(keyVaultUri), new Azure.Identity.DefaultAzureCredential());
        var secret = await secretClient.GetSecretAsync("BlobStorageConnection")
            ?? throw new Exception("BlobStorageConnection secret not found");

        BlobServiceClient blobServiceClient = new(secret.Value.Value);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["Blog:BlobContainer"]);
        await containerClient.CreateIfNotExistsAsync();
        BlobClient blobClient = containerClient.GetBlobClient(product.Photo.Name);
        var response = await blobClient.DownloadContentAsync();
        Stream content = response.Value.Content.ToStream();
        string contentType = blobClient.GetProperties().Value.ContentType;
        string blobName = product.Photo.Name;

        return (content, contentType, blobName);
    }
}