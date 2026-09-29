using Cart.Api.Models;

namespace Cart.Api.Services
{
    public interface ICartItemService
    {
        Task ChangeQuantityAsync(int cartItemId, int quantity, int userId);
        Task DeleteAsync(int cartItemId, int userId);
    }
}
