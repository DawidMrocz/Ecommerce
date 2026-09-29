using Newtonsoft.Json;

namespace MroczwareFramework.ApiModels
{
    public class SearchItem
    {
        [JsonIgnore]
        public int TotalRows { get; set; }
    }
}
