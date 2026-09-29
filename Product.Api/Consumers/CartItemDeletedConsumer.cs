using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Product.Api.Data;

namespace Product.Api.Consumers;

public class CartItemDeletedConsumer : IConsumer<BasketItemDeleted>
{
    private readonly ProductDbContext _db;
    private readonly ILogger<CartItemDeletedConsumer> _logger;

    public CartItemDeletedConsumer(ProductDbContext db, ILogger<CartItemDeletedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketItemDeleted> context)
    {
        try
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == context.Message.ProductId, context.CancellationToken)
                ?? throw new Exception("Nie znaleziono produktu");

            product.UpdatedAt = DateTime.UtcNow;
            product.Stock += context.Message.Quantity;
            await _db.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling BasketItemDeleted for ProductId={ProductId}", context.Message.ProductId);
            throw;
        }
    }
}