using Api.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Api.Services;

public class ReportService : IReportService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ReportService()
    {
        string elasticsearchUrl =Environment.GetEnvironmentVariable("ELASTICSEARCH_URL")
            ?? throw new InvalidOperationException("ELASTICSEARCH_URL is missing from .env");
        _indexName =Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX")
            ?? throw new InvalidOperationException("ELASTICSEARCH_INDEX is missing from .env");
        ElasticsearchClientSettings settings =new(new Uri(elasticsearchUrl));
        _client = new ElasticsearchClient(settings);
    }

    public async Task<List<FieldReport>> GetAllReportsAsync(CancellationToken cancellationToken)
    {
        var response =await _client.SearchAsync<FieldReport>(
            search => search
            .Index(_indexName)
            .Size(1000)
            ,cancellationToken
            );
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException("Failed to get reports from Elasticsearch");
        }
        return response.Documents.ToList();
    }

    public async Task<FieldReport?> GetReportByIdAsync(string reportId, CancellationToken cancellationToken)
    {
        var response =await _client.SearchAsync<FieldReport>(
            search => search
                    .Index(_indexName)
                    .Size(1)
                    .Query(query => query
                        .Term(term => term
                            .Field(field => field.ReportId)
                            .Value(reportId)
                        )
                    ),
                cancellationToken
            );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException("Failed to search report in Elasticsearch");
        }
        return response.Documents.FirstOrDefault();
    }

    public async Task<List<FieldReport>> SearchReportsAsync(string text,CancellationToken cancellationToken)
    {
        var response =await _client.SearchAsync<FieldReport>(
                search => search
                    .Index(_indexName)
                    .Size(100)
                    .Query(query => query
                        .Match(match => match
                            .Field(field => field.Message)
                            .Query(text)
                        )
                    ),

                cancellationToken
            );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException("Failed to search reports in Elasticsearch");
        }

        return response.Documents.ToList();
    }

    public async Task<List<FieldReport>> GetAllBySubjectId(string subjectId,CancellationToken cancellationToken)
    {
        var response =
            await _client.SearchAsync<FieldReport>(
                search => search
                    .Index(_indexName)
                    .Size(100)
                    .Query(query => query
                        .Term(term => term
                            .Field(field => field.SubjectId)
                            .Value(subjectId)
                        )
                    )
                    .Sort(sort => sort
                        .Field(
                            field => field.Timestamp,
                            new FieldSort
                            {
                                Order = SortOrder.Desc
                            }
                        )
                    ),

                cancellationToken
            );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                "Failed to search reports by subjectId"
            );
        }

        return response.Documents.ToList();
    }

    public async Task<List<FieldReport>> GetReportsAsync(string? theater,string? sector,string? location,string? priorities, DateTimeOffset? from,DateTimeOffset? to,CancellationToken cancellationToken)
    {
        List<Action<QueryDescriptor<FieldReport>>> filters = new();

        if (!string.IsNullOrWhiteSpace(theater))
        {
            filters.Add(query => query.Match(match => match.Field("theater").Query(theater)));
        }

        if (!string.IsNullOrWhiteSpace(sector))
        {
            filters.Add(query => query.Match(match => match.Field("sector").Query(sector)));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            filters.Add(query => query.Match(match => match.Field("location").Query(location)));
        }

        if (!string.IsNullOrWhiteSpace(priorities))
        {
            string[] priorityValues = priorities.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(priority => priority.Trim()).ToArray();

            List<Action<QueryDescriptor<FieldReport>>> priorityQueries = new();

            foreach (string priority in priorityValues)
            {
                priorityQueries.Add(query => query.Match(match => match.Field("priority").Query(priority)));
            }

            filters.Add(query => query.Bool(boolean => boolean.Should(priorityQueries.ToArray()).MinimumShouldMatch(1)));
        }

        if (from.HasValue || to.HasValue)
        {
            filters.Add(query => query
                .Range(range => range
                    .DateRange(dateRange =>
                    {
                        dateRange.Field("@timestamp");

                        if (from.HasValue)
                        {
                            dateRange.Gte(from.Value.ToString("O"));
                        }

                        if (to.HasValue)
                        {
                            dateRange.Lte(to.Value.ToString("O"));
                        }
                    })
                )
            );
        }

        var response = await _client.SearchAsync<FieldReport>(
            search =>
            {
                search
                    .Index(_indexName)
                    .Size(1000)
                    .Sort(sort => sort
                        .Field(
                            "@timestamp",
                            new FieldSort
                            {
                                Order = SortOrder.Desc
                            }
                        )
                    );

                if (filters.Count > 0)
                {
                    search.Query(query => query
                        .Bool(boolean => boolean
                            .Filter(filters.ToArray())
                        )
                    );
                }
            },
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to search reports: " +
                $"{response.ElasticsearchServerError?.Error?.Reason}"
            );
        }

        return response.Documents.ToList();
    }
    public async Task<List<FieldReport>> SearchReportsAsync(string? Message,string? theater,string? sector,string? location,List<string>? priorities,string? reportType,DateTimeOffset? from,DateTimeOffset? to,CancellationToken cancellationToken)
    {
        List<Action<QueryDescriptor<FieldReport>>> must = new();
        List<Action<QueryDescriptor<FieldReport>>> filters = new();
        if (!string.IsNullOrWhiteSpace(Message))
        {
            filters.Add(query => query.Match(match => match.Field("Message").Query(Message)));
        }
        if (!string.IsNullOrWhiteSpace(theater))
        {
            filters.Add(query => query.Term(match => match.Field("theater").Value(theater)));
        }
        if (!string.IsNullOrWhiteSpace(sector))
        {
            filters.Add(query => query.Term(match => match.Field("sector").Value(sector)));
        }
        if (!string.IsNullOrWhiteSpace(location))
        {
            filters.Add(query => query.Term(match => match.Field("location").Value(location)));
        }
        if (!string.IsNullOrWhiteSpace(reportType))
        {
            filters.Add(query => query.Term (match => match.Field("reportType").Value(reportType)));
        }
        if (priorities is not null && priorities.Count > 0)
        {
            filters.Add(query => query.Terms(terms => terms.Field(field => field.Priority).Term(new TermsQueryField(priorities.Select(priority =>FieldValue.String(priority)).ToArray()))));
        }
        if (from.HasValue || to.HasValue)
        {
            filters.Add(query => query.Range(range => range.DateRange(dateRange =>{dateRange.Field("@timestamp");if (from.HasValue){dateRange.Gte(from.Value.ToString("O"));}if (to.HasValue){dateRange.Lte(to.Value.ToString("O"));}})));
        }

        var response =
            await _client.SearchAsync<FieldReport>(
                search =>
                {
                    search
                        .Index(_indexName)
                        .Size(100)
                        .Sort(sort => sort
                            .Field(
                                field => field.Timestamp,
                                new FieldSort
                                {
                                    Order = SortOrder.Desc
                                }
                            )
                        );

                    if (must.Count > 0 || filters.Count > 0)
                    {
                        search.Query(query => query
                            .Bool(boolean =>
                            {
                                if (must.Count > 0)
                                {
                                    boolean.Must(
                                        must.ToArray()
                                    );
                                }

                                if (filters.Count > 0)
                                {
                                    boolean.Filter(
                                        filters.ToArray()
                                    );
                                }
                            })
                        );
                    }
                },

                cancellationToken
            );
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException("Failed to search reports");
        }
        return response.Documents.ToList();
    }
    
    public async Task<ReportsStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var response =await _client.SearchAsync<FieldReport>(
                search => search
                    .Index(_indexName)
                    .Size(0)
                    .Aggregations(aggregations => aggregations
                        .Add(
                            "by_priority",
                            aggregation => aggregation
                                .Terms(terms => terms
                                    .Field("priority")
                                    .Size(100)
                                )
                        )
                        .Add(
                            "by_report_type",
                            aggregation => aggregation
                                .Terms(terms => terms
                                    .Field("reportType")
                                    .Size(100)
                                )
                        )
                        .Add(
                            "by_theater",
                            aggregation => aggregation
                                .Terms(terms => terms
                                    .Field("theater")
                                    .Size(100)
                                )
                        )
                    ),
                cancellationToken
            );
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to get report statistics: " +$"{response.ElasticsearchServerError?.Error?.Reason}");
        }
        var priorityAggregation =response.Aggregations?.GetStringTerms("by_priority")?? throw new InvalidOperationException("Priority aggregation was not returned");
        var reportTypeAggregation =response.Aggregations?.GetStringTerms("by_report_type")?? throw new InvalidOperationException( "Report type aggregation was not returned" );
        var theaterAggregation = response.Aggregations?.GetStringTerms("by_theater")?? throw new InvalidOperationException("Theater aggregation was not returned");
        Dictionary<string, int> priorityCounts = new();
        foreach (var bucket in priorityAggregation.Buckets)
        {
            priorityCounts[bucket.Key.ToString()] =(int)bucket.DocCount;
        }
        Dictionary<string, int> reportTypeCounts = new();
        foreach (var bucket in reportTypeAggregation.Buckets)
        {
            reportTypeCounts[bucket.Key.ToString()] =(int)bucket.DocCount;
        }
        Dictionary<string, int> theaterCounts = new();
        foreach (var bucket in theaterAggregation.Buckets)
        {
            theaterCounts[bucket.Key.ToString()] =(int)bucket.DocCount;
        }
        ReportsStatisticsDto statistics = new()
        {
            ByPriority = priorityCounts,
            ByReportType = reportTypeCounts,
            ByTheater = theaterCounts
        };

        return statistics;
    }
}