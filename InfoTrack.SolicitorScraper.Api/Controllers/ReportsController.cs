using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Application.Dtos;
using InfoTrack.SolicitorScraper.Application.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(SolicitorReportSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _reportService.BuildSummaryAsync(cancellationToken);
        return Ok(summary.ToDto());
    }
}