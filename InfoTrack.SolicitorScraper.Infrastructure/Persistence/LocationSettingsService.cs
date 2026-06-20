using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InfoTrack.SolicitorScraper.Infrastructure.Persistence;

public class LocationSettingsService : ILocationSettingsService
{
    private readonly AppDbContext _dbContext;
    private readonly ISolicitorRepository _solicitorRepository;
    private readonly IReadOnlyList<string> _defaultLocations;

    public LocationSettingsService(
        AppDbContext dbContext,
        ISolicitorRepository solicitorRepository,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _solicitorRepository = solicitorRepository;
        _defaultLocations = configuration
            .GetSection("DefaultLocations")
            .Get<string[]>() ?? [];
    }

    public async Task<IReadOnlyList<string>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _dbContext.LocationEntries
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        return locations;
    }

    public async Task UpdateLocationsAsync(
        IReadOnlyList<string> locations,
        CancellationToken cancellationToken = default)
    {
        ValidateLocations(locations);

        var existing = await _dbContext.LocationEntries.ToListAsync(cancellationToken);
        _dbContext.LocationEntries.RemoveRange(existing);

        var entries = locations
            .Select((name, index) => new LocationEntryEntity
            {
                Name = name.Trim(),
                SortOrder = index
            })
            .ToList();

        await _dbContext.LocationEntries.AddRangeAsync(entries, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _solicitorRepository.DeleteExceptLocationsAsync(
            entries.Select(x => x.Name).ToList(),
            cancellationToken);
    }

    public async Task SeedDefaultLocationsIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.LocationEntries.AnyAsync(cancellationToken))
        {
            return;
        }

        if (_defaultLocations.Count == 0)
        {
            return;
        }

        await UpdateLocationsAsync(_defaultLocations, cancellationToken);
    }

    private static void ValidateLocations(IReadOnlyList<string> locations)
    {
        if (locations is null || locations.Count == 0)
        {
            throw new ArgumentException("At least one location is required.", nameof(locations));
        }

        var normalized = locations
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        if (normalized.Count != locations.Count)
        {
            throw new ArgumentException("Locations cannot be empty or whitespace.", nameof(locations));
        }

        if (normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Count)
        {
            throw new ArgumentException("Duplicate locations are not allowed.", nameof(locations));
        }

        foreach (var location in normalized)
        {
            if (location.Any(c => !char.IsLetter(c) && c != ' ' && c != '-'))
            {
                throw new ArgumentException($"Invalid characters in location: {location}", nameof(locations));
            }
        }
    }
}