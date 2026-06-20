namespace InfoTrack.SolicitorScraper.Core.Entities;

public class SolicitorContact
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Location { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Description { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? EmailUrl { get; init; }
    public string? ProfileUrl { get; init; }
    public int? ReviewCount { get; init; }
    public DateTime ScrapedDate { get; init; } = DateTime.UtcNow;
}