    using System.Text.Json.Serialization;

namespace Consumer.Models;

public class FieldReportDocument
{
    [JsonPropertyName("reportId")]
    public string ReportId { get; set; } = string.Empty;

    [JsonPropertyName("@timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("agentId")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("theater")]
    public string Theater { get; set; } = string.Empty;

    [JsonPropertyName("sector")]
    public string Sector { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("reportType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReportType ReportType { get; set; }

    [JsonPropertyName("priority")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReportPriority Priority { get; set; }

    [JsonPropertyName("sourceType")]
    public string SourceType { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("subjectId")]
    public string? SubjectId { get; set; }

    [JsonPropertyName("subjectType")]
    public string? SubjectType { get; set; }

    [JsonPropertyName("processedAt")]
    public DateTimeOffset ProcessedAt { get; set; }
}