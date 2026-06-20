using InfoTrack.SolicitorScraper.Core.Entities;

namespace InfoTrack.SolicitorScraper.Application.Abstractions;

public interface ISolicitorScraper
{
    Task<IReadOnlyList<SolicitorContact>> ScrapeLocationAsync(
        string location,
        CancellationToken cancellationToken = default);
}