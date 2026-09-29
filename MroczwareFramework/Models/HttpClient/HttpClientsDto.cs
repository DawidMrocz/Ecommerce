namespace MroczwareFramework.Models.HttpClient
{
    internal class HttpClientsDto
    {
        public List<HttpClientItemDto> HttpClients { get; set; } = [];
    }

    internal class HttpClientItemDto
    {
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? Certificate { get; set; }
        public string? Password { get; set; }

    }
}
