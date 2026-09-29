namespace Product.Api.ApiModel
{
    public class AddProductPhotoRequest
    {
        public IFormFile Photo { get; set; } = null!;
    }
}
