using Cart.Api.Data;
using Cart.Api.Models;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cart.Api.Consumers
{
    public class ProductCreatedConsumer : IConsumer<ProductCreated>
    {
        private readonly ILogger<ProductCreatedConsumer> _logger;
        private readonly CartDbContext _db;

        public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger, CartDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task Consume(ConsumeContext<ProductCreated> context)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ExternalId == context.Message.Id);

            if (product is not null)
            {
                _logger.LogInformation("Product with ExternalId={Id} already exists in Cart DB, skipping", context.Message.Id);
                return;
            }

            var model = new Product()
            {
                ExternalId = context.Message.Id,
                Name = context.Message.Name,
                Price = context.Message.Price,
                Stock = context.Message.Stock
            };

            await _db.Products.AddAsync(model);
            await _db.SaveChangesAsync();

            var msg = context.Message;
            _logger.LogInformation("Received ProductCreated: {Id} {Name} Price={Price} Stock={Stock}", msg.Id, msg.Name, msg.Price, msg.Stock);

        }
    }
}
