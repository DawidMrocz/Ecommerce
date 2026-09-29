using Cart.Api.Data;
using Cart.Api.Models;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Cart.Api.Consumers
{
    public class AddToBasketConsumer : IConsumer<AddToBasket>
    {
        private readonly ILogger<AddToBasketConsumer> _logger;
        private readonly CartDbContext _db;

        public AddToBasketConsumer(ILogger<AddToBasketConsumer> logger, CartDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task Consume(ConsumeContext<AddToBasket> context)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.ExternalId == context.Message.Id, context.CancellationToken)
                ?? throw new InvalidOperationException(
                    $"Produkt {context.Message.Id} nie istnieje");

            // Sprawdź stock
            if (product.Stock < context.Message.Quantity)
                throw new InvalidOperationException(
                    $"Brak wystarczającego stanu magazynowego dla produktu {product.ExternalId}");

            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == context.Message.UserId, context.CancellationToken);

            if (cart == null)
            {
                cart = new Models.Cart { UserId = context.Message.UserId };
                _db.Carts.Add(cart);
            }

            var cartItem = await _db.CartItems
                .FirstOrDefaultAsync(
                    ci => ci.Basket.UserId == context.Message.UserId
                       && ci.ProductId == product.Id,
                    context.CancellationToken);

            if (cartItem != null)
            {
                cartItem.Quantity += context.Message.Quantity;
            }
            else
            {
                _db.CartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = context.Message.Quantity,
                    Basket = cart
                });
            }

            product.Stock -= context.Message.Quantity;

            await _db.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Added product {ProductId} x{Quantity} to cart of user {UserId}",
                product.Id,
                context.Message.Quantity,
                context.Message.UserId);
        }

    }
}
