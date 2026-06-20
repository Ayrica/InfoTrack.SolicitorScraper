using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public interface ISolicitorsApiClient
{
    Task<IReadOnlyList<SolicitorContact>> GetAsync(
        IReadOnlyList<string>? locations = null,
        CancellationToken cancellationToken = default);
}