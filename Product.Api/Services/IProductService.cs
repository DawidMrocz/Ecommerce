
using Contracts.ApiModels.Product;
using Product.Api.ApiModel;

namespace Product.Api.Services
{
    public interface IProductService
    {
        Task AddPhotoAsync(AddProductPhotoRequest request, int id, int userId);
        Task AddToBasketAsync(int id, int userId);
        Task<int> CreateAsync(CreateProductRequest request);
        Task DeleteAsync(int id);
        Task<GetProductResponse?> GetByIdAsync(int id);
        Task<(Stream content, string contentType, string fileName)> GetPhotoAsync(int id, int userId);
    }
}
