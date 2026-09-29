using Cart.Api.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Cart.Api.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly CartDbContext _db;
        private readonly IPublishEndpoint _publish;

        public CartItemService(CartDbContext db, IPublishEndpoint publish)
        {
            _db = db;
            _publish = publish;
        }

        public async Task ChangeQuantityAsync(int cartItemId, int quantity, int userId)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(quantity);

            var basket = await _db.Carts.Include(i => i.Items).ThenInclude(p => p.Product).FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new Exception("Nie znaleziono koszyka");

            var cartItem = basket.Items.FirstOrDefault(i => i.Id == cartItemId)
                ?? throw new Exception("Nie znaleziono itemu");

            if (quantity == 0)
            {
                _db.CartItems.Remove(cartItem);
                await _db.SaveChangesAsync();
                await _publish.Publish(new BasketItemDeleted(cartItem.Product.ExternalId, cartItem.Quantity));
                return;
            }

            cartItem.Quantity = quantity;
            await _db.SaveChangesAsync();
            await _publish.Publish(new BasketItemQuantityChanged(cartItem.Product.ExternalId, cartItem.Quantity));
        }

        public async Task DeleteAsync(int cartItemId, int userId)
        {
            var basket = await _db.Carts.Include(i => i.Items).ThenInclude(p => p.Product).FirstOrDefaultAsync(c => c.UserId == userId)
               ?? throw new Exception("Nie znaleziono koszyka");

            var cartItem = basket.Items.FirstOrDefault(i => i.Id == cartItemId)
                ?? throw new Exception("Nie znaleziono itemu");

            _db.CartItems.Remove(cartItem);
            await _db.SaveChangesAsync();
            await _publish.Publish(new BasketItemDeleted(cartItem.Product.ExternalId, cartItem.Quantity));
        }
    }
}