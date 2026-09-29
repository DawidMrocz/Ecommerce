namespace Identity.Api.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid Guid { get; set; }
    }
}
