using System.Text.Json.Serialization;

namespace Consumer.Models;

public class FieldReport
{
    [JsonPropertyName("reportId")]
    public string? ReportId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; set; }

    [JsonPropertyName("agentId")]
    public string? AgentId { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("theater")]
    public string? Theater { get; set; }

    [JsonPropertyName("sector")]
    public string? Sector { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("reportType")]
    public ReportType? ReportType { get; set; }

    [JsonPropertyName("priority")]
    public ReportPriority? Priority { get; set; }

    [JsonPropertyName("sourceType")]
    public string? SourceType { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("subjectId")]
    public string? SubjectId { get; set; }

    [JsonPropertyName("subjectType")]
    public string? SubjectType { get; set; }
}