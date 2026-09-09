using Consumer.Models;

namespace Consumer.Services;

public interface IReportValidationService
{
    bool IsValid(FieldReport report,string originalJson,out string errorMessage);
}