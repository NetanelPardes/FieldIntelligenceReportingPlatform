using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FieldReport>>> GetAll(CancellationToken cancellationToken)
    {
        List<FieldReport> reports =await _reportService.GetAllReportsAsync(cancellationToken);
        return Ok(reports);
    }

    [HttpGet("{reportId}")]
    public async Task<ActionResult<FieldReport>> GetById(string reportId,CancellationToken cancellationToken)
    {
        FieldReport? report =await _reportService.GetReportByIdAsync(reportId,cancellationToken);
        if (report is null)
        {
            return NotFound(new{message =$"Report {reportId} was not found"});
        }
        return Ok(report);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<FieldReport>>> Search([FromQuery] string text,CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("Search text is required");
        }
        List<FieldReport> reports =await _reportService.SearchReportsAsync(text,cancellationToken);
        return Ok(reports);
    }

    [HttpGet("by-subject/{subjectId}")]
    public async Task<ActionResult<List<FieldReport>>> GetBySubjectId(string subjectId,CancellationToken cancellationToken)
    {
        List<FieldReport> reports =await _reportService.GetAllBySubjectId(subjectId,cancellationToken);
        return Ok(reports);
    }
    
    [HttpGet("reports")]
    public async Task<ActionResult<List<FieldReport>>> GetReports([FromQuery] string? theater,[FromQuery] string? sector,[FromQuery] string? location,[FromQuery] string? priorities,[FromQuery] DateTimeOffset? from,[FromQuery] DateTimeOffset? to,CancellationToken cancellationToken)
    {
        List<FieldReport> reports =await _reportService.GetReportsAsync(theater,sector,location,priorities,from,to,cancellationToken);
        return Ok(reports);
    }

    [HttpGet("reports/search")]
    public async Task<ActionResult<List<FieldReport>>> SearchReportsAsync([FromQuery] string? text,[FromQuery] string? theater,[FromQuery] string? sector,[FromQuery] string? location,[FromQuery] List<string>? priorities,[FromQuery] string? reportType,[FromQuery] DateTimeOffset? from,[FromQuery] DateTimeOffset? to,CancellationToken cancellationToken)
    {
        List<FieldReport> reports =await _reportService.SearchReportsAsync(text,theater,sector,location,priorities,reportType,from,to,cancellationToken);
        return Ok(reports);
    }
    [HttpGet("statistics")]
    public async Task<ActionResult<ReportsStatisticsDto>> GetStatistics( CancellationToken cancellationToken)
    {
        ReportsStatisticsDto statistics =await _reportService.GetStatisticsAsync( cancellationToken);
        return Ok(statistics);
    }
}