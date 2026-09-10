namespace Api.Models;

public class ReportsStatisticsDto
{
    public Dictionary<string, int> ByPriority { get; set; } = new();

    public Dictionary<string, int> ByReportType { get; set; } = new();

    public Dictionary<string, int> ByTheater { get; set; } = new();
}