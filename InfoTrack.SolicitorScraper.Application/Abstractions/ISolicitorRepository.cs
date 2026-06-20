using InfoTrack.SolicitorScraper.Core.Entities;

namespace InfoTrack.SolicitorScraper.Application.Abstractions;

public interface ISolicitorRepository
{
    Task ReplaceForLocationAsync(
        string location,
        IReadOnlyList<SolicitorContact> contacts,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitorContact>> GetByLocationsAsync(
        IReadOnlyList<string>? locations,
        CancellationToken cancellationToken = default);

    Task DeleteExceptLocationsAsync(
        IReadOnlyList<string> locationsToKeep,
        CancellationToken cancellationToken = default);
}