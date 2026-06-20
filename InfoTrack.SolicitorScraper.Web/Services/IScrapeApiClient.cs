using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public interface IScrapeApiClient
{
    Task<ScrapeJobResult> ScrapeAsync(
        IReadOnlyList<string> locations,
        CancellationToken cancellationToken = default);
}
