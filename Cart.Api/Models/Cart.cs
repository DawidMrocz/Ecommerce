namespace Cart.Api.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public decimal Total => Items.Sum(t => t.TotalPrice);
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = [];
    }
}
