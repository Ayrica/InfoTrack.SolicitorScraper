namespace InfoTrack.SolicitorScraper.Infrastructure.Persistence.Entities;

public class SolicitorContactEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? EmailUrl { get; set; }
    public string? ProfileUrl { get; set; }
    public int? ReviewCount { get; set; }
    public DateTime ScrapedDate { get; set; }
}