using Api.Models;

namespace Api.Services;

public interface IReportService
{
    Task<List<FieldReport>> GetAllReportsAsync(CancellationToken cancellationToken);
    Task<FieldReport?> GetReportByIdAsync(string reportId,CancellationToken cancellationToken);
    Task<List<FieldReport>> SearchReportsAsync(string text,CancellationToken cancellationToken);
}