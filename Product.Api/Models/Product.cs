namespace Product.Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsAvailable => Stock > 0;
        public int ? PhotoId { get; set; }
        public ProductPhoto? Photo { get; set; }
    }
}
