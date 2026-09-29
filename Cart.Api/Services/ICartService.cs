using Contracts.ApiModels.Basket;

namespace Cart.Api.Services
{
    public interface ICartService
    {
        Task<CartResponseDto> GetOrCreate(int userId);
    }
}
