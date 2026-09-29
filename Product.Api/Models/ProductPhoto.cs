namespace Product.Api.Models
{
    public class ProductPhoto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid Guid { get; set; }
    }
}
