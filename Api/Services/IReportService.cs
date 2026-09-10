using Api.Models;

namespace Api.Services;

public interface IReportService
{
    Task<List<FieldReport>> GetAllReportsAsync(CancellationToken cancellationToken);
    Task<FieldReport?> GetReportByIdAsync(string reportId,CancellationToken cancellationToken);
    Task<List<FieldReport>> SearchReportsAsync(string text,CancellationToken cancellationToken);
    Task<List<FieldReport>> GetAllBySubjectId(string subjectId,CancellationToken cancellationToken);
    Task<List<FieldReport>> GetReportsAsync(string? theater,string? sector,string? location,string? priorities,DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken);
    Task<List<FieldReport>> SearchReportsAsync(string? Message,string? theater,string? sector,string? location,List<string>? priorities,string? reportType,DateTimeOffset? from,DateTimeOffset? to,CancellationToken cancellationToken);
}