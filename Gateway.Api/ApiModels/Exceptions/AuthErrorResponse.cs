using System.Text.Json.Serialization;

namespace Gateway.Api.ApiModels.Exceptions
{
    public class ErrorResponse
    {
        [JsonPropertyName("exception")]
        public ExceptionInfo? Exception { get; set; }
    }

    public class ExceptionInfo
    {
        [JsonPropertyName("exceptionDetailList")]
        public List<ExceptionDetail>? ExceptionDetailList { get; set; }

        [JsonPropertyName("referenceNumber")]
        public string? ReferenceNumber { get; set; }

        [JsonPropertyName("serviceCode")]
        public string? ServiceCode { get; set; }

        [JsonPropertyName("serviceCtx")]
        public string? ServiceCtx { get; set; }

        [JsonPropertyName("serviceName")]
        public string? ServiceName { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public class ExceptionDetail
    {
        [JsonPropertyName("exceptionCode")]
        public int ExceptionCode { get; set; }

        [JsonPropertyName("exceptionDescription")]
        public string? ExceptionDescription { get; set; }

        [JsonPropertyName("details")]
        public List<string>? Details { get; set; }
    }
}
