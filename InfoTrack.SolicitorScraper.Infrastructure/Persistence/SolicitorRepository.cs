using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Core.Entities;
using InfoTrack.SolicitorScraper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.SolicitorScraper.Infrastructure.Persistence;

public class SolicitorRepository : ISolicitorRepository
{
    private readonly AppDbContext _dbContext;

    public SolicitorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ReplaceForLocationAsync(
        string location,
        IReadOnlyList<SolicitorContact> contacts,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.SolicitorContacts
            .Where(x => x.Location == location)
            .ToListAsync(cancellationToken);

        _dbContext.SolicitorContacts.RemoveRange(existing);

        var entities = contacts.Select(ToEntity).ToList();
        await _dbContext.SolicitorContacts.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SolicitorContact>> GetByLocationsAsync(
        IReadOnlyList<string>? locations,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SolicitorContacts.AsNoTracking();

        if (locations is { Count: > 0 })
        {
            var normalized = locations
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            query = query.Where(x => normalized.Contains(x.Location));
        }

        var entities = await query
            .OrderBy(x => x.Location)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDomain).ToList();
    }

    public async Task DeleteExceptLocationsAsync(
        IReadOnlyList<string> locationsToKeep,
        CancellationToken cancellationToken = default)
    {
        var keep = locationsToKeep
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        if (keep.Count == 0)
        {
            return;
        }

        var toRemove = await _dbContext.SolicitorContacts
            .Where(x => !keep.Contains(x.Location))
            .ToListAsync(cancellationToken);

        if (toRemove.Count == 0)
        {
            return;
        }

        _dbContext.SolicitorContacts.RemoveRange(toRemove);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SolicitorContactEntity ToEntity(SolicitorContact contact) =>
        new()
        {
            Id = contact.Id,
            Name = contact.Name,
            Location = contact.Location,
            Phone = contact.Phone,
            Address = contact.Address,
            Description = contact.Description,
            WebsiteUrl = contact.WebsiteUrl,
            EmailUrl = contact.EmailUrl,
            ProfileUrl = contact.ProfileUrl,
            ReviewCount = contact.ReviewCount,
            ScrapedDate = contact.ScrapedDate
        };

    private static SolicitorContact ToDomain(SolicitorContactEntity entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Location = entity.Location,
            Phone = entity.Phone,
            Address = entity.Address,
            Description = entity.Description,
            WebsiteUrl = entity.WebsiteUrl,
            EmailUrl = entity.EmailUrl,
            ProfileUrl = entity.ProfileUrl,
            ReviewCount = entity.ReviewCount,
            ScrapedDate = entity.ScrapedDate
        };
}