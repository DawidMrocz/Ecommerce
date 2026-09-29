namespace Gateway.Api.ApiModels.Exceptions
{
    public class KsefRateLimitResponse
    {
        public KsefStatus Status { get; set; } = null!;
    }

    public class KsefStatus
    {
        public int Code { get; set; }
        public string Description { get; set; } = null!;
        public List<string> Details { get; set; } = [];
    }

}
