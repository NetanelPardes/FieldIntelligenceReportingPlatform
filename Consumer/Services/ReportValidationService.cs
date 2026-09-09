using Consumer.Models;
using System.Text.Json;
namespace Consumer.Services;

public class ReportValidationService : IReportValidationService
{
    public bool IsValid(FieldReport report,string originalJson,out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(report.ReportId))
        {
            errorMessage = "reportId is missing";
            return false;
        }

        if (report.Timestamp is null)
        {
            errorMessage = "timestamp is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.AgentId))
        {
            errorMessage = "agentId is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.Unit))
        {
            errorMessage = "unit is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.Theater))
        {
            errorMessage = "theater is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.Sector))
        {
            errorMessage = "sector is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.Location))
        {
            errorMessage = "location is missing";
            return false;
        }

        if (report.ReportType is null)
        {
            errorMessage = "reportType is missing";
            return false;
        }

        if (report.Priority is null)
        {
            errorMessage = "priority is missing";
            return false;
        }
        using JsonDocument jsonDocument = JsonDocument.Parse(originalJson);

        JsonElement jsonReport = jsonDocument.RootElement;

        string originalPriority = jsonReport.GetProperty("priority").GetString()!;

        if (originalPriority != report.Priority.ToString())
        {
            errorMessage = $"priority '{originalPriority}' is not allowed";
            return false;
        }

        string originalReportType = jsonReport.GetProperty("reportType").GetString()!;

        if (originalReportType != report.ReportType.ToString())
        {
            errorMessage = $"reportType '{originalReportType}' is not allowed";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.SourceType))
        {
            errorMessage = "sourceType is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.Message))
        {
            errorMessage = "message is missing";
            return false;
        }

        bool hasSubjectId = !string.IsNullOrWhiteSpace(report.SubjectId);

        bool hasSubjectType = !string.IsNullOrWhiteSpace(report.SubjectType);

        if (hasSubjectId != hasSubjectType)
        {
            errorMessage = "subjectId and subjectType must appear together";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}