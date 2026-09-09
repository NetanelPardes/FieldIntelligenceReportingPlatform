using Consumer.Models;

namespace Consumer.Services;

public interface IElasticsearchService
{
    Task CreateIndexIfNotExistsAsync(CancellationToken cancellationToken);
    Task<bool> ReportExistsAsync(string reportId,CancellationToken cancellationToken);
    Task SaveReportAsync(FieldReportDocument report,CancellationToken cancellationToken);
}