using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Application.Dtos;
using InfoTrack.SolicitorScraper.Application.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace InfoTrack.SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitorsController : ControllerBase
{
    private readonly ISolicitorRepository _solicitorRepository;

    public SolicitorsController(ISolicitorRepository solicitorRepository)
    {
        _solicitorRepository = solicitorRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SolicitorContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? locations,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string>? locationFilter = null;
        if (!string.IsNullOrWhiteSpace(locations))
        {
            locationFilter = locations
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        var contacts = await _solicitorRepository.GetByLocationsAsync(
            locationFilter,
            cancellationToken);

        return Ok(contacts.Select(DtoMapper.ToDto));
    }
}