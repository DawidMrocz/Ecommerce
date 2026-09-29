namespace Contracts.ApiModels.Product
{
    public class GetProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsAvailable => Stock > 0;
    }
}
