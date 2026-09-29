namespace MroczwareFramework.ApiModels
{
    public class SearchRequest
    {
        public int? Page { get; set; }

        public int? PageSize { get; set; }

        public string? OrderBy { get; set; }

        public int? OrderDir { get; set; }
    }
}
