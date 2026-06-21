using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Core.Entities;

namespace InfoTrack.SolicitorScraper.Application.Services;

public class ReportService : IReportService
{
    private readonly ISolicitorRepository _solicitorRepository;
    private readonly ILocationSettingsService _locationSettingsService;

    public ReportService(
        ISolicitorRepository solicitorRepository,
        ILocationSettingsService locationSettingsService)
    {
        _solicitorRepository = solicitorRepository;
        _locationSettingsService = locationSettingsService;
    }

    public async Task<SolicitorReportSummary> BuildSummaryAsync(CancellationToken cancellationToken = default)
    {
        var configuredLocations = await _locationSettingsService.GetLocationsAsync(cancellationToken);
        var contacts = await _solicitorRepository.GetByLocationsAsync(
            configuredLocations,
            cancellationToken);

        var countByLocation = contacts
            .GroupBy(c => c.Location, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var locationCounts = configuredLocations
            .Select(location => new LocationCount
            {
                Location = location,
                Count = countByLocation.TryGetValue(location, out var count) ? count : 0
            })
            .ToList();

        var byLocation = locationCounts
            .OrderByDescending(x => x.Count)
            .ToList();

        var coverageGaps = locationCounts
            .Where(x => x.Count == 0)
            .Select(x => new LocationCoverage { Location = x.Location, Count = x.Count })
            .ToList();

        var topByReview = contacts
            .Where(c => c.ReviewCount.HasValue)
            .OrderByDescending(c => c.ReviewCount)
            .Take(10)
            .ToList();

        return new SolicitorReportSummary
        {
            TotalSolicitors = contacts.Count,
            LastScrapedDate = contacts.Count > 0
                ? contacts.Max(c => c.ScrapedDate)
                : null,
            ByLocation = byLocation,
            TopByReviewCount = topByReview,
            CoverageGaps = coverageGaps,
            SolicitorsWithoutPhone = contacts.Count(c => string.IsNullOrWhiteSpace(c.Phone)),
            SolicitorsWithoutWebsite = contacts.Count(c => string.IsNullOrWhiteSpace(c.WebsiteUrl))
        };
    }
}