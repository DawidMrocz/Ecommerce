using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Product.Api.Data;

namespace Product.Api.Consumers;

public class CartItemQuantityChangedConsumer : IConsumer<BasketItemQuantityChanged>
{
    private readonly ProductDbContext _db;
    private readonly ILogger<CartItemQuantityChangedConsumer> _logger;

    public CartItemQuantityChangedConsumer(ProductDbContext db, ILogger<CartItemQuantityChangedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketItemQuantityChanged> context)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == context.Message.ProductId, context.CancellationToken)
                ?? throw new Exception("Nie znaleziono produktu");

            product.UpdatedAt = DateTime.UtcNow;
            product.Stock = context.Message.Quantity;
            await _db.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling BasketItemDeleted for ProductId={ProductId}", context.Message.ProductId);

        }
    }
}