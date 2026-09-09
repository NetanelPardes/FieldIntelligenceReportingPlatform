using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Models;

namespace Api.Services;

public class ReportService : IReportService
{
    private readonly HttpClient _httpClient;
    private readonly string _indexName;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReportService(HttpClient httpClient)
    {
        string elasticsearchUrl = 
        Environment.GetEnvironmentVariable("ELASTICSEARCH_URL")
            ?? throw new InvalidOperationException("ELASTICSEARCH_URL is missing from .env");
        _indexName =
            Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX")
            ?? throw new InvalidOperationException("ELASTICSEARCH_INDEX is missing from .env");
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(elasticsearchUrl.TrimEnd('/') + "/");
        _jsonOptions = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true};
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<List<FieldReport>> GetAllReportsAsync(CancellationToken cancellationToken)
    {
        HttpResponseMessage response =await _httpClient.GetAsync($"{_indexName}/_search?size=1000",cancellationToken);
        response.EnsureSuccessStatusCode();
        string json =await response.Content.ReadAsStringAsync(cancellationToken);
        using JsonDocument document =JsonDocument.Parse(json);
        JsonElement hits = document.RootElement
            .GetProperty("hits")
            .GetProperty("hits");
        List<FieldReport> reports = new();
        foreach (JsonElement hit in hits.EnumerateArray())
        {
            JsonElement source = hit.GetProperty("_source");
            FieldReport? report =source.Deserialize<FieldReport>(_jsonOptions);
            if (report is not null)
            {
                reports.Add(report);
            }
        }
        return reports;
    }

    public async Task<FieldReport?> GetReportByIdAsync(string reportId,CancellationToken cancellationToken)
    {
        string query = JsonSerializer.Serialize(new{size = 1, query = new{match = new{reportId = reportId}}});
        using StringContent content = new(query, Encoding.UTF8,"application/json");
        HttpResponseMessage response =await _httpClient.PostAsync($"{_indexName}/_search",content,cancellationToken);
        response.EnsureSuccessStatusCode();
        string json =await response.Content.ReadAsStringAsync(cancellationToken);
        using JsonDocument document =JsonDocument.Parse(json);
        JsonElement hits = document.RootElement
            .GetProperty("hits")
            .GetProperty("hits");
        foreach (JsonElement hit in hits.EnumerateArray())
        {
            JsonElement source =hit.GetProperty("_source");
            return source.Deserialize<FieldReport>(_jsonOptions);
        }
        return null;
    }

    public async Task<List<FieldReport>> SearchReportsAsync(string text,CancellationToken cancellationToken)
    {
        string searchText = Uri.EscapeDataString(text);
        HttpResponseMessage response =
            await _httpClient.GetAsync($"{_indexName}/_search?q={searchText}&size=100",cancellationToken);
        response.EnsureSuccessStatusCode();
        string json =await response.Content.ReadAsStringAsync(cancellationToken);
        using JsonDocument document =JsonDocument.Parse(json);
        JsonElement hits = document.RootElement.GetProperty("hits").GetProperty("hits");
        List<FieldReport> reports = new();
        foreach (JsonElement hit in hits.EnumerateArray())
        {
            FieldReport? report = hit.GetProperty("_source").Deserialize<FieldReport>(_jsonOptions);
            if (report is not null)
            {
                reports.Add(report);
            }
        }
        return reports;
    }
}