namespace Cart.Api.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => decimal.Round(Product.Price * Quantity, 2);
        public int BasketId { get; set; }
        public Cart Basket { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
