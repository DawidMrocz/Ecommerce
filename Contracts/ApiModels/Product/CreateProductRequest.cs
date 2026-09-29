namespace Contracts.ApiModels.Product
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
