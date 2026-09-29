using Microsoft.EntityFrameworkCore;

namespace Product.Api.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Models.Product> Products => Set<Models.Product>();
    public DbSet<Models.ProductPhoto> ProductPhotos => Set<Models.ProductPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("product");
    }
}