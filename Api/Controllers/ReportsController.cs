using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            return NotFound(new
            {
                message = $"Report {reportId} was not found"
            });
        }
        return Ok(report);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<FieldReport>>> Search(string text,CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("Search text is required");
        }
        List<FieldReport> reports =await _reportService.SearchReportsAsync(text,cancellationToken);
        return Ok(reports);
    }
}