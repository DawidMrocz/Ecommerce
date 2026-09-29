using Cart.Api.Data;
using Contracts.ApiModels.Basket;
using Microsoft.EntityFrameworkCore;

namespace Cart.Api.Services
{
    public class CartService : ICartService
    {
        private readonly CartDbContext _db;

        public CartService(CartDbContext db) => _db = db;

        public async Task<CartResponseDto> GetOrCreate(int userId)
        {
            var cart = await _db.Carts
                .AsTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (cart is not null) return new CartResponseDto()
            {
                Id = cart.Id,
                CreatedAt = DateTime.Now,
                UserId = cart.UserId
            };

            cart ??= new Models.Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();

            return new CartResponseDto()
            {
                Id = cart.Id,
                CreatedAt = DateTime.Now,
                UserId = cart.UserId
            };
        }
    }
}
