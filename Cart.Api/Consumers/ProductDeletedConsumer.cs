using Cart.Api.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Cart.Api.Consumers
{
    public class ProductDeletedConsumer : IConsumer<ProductDeleted>
    {
        private readonly CartDbContext _db;
        private readonly ILogger<ProductDeletedConsumer> _logger;

        public ProductDeletedConsumer(CartDbContext db, ILogger<ProductDeletedConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductDeleted> context)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(b => b.ExternalId == context.Message.Id)
                ?? throw new InvalidOperationException($"Produkt o zewnętrznym Id {context.Message.Id} nie istnieje w bazie koszyka.");

            var cartItems = await _db.CartItems.Where(ci => ci.ProductId == product.Id).ToListAsync();

            if (cartItems.Count > 0)
                _db.CartItems.RemoveRange(cartItems);

            _db.Products.Remove(product);

            await _db.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Usunięto {CartItemCount} pozycji koszyka oraz produkt {ProductId}",
                cartItems.Count,
                product.Id
            );
        }
    }
}
