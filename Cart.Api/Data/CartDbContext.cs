using Cart.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Cart.Api.Data;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options) { }

    public DbSet<Models.Cart> Carts => Set<Models.Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("cart");
    }
}