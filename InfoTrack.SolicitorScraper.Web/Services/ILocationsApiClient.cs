namespace InfoTrack.SolicitorScraper.Web.Services;

public interface ILocationsApiClient
{
    Task<IReadOnlyList<string>> GetAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> UpdateAsync(IReadOnlyList<string> locations, CancellationToken cancellationToken = default);
}