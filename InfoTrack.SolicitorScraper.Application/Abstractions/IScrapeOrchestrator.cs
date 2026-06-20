using InfoTrack.SolicitorScraper.Core.Entities;

namespace InfoTrack.SolicitorScraper.Application.Abstractions;

public interface IScrapeOrchestrator
{
    Task<ScrapeJob> RunAsync(
        IReadOnlyList<string>? locations,
        CancellationToken cancellationToken = default);
}