namespace Gateway.Api.ApiModels.Access
{
    public class KsefChallengeResponse
    {
        /// <summary>
        /// Unikalny challenge.
        /// </summary>
        public string Challenge { get; set; } = null!;
        /// <summary>
        /// Czas wygenerowania challenge-a.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Czas wygenerowania challenge-a w milisekundach od 1 stycznia 1970 roku (Unix timestamp).
        /// </summary>
        public long TimestampMs { get; set; }
    }
}
