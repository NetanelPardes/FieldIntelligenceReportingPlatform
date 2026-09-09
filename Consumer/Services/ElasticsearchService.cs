using Consumer.Models;
using Elastic.Clients.Elasticsearch;

namespace Consumer.Services;

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchService(
        string elasticsearchUrl,
        string indexName)
    {
        _indexName = indexName;

        ElasticsearchClientSettings settings =
            new(
                new Uri(elasticsearchUrl)
            );

        _client = new ElasticsearchClient(settings);
    }

    public async Task CreateIndexIfNotExistsAsync(
        CancellationToken cancellationToken)
    {
        var existsResponse =
            await _client.Indices.ExistsAsync(
                _indexName,
                cancellationToken
            );

        if (existsResponse.Exists)
        {
            return;
        }

        var createResponse =
            await _client.Indices
                .CreateAsync<FieldReportDocument>(
                    index => index
                        .Index(_indexName)
                        .Mappings(mapping => mapping
                            .Properties(properties =>
                                properties
                                    .Keyword("reportId")
                                    .Date("@timestamp")
                                    .Keyword("agentId")
                                    .Keyword("unit")
                                    .Keyword("theater")
                                    .Keyword("sector")
                                    .Keyword("location")
                                    .Keyword("reportType")
                                    .Keyword("priority")
                                    .Keyword("sourceType")
                                    .Text("message")
                                    .Keyword("subjectId")
                                    .Keyword("subjectType")
                                    .Date("processedAt")
                            )
                        ),
                    cancellationToken
                );

        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                "Failed to create Elasticsearch index"
            );
        }
    }

    public async Task<bool> ReportExistsAsync(
        string reportId,
        CancellationToken cancellationToken)
    {
        var response =
            await _client.GetAsync<FieldReportDocument>(
                reportId,
                request => request.Index(_indexName),
                cancellationToken
            );

        return response.Found;
    }

    public async Task SaveReportAsync(
        FieldReportDocument report,
        CancellationToken cancellationToken)
    {
        var response =
            await _client.IndexAsync(
                report,
                request => request
                    .Index(_indexName)
                    .Id(report.ReportId),
                cancellationToken
            );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to save report {report.ReportId} " +
                "in Elasticsearch"
            );
        }
    }
}