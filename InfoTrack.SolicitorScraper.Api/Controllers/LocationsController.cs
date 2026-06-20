using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationSettingsService _locationSettingsService;

    public LocationsController(ILocationSettingsService locationSettings)
    {
        _locationSettingsService = locationSettings;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> Get(CancellationToken cancellationToken)
    {
        var locations = await _locationSettingsService.GetLocationsAsync(cancellationToken);
        return Ok(locations);
    }

    [HttpPut]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<string>>> Update(
        [FromBody] UpdateLocationsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _locationSettingsService.UpdateLocationsAsync(request.Locations, cancellationToken);
            var locations = await _locationSettingsService.GetLocationsAsync(cancellationToken);
            return Ok(locations);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}