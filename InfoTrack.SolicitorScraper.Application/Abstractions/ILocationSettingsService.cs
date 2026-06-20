namespace InfoTrack.SolicitorScraper.Application.Abstractions;

public interface ILocationSettingsService
{
    Task<IReadOnlyList<string>> GetLocationsAsync(CancellationToken cancellationToken = default);
    Task UpdateLocationsAsync(IReadOnlyList<string> locations, CancellationToken cancellationToken = default);
    Task SeedDefaultLocationsIfEmptyAsync(CancellationToken cancellationToken = default);
}