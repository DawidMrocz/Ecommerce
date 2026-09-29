namespace Contracts.ApiModels.Basket
{
    public class CartItemResponseDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int BasketId { get; set; }
        public int ProductId { get; set; }
    }
}
