using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Application.Dtos;
using InfoTrack.SolicitorScraper.Application.Mapping;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapeController : ControllerBase
{
    private readonly IScrapeOrchestrator _scrapeOrchestrator;

    public ScrapeController(IScrapeOrchestrator scrapeOrchestrator)
    {
        _scrapeOrchestrator = scrapeOrchestrator;
    }

    [HttpPost]
    [RequestTimeout("Scrape")]
    [ProducesResponseType(typeof(ScrapeJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Scrape(
        [FromBody] ScrapeRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var job = await _scrapeOrchestrator.RunAsync(
                request?.Locations,
                cancellationToken);

            return Ok(job.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse(ex.Message));
        }
    }
}