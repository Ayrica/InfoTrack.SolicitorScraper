using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace InfoTrack.SolicitorScraper.Application.Services;

public class ScrapeOrchestrator : IScrapeOrchestrator
{
    private readonly ISolicitorScraper _scraper;
    private readonly ISolicitorRepository _solicitorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocationSettingsService _locationSettingsService;
    private readonly ILogger<ScrapeOrchestrator> _logger;

    public ScrapeOrchestrator(
        ISolicitorScraper scraper,
        ISolicitorRepository solicitorRepository,
        IUnitOfWork unitOfWork,
        ILocationSettingsService locationSettingsService,
        ILogger<ScrapeOrchestrator> logger)
    {
        _scraper = scraper;
        _solicitorRepository = solicitorRepository;
        _unitOfWork = unitOfWork;
        _locationSettingsService = locationSettingsService;
        _logger = logger;
    }

    public async Task<ScrapeJob> RunAsync(
        IReadOnlyList<string>? locations,
        CancellationToken cancellationToken = default)
    {
        var targetLocations = locations is { Count: > 0 }
            ? locations
            : await _locationSettingsService.GetLocationsAsync(cancellationToken);

        if (targetLocations.Count == 0)
        {
            throw new InvalidOperationException("No locations configured for scraping.");
        }

        var job = new ScrapeJob
        {
            Locations = targetLocations.ToList(),
            Status = ScrapeJobStatus.Running
        };

        var failedLocations = new List<string>();
        var totalFound = 0;

        foreach (var location in targetLocations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation("Scraping solicitors for {Location}", location);
                var contacts = await _scraper.ScrapeLocationAsync(location, cancellationToken);

                await _unitOfWork.ExecuteInTransactionAsync(
                    ct => _solicitorRepository.ReplaceForLocationAsync(location, contacts, ct),
                    cancellationToken);

                totalFound += contacts.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scrape location {Location}", location);
                failedLocations.Add(location);
            }
        }

        job.TotalSolicitorsFound = totalFound;
        job.FailedLocations = failedLocations;
        job.EndDate = DateTime.UtcNow;
        job.Status = failedLocations.Count switch
        {
            0 => ScrapeJobStatus.Completed,
            _ when failedLocations.Count == targetLocations.Count => ScrapeJobStatus.Failed,
            _ => ScrapeJobStatus.PartiallyCompleted
        };

        if (job.Status == ScrapeJobStatus.Failed)
        {
            job.ErrorMessage = "All locations failed to scrape.";
        }

        return job;
    }
}